namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

internal static class GreaseweazleMediaOptionFunctions
{
    internal static string ReadRequired(IReadOnlyDictionary<string, string> options, string key) => options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException($"Required Greaseweazle option '{key}' is missing.", nameof(options));

    internal static T ReadEnum<T>(IReadOnlyDictionary<string, string> options, string key) where T : struct, Enum => Enum.TryParse<T>(ReadRequired(options, key), true, out var parsed) ? parsed : throw new ArgumentException($"Invalid {typeof(T).Name} value for option '{key}'.", nameof(options));
}
