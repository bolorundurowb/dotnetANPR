using Microsoft.Extensions.Logging;

namespace dotnetANPR.Utilities;

public static class Logging
{
    public static ILogger<T> GetLogger<T>() => LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<T>();
}
