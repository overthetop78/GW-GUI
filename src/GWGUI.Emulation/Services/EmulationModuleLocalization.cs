using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace GWGUI.Emulation.Services;

public sealed class EmulationModuleLocalization(Assembly assembly, string resourcePrefix)
    : IEmulationModuleLocalization
{
    private const string BaseCulture = "00-Base";
    private const string FallbackCulture = "en-US";
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _catalogs =
        new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetString(string key, CultureInfo culture, out string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(culture);
        if (!culture.Equals(CultureInfo.InvariantCulture))
        {
            for (var candidate = culture; !candidate.Equals(CultureInfo.InvariantCulture);
                 candidate = candidate.Parent)
                if (TryGet(candidate.Name, key, out value)) return true;
            if (TryGet(FallbackCulture, key, out value)) return true;
        }
        return TryGet(BaseCulture, key, out value);
    }

    private bool TryGet(string culture, string key, out string value)
    {
        if (_catalogs.GetOrAdd(culture, LoadCatalog).TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }
        value = string.Empty;
        return false;
    }

    private IReadOnlyDictionary<string, string> LoadCatalog(string culture)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        using var stream = assembly.GetManifestResourceStream($"{resourcePrefix}.{culture}.resources");
        if (stream is null) return result;
        using var reader = new ResourceReader(stream);
        foreach (DictionaryEntry entry in reader)
            if (entry.Key is string key && entry.Value is string value)
                result.Add(key, value);
        return result;
    }
}
