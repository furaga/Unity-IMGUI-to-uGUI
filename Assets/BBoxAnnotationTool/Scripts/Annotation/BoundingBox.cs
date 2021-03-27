using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BBoxAnnotationTool
{
    public class BoundingBox
    {
        static int bboxCounter_ = 0;

        public string Label { get; set; }
        public string Name { get; set; }
        public float  X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public bool Empty { get { return X == 0 && Y == 0 && Width == 0 && Height == 0; } }
        public float XMin { get { return X; } }
        public float XMax { get { return X + Width; } }
        public float YMin { get { return Y; } }
        public float YMax { get { return Y + Height; } }
        public Rect Rect
        {
            set
            {
                X = value.x;
                Y = value.y;
                Width = value.width;
                Height = value.height;
            }
            get
            {
                return new Rect(X, Y, Width, Height);
            }
        }

        public BoundingBox()
        {
            Name = newName();
            Label = "";
        }

        public BoundingBox(string label, float x, float y, float w, float h)
        {
            Name = newName();
            Label = label;
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public List<Vector2> Corners()
        {
            var points = new List<Vector2>()
            {
                new Vector2(XMin, YMin), // Left Top
                new Vector2(XMin, YMax), // Left Bottom
                new Vector2(XMax, YMax), // Right Bottom
                new Vector2(XMax, YMin), // Right Top
            };
            return points;
        }

        public void Clear()
        {
            X = Y = Width = Height = 0;
        }

        public static BoundingBox Copy(BoundingBox bbox)
        {
            BoundingBox newBBox = new BoundingBox(bbox.Label, bbox.X, bbox.Y, bbox.Width, bbox.Height);
            return newBBox;
        }

        static string newName()
        {
            string name = string.Format("ID{0:000}", bboxCounter_);
            bboxCounter_++;
            return name;
        }

    }
}