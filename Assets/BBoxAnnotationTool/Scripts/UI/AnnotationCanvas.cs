using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EasyGUI;

namespace BBoxAnnotationTool
{
    public class AnnotationCanvas
    {
        Vector3 prevMousePosition_ = Vector3.zero;
        Matrix4x4 canvasMatrix_ = Matrix4x4.identity;
        Rect prevRawImageRect_ = Rect.zero;
        int focusedPointIndex_ = -1;
        BoundingBox bboxOnClipBoard_ = null;
        BoundingBox editBBox_ = new BoundingBox();
        Vector2 editStartPos_;
        Rect preRect_;

        enum EditMode
        {
            None,
            Creating,
            MovingPoint,
        }

        EditMode editMode_ = EditMode.None;

        //
        // Setup/Reset
        //

        public AnnotationCanvas()
        {
        }

        public void ResetMatrix()
        {
            canvasMatrix_ = Matrix4x4.identity;
            prevRawImageRect_ = Rect.zero;
        }

        //
        // Shortcut
        //

        public void OnShortcutKey(Rect region, BBoxAnnotation annot, BBoxAnnotationSetting setting)
        {
            var mousePosition2D = to2D(Input.mousePosition, true);
            if (false == region.Contains(mousePosition2D))
            {
                return;
            }

            var focusedBBox = getFocusedBBox(annot);

            // C: Copy bbox
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (focusedBBox != null)
                {
                    bboxOnClipBoard_ = BoundingBox.Copy(focusedBBox);
                    Debug.Log("Copied skeleton" + focusedBBox.Name);
                }
            }

            // V: Paste bbox
            if (Input.GetKeyDown(KeyCode.V))
            {
                // toggle joint hide/show
                if (bboxOnClipBoard_ != null)
                {
                    var newBBox = BoundingBox.Copy(bboxOnClipBoard_);
                    UndoRedoManager.Instance.AddBBox(annot, newBBox);
                    Debug.Log("Pasted skeleton: " + newBBox.Name);
                }
            }

            // N: Delete bbox
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (focusedBBox != null)
                {
                    string removedBBoxName = focusedBBox.Name;
                    UndoRedoManager.Instance.RemoveBBox(annot, focusedBBox);
                    annot.Unfocus();
                    focusedPointIndex_ = -1;
                    Debug.Log("Deleted skeleton: " + removedBBoxName);
                }
            }

