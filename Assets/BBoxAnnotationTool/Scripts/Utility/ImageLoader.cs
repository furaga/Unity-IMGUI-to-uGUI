using UnityEngine;
using System.IO;

namespace BBoxAnnotationTool
{
    public class ImageLoader
    {
        public static Texture2D Load(string filepath)
        {
            string filePath = filepath;
            byte[] byteData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            texture.LoadImage(byteData);
            return texture;
        }
    }
}