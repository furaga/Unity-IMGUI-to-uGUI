using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EasyGUI;

namespace BBoxAnnotationTool
{
    public class BBoxAnnotationWindow : MonoBehaviour
    {
        public int ImageIndex = 0;
        public GameObject InputImage;
        private Sprite inputSprite_ = null;
        public string ImageDirectory = "./Assets/BBoxAnnotationTool/Resources/Annotations";
        public string LabelFile = "./Assets/BBoxAnnotationTool/Resources/Settings/labels.txt";
        public GUISkin guiSkin;

        // Annotation Data
        BBoxAnnotationSet annotSet_;

        // UI components
        SaveLoadPanel saveLoadPanel_;
        AnnotationCanvas annotCanvas_;
        SeekBar seekBar_;

        BBoxAnnotation currentSegmentation_
        {
            get
            {
                if (0 <= ImageIndex && ImageIndex < annotSet_.AnnotationList.Count)
                {
                    return annotSet_.AnnotationList[ImageIndex];
                }
                return null;
            }
        }

        void Start()
        {
            saveLoadPanel_ = new SaveLoadPanel();
            annotCanvas_ = new AnnotationCanvas();
            seekBar_ = new SeekBar();

            // Load images and already existing annotations
            string annotPath = System.IO.Path.Combine(ImageDirectory, "bboxes.csv");
            annotSet_ = new BBoxAnnotationSet(LabelFile);
            annotSet_.Load(ImageDirectory, annotPath);
            Debug.Log("Loaded image directory: " + ImageDirectory);
            Debug.Log("Loaded annotation file: " + annotPath);
            
            saveLoadPanel_.SetImageDirectoryText(ImageDirectory);

            clearSprite();
            switchImage(ImageIndex);
        }

        private void Update()
        {
            string prevImageDirectory = ImageDirectory;
            Rect annotCanvasRect = calculateAnnotCanvasRect();
            var setting = annotSet_.Setting;

            // Shortcut
            ImageDirectory = saveLoadPanel_.OnShortcutKey(ImageDirectory, annotSet_);
            if (currentSegmentation_ != null)
            {
                annotCanvas_.OnShortcutKey(annotCanvasRect, currentSegmentation_, setting);
            }
            onShortcutKey();

            // Update
            if (currentSegmentation_ != null)
            {
                annotCanvas_.Update(annotCanvasRect, InputImage, currentSegmentation_, setting);
            }

            if (prevImageDirectory != ImageDirectory)
            {
                clearSprite();
                switchImage(0);
            }

            onDUI();
        }

        void onShortcutKey()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                object modifiedObject = UndoRedoManager.Instance.Undo();
                int newImageIndex = findModifiedImageIndex(modifiedObject);
                if (newImageIndex >= 0 && newImageIndex != ImageIndex)
                {
                    switchImage(newImageIndex);
                }
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                object modifiedObject = UndoRedoManager.Instance.Redo();
                int newImageIndex = findModifiedImageIndex(modifiedObject);
                if (newImageIndex >= 0 && newImageIndex != ImageIndex)
                {
                    switchImage(newImageIndex);
                }
            }
        }

        int findModifiedImageIndex(object modifiedObject)
        {
            if (modifiedObject is BBoxAnnotation)
            {
                for (int i = 0; i < annotSet_.AnnotationList.Count; i++)
                {
                    if (annotSet_.AnnotationList[i] == modifiedObject)
                    {
                        return i;
                    }
                }
                return -1;
            }
            if (modifiedObject is BoundingBox)
            {
                for (int i = 0; i < annotSet_.AnnotationList.Count; i++)
                {
                    foreach (var c in annotSet_.AnnotationList[i].BBoxs)
                    {
                        if (c == modifiedObject)
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
            return -1;
        }

        void onDUI()
        {
            string prevImageDirectory = ImageDirectory;

            Rect saveLoadPanelRect, seekBarRect, canvasRect;
            calcPanelRegions(out saveLoadPanelRect, out seekBarRect, out canvasRect);

            // Draw UI components
            DUI.skin = guiSkin;
            annotCanvas_.Draw(canvasRect, InputImage, currentSegmentation_, annotSet_.Setting);
            ImageDirectory = saveLoadPanel_.Draw(saveLoadPanelRect, ImageDirectory, annotSet_);
            int newImageIndex = seekBar_.Draw(seekBarRect, annotSet_, ImageIndex);
            if (prevImageDirectory != ImageDirectory)
            {
                clearSprite();
                switchImage(0);
            }
            else if (newImageIndex != ImageIndex)
            {
                switchImage(newImageIndex);
            }
        }

        void clearSprite()
        {
            var whitepixel = GUIUtility.MakeTexture(1, 1, Color.white);
            replaceInputTexture(whitepixel);
        }

        void switchImage(int newImageIndex)
        {
            if (0 <= newImageIndex && newImageIndex < annotSet_.AnnotationList.Count)
            {
                ImageIndex = newImageIndex;
                var imgName = annotSet_.AnnotationList[ImageIndex].ImageName;
                var imgPath = System.IO.Path.Combine(ImageDirectory, imgName);
                var img = ImageLoader.Load(imgPath);
                replaceInputTexture(img);
            }

            annotCanvas_.ResetMatrix();
        }

        void calcPanelRegions(out Rect saveLoadPanelRect, out Rect seekBarRect, out Rect annotCanvasRect)
        {
            int w = Screen.width;
            int h = Screen.height;
            float seekbarHeight = 100;

            saveLoadPanelRect = new Rect(0, 0, w, 80);
            seekBarRect = new Rect(0, h - seekbarHeight, w, seekbarHeight);
            annotCanvasRect = new Rect(0, saveLoadPanelRect.yMax, seekBarRect.width, (int)(seekBarRect.yMin - saveLoadPanelRect.yMax));
        }

        Rect calculateAnnotCanvasRect()
        {
            Rect saveLoadPanelRect, seekBarRect, annotCanvasRect;
            calcPanelRegions(out saveLoadPanelRect, out seekBarRect, out annotCanvasRect);
            return annotCanvasRect;
        }

        void replaceInputTexture(Texture2D texture)
        {
            if (inputSprite_ != null)
            {
                Destroy(inputSprite_.texture);
            }
            Destroy(inputSprite_);

            inputSprite_ = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            replaceSprite(InputImage, inputSprite_);
        }

        Sprite getImageSprite(GameObject imgObj)
        {
            if (imgObj == null)
            {
                return null;
            }
            var imgComponent = imgObj.GetComponent<UnityEngine.UI.Image>();
            if (imgComponent == null)
            {
                return null;
            }
            return imgComponent.sprite;
        }

        void replaceSprite(GameObject imgObj, Sprite sprite)
        {
            if (imgObj == null)
            {
                return;
            }
            var imgComponent = imgObj.GetComponent<UnityEngine.UI.Image>();
            if (imgComponent == null)
            {
                return;
            }
            imgComponent.sprite = sprite;
        }
    }
}