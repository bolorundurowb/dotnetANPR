using System;
using System.IO;
using System.Text;
using DotNetANPR.Configuration;
using SkiaSharp;

namespace DotNetANPR.Utilities;

/// <summary>
/// Generates an HTML report containing text and images from the ANPR recognition process.
/// Images are encoded as PNG files using SkiaSharp and saved to the report directory.
/// </summary>
public class ReportGenerator
{
    private readonly string _reportDirectory;
    private readonly StringBuilder _reportContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportGenerator"/> class.
    /// Creates the report directory if it does not exist and begins building the HTML document.
    /// </summary>
    /// <param name="directory">The directory where the report files will be saved.</param>
    public ReportGenerator(string directory)
    {
        _reportDirectory = directory;

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _reportContent = new StringBuilder();
        _reportContent.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">"
            + "<html>"
            + "<head><title>ANPR report</title>"
            + "</head>"
            + "<style type=\"text/css\">"
            + "@import \"style.css\";"
            + "</style>");
    }

    /// <summary>
    /// Appends arbitrary HTML or text content to the report.
    /// </summary>
    /// <param name="text">The HTML or text content to insert.</param>
    public void InsertText(string text) => _reportContent.AppendLine(text);

    /// <summary>
    /// Encodes the given bitmap as a PNG image, saves it to the report directory,
    /// and inserts an <c>&lt;img&gt;</c> tag into the report HTML.
    /// </summary>
    /// <param name="image">The bitmap to insert. If <c>null</c>, no action is taken.</param>
    /// <param name="cssClass">The CSS class to apply to the <c>&lt;img&gt;</c> element.</param>
    /// <param name="width">
    /// The display width in pixels. If both <paramref name="width"/> and <paramref name="height"/>
    /// are zero, width and height attributes are omitted.
    /// </param>
    /// <param name="height">
    /// The display height in pixels. If both <paramref name="width"/> and <paramref name="height"/>
    /// are zero, width and height attributes are omitted.
    /// </param>
    public void InsertImage(SKBitmap? image, string cssClass, int width, int height)
    {
        if (image is null)
        {
            return;
        }

        var imageName = image.GetHashCode() + ".png";
        SaveImage(image, imageName);

        if (width != 0 && height != 0)
        {
            _reportContent.Append("<img src='").Append(imageName)
                .Append("' alt='' width='").Append(width)
                .Append("' height='").Append(height)
                .Append("' class='").Append(cssClass).AppendLine("'>");
        }
        else
        {
            _reportContent.Append("<img src='").Append(imageName)
                .Append("' alt='' class='").Append(cssClass).AppendLine("'>");
        }
    }

    /// <summary>
    /// Finalizes the report by closing the HTML document, writing <c>index.html</c> to the
    /// report directory, and copying the CSS stylesheet alongside it.
    /// </summary>
    public void Finish()
    {
        _reportContent.Append("</html>");
        var outputPath = Path.Combine(_reportDirectory, "index.html");
        File.WriteAllText(outputPath, _reportContent.ToString());

        var cssPath = Configurator.Instance.GetPath("reportgeneratorcss");
        var cssOutputPath = Path.Combine(_reportDirectory, "style.css");
        if (File.Exists(cssPath))
        {
            File.Copy(cssPath, cssOutputPath, overwrite: true);
        }
    }

    /// <summary>
    /// Encodes the given bitmap as a PNG and saves it to the report directory.
    /// </summary>
    /// <param name="image">The bitmap to save.</param>
    /// <param name="fileName">The file name (including extension) to save as.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is <c>null</c>.</exception>
    public void SaveImage(SKBitmap image, string fileName)
    {
        ArgumentNullException.ThrowIfNull(image);

        var outputPath = Path.Combine(_reportDirectory, fileName);

        using var imageData = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        imageData.SaveTo(stream);
    }
}
