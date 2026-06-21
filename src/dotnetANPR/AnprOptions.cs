using Microsoft.Extensions.Logging;

namespace dotnetANPR;

/// <summary>
/// Options for constructing an <see cref="AnprEngine"/>.
/// </summary>
public sealed class AnprOptions
{
    /// <summary>
    /// Optional path to a configuration XML file. When omitted, built-in defaults are used
    /// and <c>Resources/config.xml</c> is loaded from the application base if present.
    /// </summary>
    public string? ConfigFilePath { get; init; }

    /// <summary>
    /// Optional logger factory for pipeline diagnostics.
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; init; }

    /// <summary>
    /// Optional override for the alphabet training images directory.
    /// </summary>
    public string? AlphabetPath { get; init; }

    /// <summary>
    /// Optional override for the syntax description XML file.
    /// </summary>
    public string? SyntaxFilePath { get; init; }

    /// <summary>
    /// Optional override for the neural network XML file.
    /// </summary>
    public string? NeuralNetworkPath { get; init; }
}
