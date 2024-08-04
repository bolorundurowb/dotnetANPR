using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using DotNetANPR.Configuration;

namespace DotNetANPR.Utilities;

public class ReportGenerator
{
    private static readonly HashSet<string> SupportedImageFormats =
    [
        ".bmp",
        ".jpg",
        ".jpeg",
        ".png"
    ];

    private readonly string _reportDirectory;
    private readonly StringBuilder _reportContent;

    /// <summary>
    /// Initializes a new instance of the ReportGenerator class.
    /// </summary>
    /// <param name="directory">The directory where the report will be saved.</param>
    public ReportGenerator(string directory)
    {
        _reportDirectory = directory;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        _reportContent = new StringBuilder();
        _reportContent.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">" + "<html>"
            + "<head><title>ANPR report</title>" + "</head>" + "<style type=\"text/css\">"
            + "@import \"style.css\";" + "</style>");
    }

    /// <summary>
    /// Inserts text into the report.
    /// </summary>
    /// <param name="text">The text to be inserted.</param>
    public void InsertText(string text) => _reportContent.AppendLine(text);

    /// <summary>
    /// Inserts an image into the report.
    /// </summary>
    /// <param name="image">The image to be inserted.</param>
    /// <param name="cssClass">The CSS class to be applied to the image.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    public void InsertImage(Bitmap? image, string cssClass, int width, int height)
    {
        if (image is null)
            return;

        var imageName = image.GetHashCode() + ".jpg";
        SaveImage(image, imageName);

        if (width != 0 && height != 0)
            _reportContent.Append("<img src='").Append(imageName).Append("' alt='' width='").Append(width)
                .Append("' height='")
                .Append(height).Append("' class='").Append(cssClass).AppendLine("'>");
        else
            _reportContent.Append("<img src='").Append(imageName).Append("' alt='' class='").Append(cssClass)
                .AppendLine("'>");
    }

    /// <summary>
    /// Finalizes the report by appending the closing HTML tag and saving the report content.
    /// </summary>
    public void Finish()
    {
        _reportContent.Append("</html>");
        var outputPath = Path.Combine(_reportDirectory, "index.html");
        File.WriteAllText(outputPath, _reportContent.ToString());

        var cssPath = Configurator.Instance.GetPath("reportgeneratorcss");
        var cssOutputPath = Path.Combine(_reportDirectory, "style.css");
        File.Copy(cssPath, cssOutputPath);
    }

    /// <summary>
    /// Saves an image to a file.
    /// </summary>
    /// <param name="image">The image to be saved.</param>
    /// <param name="fileName">The name of the file where the image will be saved.</param>
    public void SaveImage(Bitmap image, string fileName)
    {
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!SupportedImageFormats.Contains(fileExtension))
            throw new InvalidOperationException("Unsupported file extension: " + fileExtension);

        var outputPath = Path.Combine(_reportDirectory, fileName);
        image.Save(outputPath);
    }
}
