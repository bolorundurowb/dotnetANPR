/*
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace dotNETANPR.GUI
{
    class ReportGenerator
    {
        string path;
        string output;
        Bitmap outImage;
        bool enabled;

        public ReportGenerator()
        {
            this.enabled = false;
        }

        public ReportGenerator(string path)
        {
            this.path = path;
            this.enabled = true;

            if (!File.Exists(path))
                throw new IOException("Report directory '" + path + "' doesn't exist.");
            this.output = "<!DOCTYPE HTML>" +
                "<html>" +
                "<head><title>ANPR Report</title>" +
                "</head>" +
                "<style type=\"text/css\">" +
                "@import \"style.css\";" +
                "</style>";
        }

        public void InsertText (string text)
        {
            if (!enabled)
                return;
            this.output += text;
            this.output += "/n";
        }

        public void InsertImage(Bitmap image, string cls, int w, int h)
        {
            if (!enabled) 
                return;
            string imageName = (image.GetHashCode()).ToString() + ".jpg";
            SaveImage(image, path + Path.DirectorySeparatorChar + imageName);
            if (w != 0 && h != 0)
                this.output += "<img src='" + imageName + "' alt='' width='" + w + "' height='" + h + "' class='" + cls + "'>\n";
            else
                this.output += "<img src='" + imageName + "' alt='' class='" + cls + "'>\n";
        }

        public void Finish()
        {
            if (!enabled)
                return;
            this.output += "</html>";
            FileStream fs = new FileStream(this.path + Path.DirectorySeparatorChar + "index.html", FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(fs);
            writer.Write(output);
            writer.Flush();
            writer.Close();
            Configurator.Configurator config = new Configurator.Configurator();
            string fileIn = config.GetPathProperty("reportgeneratorcss");
            CopyFile(fileIn, this.path + Path.DirectorySeparatorChar + "style.css");
        }

        public void CopyFile(string source, string dest)
        {
            File.Copy(source, dest, true);
        }

        public void SaveImage(Bitmap bi, string filepath)
        {
            if (!enabled) return;
            string type = filepath.Substring(filepath.LastIndexOf('.') + 1, filepath.Length).ToUpper();
            if (!type.Equals("BMP") &&
                    !type.Equals("JPG") &&
                    !type.Equals("JPEG") &&
                    !type.Equals("PNG")
                    ) 
                Console.WriteLine("unsupported format exception");

            try
            {
                bi.Save(filepath, new ImageFormat(new Guid(type)));
            }
            catch (Exception e)
            {
                Console.WriteLine("caught " + e.ToString());
                Application.Exit();
                throw new IOException("Can't open destination report directory.");
            }
        }
    }
}
