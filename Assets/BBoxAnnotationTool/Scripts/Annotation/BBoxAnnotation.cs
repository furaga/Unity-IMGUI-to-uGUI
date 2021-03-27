using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using System.Xml.Linq;

namespace BBoxAnnotationTool
{
    public class BBoxAnnotation
    {
        public string ImageName { get; set; }
        public List<BoundingBox> BBoxs { get; set; }

        public int FocusedBBoxIndex { get; private set; }
        public BoundingBox FocusedBBox
        {
            get
            {
                if (0 <= FocusedBBoxIndex && FocusedBBoxIndex < BBoxs.Count)
                {
                    return BBoxs[FocusedBBoxIndex];
                }
                return null;
            }
        }

        public BBoxAnnotation(string imageName)
        {
            ImageName = imageName;
            BBoxs = new List<BoundingBox>();
            FocusedBBoxIndex = -1;
        }

        public BBoxAnnotation(string imageName, List<BoundingBox> bboxs)
        {
            ImageName = imageName;
            BBoxs = bboxs;
            FocusedBBoxIndex = -1;
        }

        public void SetFocus(int i)
        {
            if (0 <= i && i < BBoxs.Count)
            {
                FocusedBBoxIndex = i;
            }
        }
        public void Unfocus()
        {
            FocusedBBoxIndex = -1;
        }
    }

    public class BBoxAnnotationSet
    {
        public List<BBoxAnnotation> AnnotationList = new List<BBoxAnnotation>();
        public BBoxAnnotationSetting Setting;

        public BBoxAnnotationSet(string settingPath)
        {
            Setting = BBoxAnnotationSetting.Load(settingPath);
        }

        List<BoundingBox> GetBBoxList(int imageIndex)
        {
            var annots = new List<BoundingBox>();
            if (0 <= imageIndex && imageIndex < AnnotationList.Count)
            {
                annots = AnnotationList[imageIndex].BBoxs;
            }
            return annots;
        }

        public void Load(string imageDirectory, string annotFile)
        {
            AnnotationList = new List<BBoxAnnotation>();

            var imageExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", }.ToList();

            if (!System.IO.Directory.Exists(imageDirectory))
            {
                Debug.LogWarning(string.Format("Not Found: {0}", imageDirectory)); ;
                return;
            }

            Dictionary<string, BBoxAnnotation> annotDict = new Dictionary<string, BBoxAnnotation>();
            if (System.IO.File.Exists(annotFile))
            {
                annotDict = loadBBoxes(imageDirectory, annotFile);
            }

            var imgPaths = System.IO.Directory.GetFiles(imageDirectory);
            foreach (var path in imgPaths)
            {
                string ext = System.IO.Path.GetExtension(path).ToLower();
                if (imageExts.Contains(ext))
                {
                    string imgName = System.IO.Path.GetFileName(path);
                    var segm = annotDict.ContainsKey(imgName) ? annotDict[imgName] : new BBoxAnnotation(imgName);
                    AnnotationList.Add(segm);
                }
            }
            Debug.Log("Loaded annotations: " + imageDirectory + " (" + annotFile + ")");
        }

        bool tryGetImageSize(string imagePath, out int w, out int h)
        {
            bool ok = false;
            w = h = -1;
            if (System.IO.File.Exists(imagePath))
            {
                var tex = ImageLoader.Load(imagePath);
                if (tex)
                {
                    w = tex.width;
                    h = tex.height;
                    ok = true;
                }
                UnityEngine.Object.Destroy(tex);
            }
            return ok;
        }

        Dictionary<string, BBoxAnnotation> loadBBoxes(string imageDirectory, string annotFile)
        {
            Dictionary<string, BBoxAnnotation> bboxDict = new Dictionary<string, BBoxAnnotation>();
            foreach (string line in System.IO.File.ReadAllLines(annotFile, System.Text.Encoding.UTF8).Skip(1))
            {
                var tokens = line.Split(',');
                string imgName = tokens[0];

                // Get Image width/height
                int imgWidth, imgHeight;
                string imagePath = System.IO.Path.Combine(imageDirectory, imgName);
                bool ok = tryGetImageSize(imagePath, out imgWidth, out imgHeight);
                if (false == ok)
                {
                    Debug.LogWarning("Failed to load because " + imagePath + " does not exist.");
                    continue;
                }

                string label = tokens[1];
                float x = float.Parse(tokens[2]) * imgWidth;
                float y = float.Parse(tokens[3]) * imgHeight;
                float w = float.Parse(tokens[4]) * imgWidth;
                float h = float.Parse(tokens[5]) * imgHeight;

                if (bboxDict.ContainsKey(imgName) == false)
                {
                    bboxDict[imgName] = new BBoxAnnotation(imgName);
                }
                bboxDict[imgName].BBoxs.Add(new BoundingBox(label, x, y, w, h));
            }

            return bboxDict;
        }

        public void Save(string outCsvPath, string imageDirectory)
        {
            List<string> lines = new List<string>();
            lines.Add("ImageName,Label,X,Y,W,H");

            foreach (var annot in AnnotationList)
            {
                int imgWidth, imgHeight;
                string imagePath = System.IO.Path.Combine(imageDirectory, annot.ImageName);
                bool ok = tryGetImageSize(imagePath, out imgWidth, out imgHeight);
                if (false == ok)
                {
                    Debug.LogWarning("Failed to load because " + imagePath + " does not exist.");
                    continue;
                }

                foreach (var bbox in annot.BBoxs)
                {
                    lines.Add(string.Format(
                        "{0},{1},{2},{3},{4},{5}",
                        annot.ImageName,
                        bbox.Label,
                        bbox.X / imgWidth,
                        bbox.Y / imgHeight,
                        bbox.Width / imgWidth,
                        bbox.Height / imgHeight));
                }
            }

            System.IO.File.WriteAllLines(outCsvPath, lines.ToArray(), System.Text.Encoding.UTF8);

            Debug.Log("Saved annotations: " + outCsvPath);
        }
    }
}