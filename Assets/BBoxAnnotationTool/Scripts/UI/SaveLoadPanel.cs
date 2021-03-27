using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGUI;

namespace BBoxAnnotationTool
{
    public class SaveLoadPanel
    {
        string imageDirText_ = "";
        
        public void SetImageDirectoryText(string text)
        {
            imageDirText_ = text;
        }

        public string OnShortcutKey(string imageDir, BBoxAnnotationSet annotSet)
        {
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (alt && Input.GetKeyDown(KeyCode.S))
            {
                save(annotSet, imageDir);
            }
            if (alt && Input.GetKeyDown(KeyCode.O))
            {
#if UNITY_EDITOR
                imageDir = showLoadDialog(annotSet, imageDir);
#endif
            }
            if (alt && Input.GetKeyDown(KeyCode.R))
            {
                imageDir = reload(imageDir, annotSet);
            }
            return imageDir;
        }

        string reload(string imageDir, BBoxAnnotationSet annotSet)
        {
            if (System.IO.Directory.Exists(imageDirText_))
            {
                imageDir = imageDirText_;
                annotSet.Load(imageDirText_, System.IO.Path.Combine(imageDir, "bboxes.csv"));
                UndoRedoManager.Instance.Reset();
            }
            else
            {
                Debug.LogWarning(string.Format("Failed to load {0}. There is not such directory", imageDirText_)); ;
            }
            return imageDir;
        }

        public string Draw(Rect region, string imageDir, BBoxAnnotationSet annotSet)
        {
            DUI.Box(region, "");

            var loadButtonRect = new Rect(region.position + new Vector2(10, 10), new Vector2(80, 25));
            if (DUI.Button(loadButtonRect, "Reload"))
            {
                imageDir = reload(imageDir, annotSet);
            }

            var saveButtonRect = new Rect(region.position + new Vector2(100, 10), new Vector2(80, 25));
            if (DUI.Button(saveButtonRect, "Save"))
            {
                save(annotSet, imageDir);
            }


            var labelRect = new Rect(region.position + new Vector2(10, 40), new Vector2(region.width - 50, 30));
            imageDirText_ = DUI.TextField(labelRect, imageDirText_).Trim();

#if UNITY_EDITOR
            var dialogButtonRect = new Rect(new Vector2(region.xMax - 40, 40), new Vector2(30, 30));
            if (DUI.Button(dialogButtonRect, ".."))
            {
                imageDir = showLoadDialog(annotSet, imageDir);
            }
#endif

            return imageDir;
        }

        public void save(BBoxAnnotationSet annotSet, string imageDir)
        {
            annotSet.Save(System.IO.Path.Combine(imageDir, "bboxes.csv"), imageDir);
        }

#if UNITY_EDITOR
        public string showLoadDialog(BBoxAnnotationSet annotSet, string imageDir)
        {
            var path = UnityEditor.EditorUtility.OpenFolderPanel("Select image folder", imageDir, "");
            if (System.IO.Directory.Exists(path))
            {
                imageDir = path;
                SetImageDirectoryText(imageDir);
                annotSet.Load(imageDir, System.IO.Path.Combine(imageDir, "bboxes.csv"));
                UndoRedoManager.Instance.Reset();
            }
            else
            {
                Debug.LogWarning(string.Format("Failed to load {0}. There is not such directory", path)); ;
            }
            return imageDir;
        }
#endif
    }
}