            // Return: Change BBox Color
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (focusedBBox != null)
                {
                    setting.ChangeShowColor(focusedBBox.Label);
                    Debug.Log("Change Color of " + focusedBBox.Label);
                }
            }
        }

        //
        // Update
        //

        public void Update(Rect region, GameObject imgObj, BBoxAnnotation annotentation, BBoxAnnotationSetting setting)
        {
            string currentLabel = "";
            if (dropdown_)
            {
                var ddComp = dropdown_.GetComponent<UnityEngine.UI.Dropdown>();
                if (ddComp)
                {
                    currentLabel = ddComp.captionText.text;
                }
            }

            var mousePosition = Input.mousePosition;
            Rect imgRegion, prefRegion;
            calcRect(region, out imgRegion, out prefRegion);

            updateCanvasMatrix(imgRegion, mousePosition, imgObj);
            updateBBox(imgRegion, mousePosition, annotentation, setting, currentLabel);
            prevMousePosition_ = mousePosition;
        }

        void updateCanvasMatrix(Rect region, Vector3 mousePosition, GameObject imgObj)
        {
            var mousePosition2D = to2D(mousePosition, true);
            var prevMousePosition2D = to2D(prevMousePosition_, true);

            var mouseScroll = Input.mouseScrollDelta;
            if (region.Contains(mousePosition2D))
            {
                // Pan
                if (Input.GetMouseButton(2))
                {
                    var delta = mousePosition2D - prevMousePosition2D;
                    canvasMatrix_ = Matrix4x4.Translate(delta) * canvasMatrix_;
                }

                // Zoom
                if (mouseScroll.y != 0)
                {
                    float scale = canvasMatrix_.m00;
                    float newScale = scale * Mathf.Pow(1.04f, mouseScroll.y);
                    newScale = Mathf.Clamp(newScale, 0.1f, 5.0f);
                    float deltaScale = newScale / (1e-8f + scale);
                    canvasMatrix_ = Matrix4x4.Translate(-mousePosition2D) * canvasMatrix_;
                    canvasMatrix_ = Matrix4x4.Scale(Vector3.one * deltaScale) * canvasMatrix_;
                    canvasMatrix_ = Matrix4x4.Translate(mousePosition2D) * canvasMatrix_;
                }
            }

            var rawImgRect = calculateImageObjectRect(region, imgObj);
            if (rawImgRect != prevRawImageRect_)
            {
                float spriteHeight = getSpriteRect(imgObj).height;
                float scale = rawImgRect.height / spriteHeight;
                if (prevRawImageRect_ == Rect.zero)
                {
                    canvasMatrix_ = Matrix4x4.Translate(rawImgRect.position) * Matrix4x4.Scale(new Vector3(scale, scale, 1));
                }
                else
                {
                    float prevscale = prevRawImageRect_.height / spriteHeight;
                    float ds = scale / prevscale;
                    canvasMatrix_ = Matrix4x4.Translate(-prevRawImageRect_.position) * canvasMatrix_;
                    canvasMatrix_ = Matrix4x4.Scale(new Vector3(ds, ds, 1)) * canvasMatrix_;
                    canvasMatrix_ = Matrix4x4.Translate(rawImgRect.position) * canvasMatrix_;
                }
                prevRawImageRect_ = rawImgRect;
            }
        }

        void updateBBox(Rect region, Vector3 mousePosition, BBoxAnnotation annotentation, BBoxAnnotationSetting setting, string currentLabel)
        {
            var mousePosition2D = to2D(mousePosition, true);

            if (region.Contains(mousePosition2D))
            {
                var focusedBBox = updateFocus(mousePosition2D, annotentation);
                var newPos = to2D(canvasMatrix_.inverse.MultiplyPoint(mousePosition2D));
                editBBoxs(focusedBBox, newPos, annotentation, currentLabel);
            }
        }

        BoundingBox updateFocus(Vector2 mousePosition2D, BBoxAnnotation annot)
        {
            int focusBBoxIndex;
            int focusPointIndex;
            bool found = searchBBoxNearPoint(annot, mousePosition2D, out focusBBoxIndex, out focusPointIndex);

            // Update focus if the user is not moving a joint.
            if (editMode_ == EditMode.None)
            {
                if (found)
                {
                    annot.SetFocus(focusBBoxIndex);
                    focusedPointIndex_ = focusPointIndex;
                }
                else
                {
                    annot.Unfocus();
                    focusedPointIndex_ = -1;
                }
            }

            var focusedBBox = getFocusedBBox(annot);
            return focusedBBox;
        }

        void editBBoxs(BoundingBox focusedBBox, Vector2 newPos, BBoxAnnotation annot, string currentLabel)
        {
            if (GameObject.Find("Dropdown List"))
            {
                return;
            }

            switch (editMode_)
            {
                case EditMode.None:
                    editMode_ = editModeNone(focusedBBox, newPos, annot, currentLabel);
                    break;
                case EditMode.Creating:
                    editMode_ = editModeCreating(focusedBBox, newPos, annot, currentLabel);
                    break;
                case EditMode.MovingPoint:
                    editMode_ = editModeMovingPoints(focusedBBox, newPos, annot, currentLabel);
                    break;
            }
        }

        EditMode editModeNone(BoundingBox focusedBBox, Vector2 newPos, BBoxAnnotation annot, string currentLabel)
        {
            if (focusedBBox == null && Input.GetMouseButtonDown(0))
            {
                editStartPos_ = newPos;
                editBBox_.Label = currentLabel;
                return EditMode.Creating;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var points = focusedBBox.Corners();
                if (focusedBBox != null && 0 <= focusedPointIndex_ && focusedPointIndex_ < points.Count)
                {
                    preRect_ = focusedBBox.Rect;
                    editStartPos_ = points[(focusedPointIndex_ + 2) % points.Count];
                    return EditMode.MovingPoint;
                }
            }

            return EditMode.None;
        }

        Rect points2Rect(Vector2 p1, Vector2 p2)
        {
            float xMin = Mathf.Min(p1.x, p2.x);
            float xMax = Mathf.Max(p1.x, p2.x);
            float yMin = Mathf.Min(p1.y, p2.y);
            float yMax = Mathf.Max(p1.y, p2.y);
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        EditMode editModeCreating(BoundingBox focusedBBox, Vector2 newPos, BBoxAnnotation annot, string currentLabel)
        {
            editBBox_.Rect = points2Rect(newPos, editStartPos_);

            if (Input.GetMouseButtonDown(0))
            {
                if (!editBBox_.Empty)
                {
                    UndoRedoManager.Instance.AddBBox(annot, BoundingBox.Copy(editBBox_));
                }
                editBBox_.Clear();
                return EditMode.None;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                editBBox_.Clear();
                return EditMode.None;
            }

            return EditMode.Creating;
        }

        EditMode editModeMovingPoints(BoundingBox focusedBBox, Vector2 newPos, BBoxAnnotation annot, string currentLabel)
        {
            var points = focusedBBox != null ? focusedBBox.Corners() : new List<Vector2>();
            if (Input.GetMouseButton(0) == false)
            {
                if (focusedBBox != null && 0 <= focusedPointIndex_ && focusedPointIndex_ < points.Count)
                {
                    UndoRedoManager.Instance.ChangeBBoxRect(focusedBBox, preRect_, focusedBBox.Rect);
                }
                return EditMode.None;
            }

            if (Input.GetMouseButton(0))
            {
                if (0 <= focusedPointIndex_ && focusedPointIndex_ < points.Count)
                {
                    var tmpRect = focusedBBox.Rect;
                    focusedBBox.Rect = points2Rect(newPos, editStartPos_);
                }
            }

            return EditMode.MovingPoint;
        }
        //
        // Draw
        //

        public void calcRect(Rect region, out Rect imgRegion, out Rect prefRegion)
        {
            prefRegion = region;
            prefRegion.yMax = prefRegion.yMin + 40;

            imgRegion = region;
            imgRegion.yMin += 40;
        }

        public void Draw(Rect region, GameObject imgObj, BBoxAnnotation annotentation, BBoxAnnotationSetting setting)
        {
            if (annotentation == null)
            {
                return;
            }

            Rect imgRegion, prefRegion;
            calcRect(region, out imgRegion, out prefRegion);

            drawPreferences(prefRegion, setting);

            var oldMatrix = DUI.matrix;
            DUI.matrix = canvasMatrix_;
            moveImageObject(imgRegion, imgObj);
            drawBBoxList(imgRegion, annotentation, setting);
            drawBBox(imgRegion, editBBox_, setting, true, Color.white);
            DUI.matrix = oldMatrix;
        }

        GameObject dropdown_ = null;
        GameObject prefBG_ = null;
        void drawPreferences(Rect region, BBoxAnnotationSetting setting)
        {
            if (prefBG_ == null)
            {
                GameObject prefab = Resources.Load("Prefabs/PreferenceBG") as GameObject;
                var canvas = GameObject.Find("Canvas");
                prefBG_ = GameObject.Instantiate(prefab, canvas.transform);

                var imgComp = prefBG_.GetComponent<UnityEngine.UI.Image>();
                imgComp.color = new Color(0.7f, 0.7f, 0.7f);
            }

            if (dropdown_ == null)
            {
                // Crate new drop down
                GameObject prefab = Resources.Load("Prefabs/DropdownTemplate") as GameObject;
                var canvas = GameObject.Find("Canvas");
                dropdown_ = GameObject.Instantiate(prefab, canvas.transform);


                // setup options
                var labels = setting.Labels;
                var dropdownComponent = dropdown_.GetComponent<UnityEngine.UI.Dropdown>();
                var options = dropdownComponent.options;
                while (options.Count < labels.Count)
                {
                    options.Add(new UnityEngine.UI.Dropdown.OptionData(""));
                }
                while (options.Count > labels.Count)
                {
                    options.Remove(options.Last());
                }
                Debug.Assert(options.Count == labels.Count);
                for (int i = 0; i < options.Count; i++)
                {
                    if (options[i].text != labels[i])
                    {
                        options[i].text = labels[i];
                    }
                }

                int index = dropdownComponent.value;
                if (dropdownComponent.captionText.text != options[index].text)
                {
                    dropdownComponent.captionText.text = options[index].text;
                }
            }

            GUIUtility.MoveUI(prefBG_, region, Rect.zero);
            var ddRect = new Rect(region.center.x - 200, region.center.y - 20, 400, 40);
            GUIUtility.MoveUI(dropdown_, ddRect, Rect.zero);
        }
         
        void moveImageObject(Rect region, GameObject imgObj)
        {
            var spriteRect = getSpriteRect(imgObj);
            var lt = spriteRect.position;
            var rb = spriteRect.position + spriteRect.size;
            lt = canvasMatrix_.MultiplyPoint(lt);
            rb = canvasMatrix_.MultiplyPoint(rb);
            var imgRect = imgObj.GetComponent<RectTransform>();
            imgRect.anchoredPosition = new Vector2(lt.x, -lt.y);
            imgRect.sizeDelta = rb - lt;
        }

        void drawBBoxList(Rect region, BBoxAnnotation annot, BBoxAnnotationSetting setting)
        {
            for (int i = 0; i < annot.BBoxs.Count; i++)
            {
                bool focused = annot.FocusedBBox == annot.BBoxs[i];
                var color = setting.GetShowColor(annot.BBoxs[i].Label);
                drawBBox(region, annot.BBoxs[i], setting, focused, color);
            }
        }

        void drawBBox(Rect region, BoundingBox bbox, BBoxAnnotationSetting setting, bool focused, Color color)
        {
            if (bbox.Rect == Rect.zero)
            {
                return;
            }

            var points = bbox.Corners();

            float currentScale = DUI.matrix.lossyScale[0];
            float bboxWidth = focused ? 8 : 4;
            float bboxPointSize = focused ? 12 : 6;

            // Render BBox line
            for (int i = 0; i < points.Count; i++)
            {
                var pt1 = points[i];
                var pt2 = points[(1 + i) % points.Count];
                GUIUtility.DrawLine(pt1, pt2, color, Math.Max(1, (int)(bboxWidth / currentScale)));
            }

            // Render BBox Points
            for (int i = 0; i < points.Count; i++)
            {
                var pt = points[i];
                if (focused && i == focusedPointIndex_ && editMode_ == EditMode.None)
                {
                    GUIUtility.DrawCircle(pt, 0.5f * color + 0.5f * Color.white, bboxPointSize / currentScale / 2);
                }
                else
                {
                    GUIUtility.DrawCircle(pt, color, bboxPointSize / currentScale / 2);
                }
            }

            // Draw Label Text
            var prevMat = DUI.matrix;
            DUI.matrix = Matrix4x4.identity;
            GUIStyle labelStyle = null; // DUI.skin.FindStyle("CanvasLabel");
            if (points.Count >= 1)
            {
                var tl = to2D(prevMat.MultiplyPoint(points[0]));
                var textPos = new Rect(tl.x + 5, tl.y + 5, 150, 30);
                DUI.Label(textPos, bbox.Label, labelStyle);
            }
            DUI.matrix = prevMat;
        }


        //
        // Helpers
        //

        BoundingBox getFocusedBBox(BBoxAnnotation annot)
        {
            if (0 <= annot.FocusedBBoxIndex && annot.FocusedBBoxIndex < annot.BBoxs.Count)
            {
                var bbox = annot.BBoxs[annot.FocusedBBoxIndex];
                if (0 <= focusedPointIndex_ && focusedPointIndex_ < bbox.Corners().Count)
                {
                    return bbox;
                }
            }
            return null;
        }

        Rect calculateImageObjectRect(Rect region, GameObject imgObj)
        {
            var spriteRect = getSpriteRect(imgObj);

            var lt = region.position;
            var rb = region.position + region.size;
            var imageRect = new Rect(lt, rb - lt);

            var ratios = imageRect.size / spriteRect.size;
            float ratio = Mathf.Max(1e-4f, Mathf.Min(ratios.x, ratios.y));

            var newSize = spriteRect.size * ratio;
            var newOffset = imageRect.center - newSize / 2;
            return new Rect(newOffset, newSize);
        }

        Rect getSpriteRect(GameObject imgObj)
        {
            var imgComponent = imgObj.GetComponent<UnityEngine.UI.Image>();
            Debug.Assert(imgComponent, imgObj.name + " must have component UnityEngine.UI.Image.");

            return imgComponent.sprite.rect;
        }

        bool searchBBoxNearPoint(BBoxAnnotation annot, Vector2 point2D, out int bboxIndex, out int pointIndex)
        {
            point2D = to2D(canvasMatrix_.inverse.MultiplyPoint(point2D));
            bboxIndex = -1;
            pointIndex = -1;

            float minDistance = 1e8f;
            for (int i = 0; i < annot.BBoxs.Count; i++)
            {
                var bbox = annot.BBoxs[i];
                var points = bbox.Corners();

                // Search by Points
                float dist;
                int j = nearestPoint(point2D, points, out dist);
                if (minDistance > dist)
                {
                    bboxIndex = i;
                    pointIndex = j;
                    minDistance = dist;
                }
            }

            return bboxIndex != -1;
        }

        int nearestPoint(Vector2 target, List<Vector2> points, out float minDistance)
        {
            const float threshold = 8.0f;
            int pointIndex = -1;
            minDistance = 1e8f;

            for (int j = 0; j < points.Count; j++)
            {
                var distance = (points[j] - target).magnitude;
                if (threshold >= distance)
                {
                    if (minDistance > distance)
                    {
                        pointIndex = j;
                        minDistance = distance;
                    }
                }
            }

            return pointIndex;
        }

        Vector2 to2D(Vector3 pos, bool upSideDown = false)
        {
            if (upSideDown)
            {
                return new Vector2(pos.x, Screen.height - pos.y);
            }
            return new Vector2(pos.x, pos.y);
        }
    }
}