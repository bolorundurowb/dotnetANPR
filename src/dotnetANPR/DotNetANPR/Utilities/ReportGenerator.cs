using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using DotNetANPR.Configuration;

namespace DotNetANPR.Utilities;

public class ReportGenerator
{
    private static readonly HashSet<string> supportedFileFormats = new HashSet<string>()
    {
        "bmp",
        "jpg",
        "jpeg",
        "png"
    };

    private String directory;
    private StringBuilder output; // TODO refactor into a form

    public ReportGenerator(String directory)
    {
        this.directory = directory;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        output = new StringBuilder();
        output.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">" + "<html>"
            + "<head><title>ANPR report</title>" + "</head>" + "<style type=\"text/css\">"
            + "@import \"style.css\";" + "</style>");
    }

    public void InsertText(String text) => output.AppendLine(text);

    public void InsertImage(Bitmap image, String cls, int w, int h)
    {
        String imageName = image.GetHashCode() + ".jpg";
        SaveImage(image, imageName);
        if ((w != 0) && (h != 0))
        {
            output.Append("<img src='").Append(imageName).Append("' alt='' width='").Append(w).Append("' height='")
                .Append(h).Append("' class='").Append(cls).AppendLine("'>");
        }
        else
        {
            output.Append("<img src='").Append(imageName).Append("' alt='' class='").Append(cls).AppendLine("'>");
        }
    }

    public void Finish()
    {
        output.Append("</html>");
        var outputPath = directory + Path.PathSeparator + "index.html";
        File.WriteAllText(outputPath, output.ToString());

        String cssPath = Configurator.Instance.GetPath("reportgeneratorcss");
        var cssOutputPath = directory + Path.PathSeparator + "style.css";
        File.Copy(cssPath, cssOutputPath);
    }

    public void SaveImage(Bitmap bi, String filename)
    {
        String fileExtension = Path.GetExtension(filename);
        if (!supportedFileFormats.Contains(fileExtension))
        {
            throw new InvalidOperationException("Unsupported file extension: " + fileExtension);
        }

        var outputPath = directory + Path.PathSeparator + filename;
        bi.Save(outputPath);
    }
}
