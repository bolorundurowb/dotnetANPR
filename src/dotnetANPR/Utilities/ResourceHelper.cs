using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNetANPR.Utilities;

/// <summary>
/// Helper for loading application resources. Resources embedded in the assembly are preferred;
/// if a requested path is not embedded, the helper falls back to the local file system.
/// </summary>
public static class ResourceHelper
{
    private static readonly Assembly Assembly = typeof(ResourceHelper).Assembly;
    private static readonly string[] ManifestNames = Assembly.GetManifestResourceNames();

    /// <summary>
    /// Opens a stream for the resource at the specified path.
    /// </summary>
    /// <param name="path">The logical resource path (e.g. "Resources/config.json").</param>
    /// <returns>A read-only stream, or <c>null</c> if the resource is not found.</returns>
    public static Stream? OpenStream(string path)
    {
        var manifestName = ResolveManifestName(path);
        if (manifestName != null)
            return Assembly.GetManifestResourceStream(manifestName);

        if (File.Exists(path))
            return File.OpenRead(path);

        return null;
    }

    /// <summary>
    /// Reads the text content of the resource at the specified path.
    /// </summary>
    /// <param name="path">The logical resource path.</param>
    /// <returns>The resource content, or <c>null</c> if the resource is not found.</returns>
    public static string? ReadText(string path)
    {
        using var stream = OpenStream(path);
        if (stream == null)
            return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Determines whether a resource exists at the specified path.
    /// </summary>
    /// <param name="path">The logical resource path.</param>
    /// <returns><c>true</c> if the resource exists either as an embedded resource or on disk.</returns>
    public static bool Exists(string path)
    {
        if (ResolveManifestName(path) != null)
            return true;

        return File.Exists(path);
    }

    /// <summary>
    /// Enumerates resources with the specified extension under the given directory path.
    /// Embedded resources are enumerated first; if none are found, the file system is used.
    /// </summary>
    /// <param name="directory">The logical directory path (e.g. "Resources/alphabets/alphabet_8x13").</param>
    /// <param name="extension">The file extension including the leading dot (e.g. ".jpg").</param>
    /// <returns>A list of logical paths to matching resources.</returns>
    public static List<string> Enumerate(string directory, string extension)
    {
        var results = new List<string>();
        var normalizedDir = NormalizePath(directory);
        var expectedPrefix = $"DotNetANPR.{normalizedDir.Replace('/', '.')}.";

        foreach (var manifestName in ManifestNames)
        {
            if (manifestName.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
                && manifestName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(ToLogicalPath(manifestName));
            }
        }

        if (results.Count != 0)
            return results;

        if (Directory.Exists(directory))
        {
            results.AddRange(Directory
                .EnumerateFiles(directory, "*" + extension)
                .Select(f => f.Replace('\\', '/')));
        }

        return results;
    }

    /// <summary>
    /// Converts a logical path to the most likely manifest resource name.
    /// </summary>
    private static string? ResolveManifestName(string path)
    {
        var normalized = NormalizePath(path);
        var dotted = normalized.Replace('/', '.');

        // Exact match using the default namespace prefix.
        var expected = $"DotNetANPR.{dotted}";
        var match = ManifestNames.FirstOrDefault(n =>
            string.Equals(n, expected, StringComparison.OrdinalIgnoreCase));
        if (match != null)
            return match;

        // Fallback: match any manifest name ending with the dotted path suffix.
        return ManifestNames.FirstOrDefault(n =>
            n.EndsWith("." + dotted, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').Trim('/');

    private static string ToLogicalPath(string manifestName)
    {
        // Strip the default namespace prefix if present.
        const string prefix = "DotNetANPR.";
        var logical = manifestName;
        if (logical.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            logical = logical.Substring(prefix.Length);

        // The last segment after the final dot is the file extension; preserve it.
        var lastDot = logical.LastIndexOf('.');
        if (lastDot <= 0)
            return logical;

        var pathPart = logical.Substring(0, lastDot).Replace('.', '/');
        var extension = logical.Substring(lastDot);
        return pathPart + extension;
    }
}
