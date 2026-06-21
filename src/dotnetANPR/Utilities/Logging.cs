using Microsoft.Extensions.Logging;

namespace dotnetANPR.Utilities;

internal static class Logging
{
    private static ILoggerFactory? _factory;

    public static void Configure(ILoggerFactory? factory) => _factory = factory;

    public static ILoggerFactory Factory =>
        _factory ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

    public static ILogger<T> GetLogger<T>() => Factory.CreateLogger<T>();

    public static ILogger GetLogger(string categoryName) => Factory.CreateLogger(categoryName);
}
