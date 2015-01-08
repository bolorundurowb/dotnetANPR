using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using dotNETANPR.Configurator;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace dotNETANPR.GUI
{
    public class ReportGenerator
    {
        private string path;
    private string outtput;
    private StreamWriter outt;
    private bool enabled;
    
    public ReportGenerator(string path) 
    {
        this.path = path;
        this.enabled = true;
        if (!File.Exists(path)) throw new IOException("Report directory '"+path+"' doesn't exists");
        
        this.outtput = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">"+
                "<html>" +
                "<head><title>ANPR report</title>" +
                "</head>" +
                "<style type=\"text/css\">" +
		"@import \"style.css\";" +
                "</style>";
        
    }
    
    public ReportGenerator() {
        this.enabled = false;
    }
    
    public void insertText(string text) {
        if (!enabled) return;
        this.outtput += text;
        this.outtput += "\n";
    }
    
    public void insertImage(Bitmap image, string cls, int w, int h) 
  {
        if (!enabled) return;
        string imageName = image.GetHashCode().ToString()+".jpg";
        this.saveImage(image, path + Path.DirectorySeparatorChar + imageName);
        if (w!=0 && h!=0)
            this.outtput += "<img src='"+imageName+"' alt='' width='"+w+"' height='"+h+"' class='"+cls+"'>\n";
        else 
            this.outtput += "<img src='"+imageName+"' alt='' class='"+cls+"'>\n";
    }
    
    public void finish()
    {
        if (!enabled) return;
        this.outtput += "</html>";
        FileStream os = new FileStream(this.path + Path.DirectorySeparatorChar + "index.html", FileMode.OpenOrCreate);
        StreamWriter writer = new StreamWriter(os);
        writer.Write(outtput);
        writer.Flush();
        writer.Close();
        Configurator.Configurator config = new Configurator.Configurator();
        string fileIn = config.getPathProperty("reportgeneratorcss");
        copyFile(fileIn,this.path + Path.DirectorySeparatorChar+"style.css");
    }
    
    public void copyFile(string ini, string outt)
    {
        File.Copy(ini, outt, true);
        // or
        //  destinationChannel.transferFrom
        //       (sourceChannel, 0, sourceChannel.size());
        //sourceChannel.close();
        //destinationChannel.close();
    }

    public void saveImage(Bitmap bi, string filepath)
    {
        if (!enabled) return;
        string type = filepath.Substring(filepath.LastIndexOf('.') +1,filepath.Length).ToUpper();
        if (!type.Equals("BMP") &&
                !type.Equals("JPG") &&
                !type.Equals("JPEG") &&
                !type.Equals("PNG")
                ) Console.WriteLine("unsupported format exception");//throw new IOException("Unsupported file format");
        
        try
        {
            bi.Save(filepath, new ImageFormat(new Guid(type)));
        } 
        catch (Exception e) 
        {
            Console.WriteLine("catched "+e.ToString());
            Application.Exit();
            throw new IOException("Can't open destination report directory");
        }
    }
    }
}
