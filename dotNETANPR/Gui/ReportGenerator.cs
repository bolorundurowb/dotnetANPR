using System;
using System.Drawing;
using System.IO;

namespace dotNETANPR.Gui
{
    public class ReportGenerator
    {
        private string path;
        private string output;
        private bool enabled;

        public ReportGenerator()
        {
            enabled = false;
        }

        public void InsertText(string text)
        {
            if (!enabled)
            {
                return;
            }
            output += text;
            output += "\n";
        }

        public void InsertImage(Bitmap image, string cls, int w, int h)
        {
            if (!enabled) return;
            string imageName = image.GetHashCode() + ".jpg";
            this.SaveImage(image, path + Path.DirectorySeparatorChar + imageName);
            if (w != 0 && h != 0)
                this.output += "<img src='" + imageName + "' alt='' width='" + w + "' height='" + h + "' class='" +
                               cls + "'>\n";
            else
                this.output += "<img src='" + imageName + "' alt='' class='" + cls + "'>\n";
        }

        public void SaveImage(Bitmap bi, string filepath)
        {
            if (!enabled) return;
            string type = filepath.Substring(filepath.LastIndexOf('.') + 1, filepath.Length).ToUpper();
            if (!type.Equals("BMP") &&
                !type.Equals("JPG") &&
                !type.Equals("JPEG") &&
                !type.Equals("PNG")
            ) Console.WriteLine("unsupported format exception");

            try
            {
                bi.Save(filepath);
            }
            catch (Exception e)
            {
                Console.WriteLine("catched " + e.ToString());
                Environment.Exit(1);
            }
        }
    }
}