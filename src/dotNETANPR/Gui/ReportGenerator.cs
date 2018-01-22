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

        public ReportGenerator(string path)
        {
            this.path = path;
            enabled = true;
            if (!Directory.Exists(path))
            {
                throw new IOException("Report directory '" + path + "' doesn't exist");
            }
            output = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">" +
                          "<html>" +
                          "<head><title>ANPR report</title>" +
                          "</head>" +
                          "<style type=\"text/css\">" +
                          "@import \"style.css\";" +
                          "</style>";
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
            var imageName = image.GetHashCode() + ".jpg";
            SaveImage(image, path + Path.DirectorySeparatorChar + imageName);
            if (w != 0 && h != 0)
                output += "<img src='" + imageName + "' alt='' width='" + w + "' height='" + h + "' class='" +
                               cls + "'>\n";
            else
                output += "<img src='" + imageName + "' alt='' class='" + cls + "'>\n";
        }

        public void Finish()
        {
            if (!enabled)
            {
                return;
            }
            output += "</html>";
            var path = this.path + Path.DirectorySeparatorChar + "index.html";
            File.WriteAllText(path, output);
            CopyFile(Intelligence.Intelligence.Configurator.GetPathProperty("reportgeneratorcss"),
                this.path + Path.DirectorySeparatorChar + "style.css");
        }

        private void CopyFile(string source, string dest)
        {
            File.Copy(source, dest);
        }

        public void SaveImage(Bitmap bitmap, string filepath)
        {
            if (!enabled) return;
            var type = filepath.Substring(filepath.LastIndexOf('.') + 1, filepath.Length).ToUpper();
            if (!type.Equals("BMP") &&
                !type.Equals("JPG") &&
                !type.Equals("JPEG") &&
                !type.Equals("PNG")
            )
            {
                Console.WriteLine("unsupported format exception");
            }

            try
            {
                bitmap.Save(filepath);
            }
            catch (Exception e)
            {
                Console.WriteLine("catched " + e);
                Environment.Exit(1);
            }
        }
    }
}