using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace dotnetANPR.Utilities;

/// <summary>
/// Writes intermediate image processing stages to a directory for diagnostic purposes.
/// Each call to <see cref="Write"/> saves a sequentially-numbered JPEG named
/// <c>{counter:D2}-{stageName}.jpg</c>.
/// </summary>
public class StageWriter
{
    private readonly string _directory;
    private int _counter;

    public StageWriter(string directory)
    {
        _directory = directory;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        _counter = 0;
    }

    /// <summary>
    /// Saves <paramref name="image"/> to the dump directory as the next numbered stage.
    /// </summary>
    public void Write(string stageName, Bitmap image)
    {
        _counter++;
        var fileName = $"{_counter:D2}-{stageName}.jpg";
        var outputPath = Path.Combine(_directory, fileName);
        image.Save(outputPath, ImageFormat.Jpeg);
    }
}
