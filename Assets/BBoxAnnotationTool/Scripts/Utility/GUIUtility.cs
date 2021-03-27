using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGUI;

namespace BBoxAnnotationTool
{
    public static class GUIUtility
    {
        static Texture2D whitePixel_ = null;
        static Texture2D circleTex_ = null;

        public static Texture2D MakeTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        static void setupLineTexture()
        {
            if (!whitePixel_)
            {
                whitePixel_ = new Texture2D(1, 1);
                whitePixel_.filterMode = FilterMode.Bilinear;
            }
        }

        static void setupCirlceTexture()
        {
            if (!circleTex_)
            {
                circleTex_ = Resources.Load<Texture2D>("Textures/circle");
            }
        }

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            setupLineTexture();

            Matrix4x4 oldMatrix = DUI.matrix;
            Color oldColor = DUI.color;

            DUI.color = color;

            float angle = Vector3.Angle(pointB - pointA, Vector2.right);
            if (pointA.y > pointB.y) { angle = -angle; }

            var pivot = new Vector2(pointA.x, pointA.y + 0.5f);
            var scale2D = new Vector2(Math.Max(1, (pointB - pointA).magnitude), width);
            var scale = new Vector3(scale2D.x, scale2D.y, 1);
            var deltaMatrix = Matrix4x4.Translate(-pivot);
            deltaMatrix = Matrix4x4.Scale(scale) * deltaMatrix;
            deltaMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, angle)) * deltaMatrix;
            deltaMatrix = Matrix4x4.Translate(pivot) * deltaMatrix;
            DUI.matrix = oldMatrix * deltaMatrix;

            DUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), whitePixel_);

            DUI.matrix = oldMatrix;
            DUI.color = oldColor;
        }

        public static void DrawCircle(Vector2 point, Color color, float radius)
        {
            setupCirlceTexture();
            Color oldColor = DUI.color;
            DUI.color = color;
            DUI.DrawTexture(new Rect(point.x - radius, point.y - radius, 2 * radius, 2 * radius), circleTex_);
            DUI.color = oldColor;
        }

        public static void MoveUI(GameObject ui, Rect position, Rect bound)
        {
            if (bound != Rect.zero)
            {
                if (!bound.Contains(position.position) || !bound.Contains(position.position + position.size))
                {
                    // Out of bound. Skip.
                    ui.SetActive(false);
                }
                else
                {
                    ui.SetActive(true);
                }
            }
            var rect = ui.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(position.x, -position.y);
            rect.sizeDelta = position.size;
        }
    }
}