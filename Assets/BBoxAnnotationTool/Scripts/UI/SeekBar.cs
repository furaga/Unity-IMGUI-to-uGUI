using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGUI;

namespace BBoxAnnotationTool
{
    public class SeekBar
    {
        int imageIndex_ = 0;
        string textFieldText_ = "0";

        public int Draw(Rect region, BBoxAnnotationSet annotSet, int imageIndex)
        {
            int minIndex = 0;
            int maxIndex = annotSet.AnnotationList.Count - 1;

            updateImageIndex(imageIndex, minIndex, maxIndex);


            DUI.Box(region, "");
            
            var sliderOffset = new Vector2(10, 10);
            int sliderHeight = 30;
            int buttonOffsetY = 30;
            Vector2 buttonSize = new Vector2(40, 40);
            float buttonMargin = 10;

            // Draw Slider
            var sliderPosition = new Rect(
                region.x + sliderOffset.x,
                region.y + sliderOffset.y,
                region.width - 2 * sliderOffset.x,
                sliderHeight);
            int newIdx = (int)DUI.HorizontalSlider(sliderPosition, imageIndex_, minIndex, maxIndex);

            // Draw Buttons
            Vector2 buttonPos = new Vector2(region.center.x - 2 * buttonSize.x - 1.5f * buttonMargin, region.y + sliderOffset.y + buttonOffsetY);
            if (DUI.Button(new Rect(buttonPos, buttonSize), "◁◁"))
            {
                newIdx -= 10;
            }
            buttonPos += new Vector2(buttonMargin + buttonSize.x, 0);
            if (DUI.Button(new Rect(buttonPos, buttonSize), "◁"))
            {
                newIdx -= 1;
            }
            buttonPos += new Vector2(buttonMargin + buttonSize.x, 0);
            if (DUI.Button(new Rect(buttonPos, buttonSize), "▷"))
            {
                newIdx += 1;
            }
            buttonPos += new Vector2(buttonMargin + buttonSize.x, 0);
            if (DUI.Button(new Rect(buttonPos, buttonSize), "▷▷"))
            {
                newIdx += 10;
            }
            updateImageIndex(newIdx, minIndex, maxIndex);

            // Draw Text Field
            var textFieldSize = new Vector2(region.width * 0.2f, buttonSize.y);
            var textFieldPosition = new Vector2(sliderPosition.xMax - textFieldSize.x, buttonPos.y);
            string newText = DUI.TextField(new Rect(textFieldPosition, textFieldSize), textFieldText_);
            updateTextFieldText(newText, minIndex, maxIndex);

            return imageIndex_;
        }


        void updateImageIndex(int newIdx, int minIndex, int maxIndex)
        {
            newIdx = Mathf.Max(minIndex, Mathf.Min(maxIndex, newIdx));
            if (newIdx == imageIndex_)
            {
                return;
            }

            imageIndex_ = newIdx;
            textFieldText_ = imageIndex_.ToString();
        }

        void updateTextFieldText(string newText, int minIndex, int maxIndex)
        {
            if (newText != textFieldText_)
            {
                textFieldText_ = newText;
                int newIdx;
                if (int.TryParse(newText, out newIdx))
                {
                    updateImageIndex(newIdx, minIndex, maxIndex);
                }
            }
        }
        
    }
}