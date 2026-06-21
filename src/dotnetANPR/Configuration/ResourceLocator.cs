using System;
using System.IO;
using System.Reflection;

namespace dotnetANPR.Configuration;

/// <summary>
/// Resolves relative resource paths against the application base directory and assembly location.
/// </summary>
internal sealed class ResourceLocator
{
    private readonly string _baseDirectory;

    public ResourceLocator(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory ?? AppContext.BaseDirectory;
    }

    public string Resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var normalized = path.Replace('/', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalized))
            return Path.GetFullPath(normalized);

        var fromBase = Path.GetFullPath(Path.Combine(_baseDirectory, normalized));
        if (File.Exists(fromBase) || Directory.Exists(fromBase))
            return fromBase;

        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!string.IsNullOrEmpty(assemblyDir))
        {
            var fromAssembly = Path.GetFullPath(Path.Combine(assemblyDir, normalized));
            if (File.Exists(fromAssembly) || Directory.Exists(fromAssembly))
                return fromAssembly;
        }

        return fromBase;
    }
}
