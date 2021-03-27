using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace BBoxAnnotationTool
{
    public class BBoxAnnotationSetting
    {
        public List<string> Labels { get; set; }

        Dictionary<string, Color> colormap_ = new Dictionary<string, Color>();
        System.Random random = new System.Random();

        public Color GetShowColor(string label)
        {
            if (false == colormap_.ContainsKey(label))
            {
                colormap_[label] = newColor();
            }

            return colormap_[label];
        }

        public Color ChangeShowColor(string label)
        {
            colormap_[label] = newColor();
            return colormap_[label];
        }

        Color newColor()
        {
            float h = (float)random.NextDouble();
            float s = (float)random.NextDouble() / 2 + 0.5f;
            float v = (float)random.NextDouble() / 5 + 0.8f;
            return Color.HSVToRGB(h, s, v);
        }

        public BBoxAnnotationSetting()
        {
            Labels = new List<string>();
        }

        public static BBoxAnnotationSetting Load(string labelsPath)
        {
            var fallback = fallbacksetting();
            if (!System.IO.File.Exists(labelsPath))
            {
                Debug.LogWarning("Label list path " + labelsPath + " was not found. We use a default setting.");
                return fallback;
            }

            var setting = loadSettings(labelsPath);
            return setting;
        }

        static BBoxAnnotationSetting fallbacksetting()
        {
            var setting = new BBoxAnnotationSetting();
            setting.Labels = new List<string>()
            {
                "person",
                "face",
                "street sign",
                "road",
                "grass",
                "car",
                "house",
                "window",
                "chair",
                "sports ball",
            };
            return setting;
        }

        static BBoxAnnotationSetting loadSettings(string labelsPath)
        {
            var setting = new BBoxAnnotationSetting();
            setting.Labels = new List<string>(System.IO.File.ReadAllLines(labelsPath));
            return setting;
        }
    }
}