using System.Globalization;
using System.ComponentModel;
using System.Resources;
using System.Windows.Data;
using System.Windows.Markup;

namespace GWGUI.App.Localization;

[MarkupExtensionReturnType(typeof(object))]
public sealed class LocExtension(string key) : MarkupExtension
{
    public static IReadOnlyList<string> CatalogNames { get; } =
    [
        "Common", "Actions", "Errors", "Shell", "Menus", "Read", "Write", "Conversion",
        "Visualizer", "Explorer", "ExplorerWarnings", "Formats", "Advanced", "Tools", "Hardware", "HostTools",
        "Options", "Profiles", "Logs", "About"
    ];

    private static readonly IReadOnlyDictionary<string, ResourceManager> ResourcesByKey = BuildResourceIndex();
    public string Key { get; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new Binding($"[{Key}]") { Source = LocalizationSource.Instance, Mode = BindingMode.OneWay }
            .ProvideValue(serviceProvider);

    public static string Get(string key, params object[] arguments)
    {
        var value = ResourcesByKey.TryGetValue(key, out var resources)
            ? resources.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]"
            : $"[{key}]";
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }

    public static string GetInvariant(string key) =>
        ResourcesByKey.TryGetValue(key, out var resources)
            ? resources.GetString(key, CultureInfo.InvariantCulture) ?? $"[{key}]"
            : $"[{key}]";

    public static IReadOnlySet<string> GetDefinedKeys(CultureInfo culture)
    {
        return CatalogNames.SelectMany(catalog => GetDefinedKeys(catalog, culture)).ToHashSet(StringComparer.Ordinal);
    }

    public static IReadOnlySet<string> GetDefinedKeys(string catalog, CultureInfo culture)
    {
        var resources = CreateResourceManager(catalog);
        var set = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        return set is null ? new HashSet<string>() : set.Cast<System.Collections.DictionaryEntry>()
            .Select(entry => (string)entry.Key).ToHashSet(StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, ResourceManager> BuildResourceIndex()
    {
        var index = new Dictionary<string, ResourceManager>(StringComparer.Ordinal);
        foreach (var catalog in CatalogNames)
        {
            var resources = CreateResourceManager(catalog);
            var set = resources.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false)
                ?? throw new MissingManifestResourceException($"The neutral localization catalog '{catalog}' is missing.");
            foreach (System.Collections.DictionaryEntry entry in set)
                if (!index.TryAdd((string)entry.Key, resources))
                    throw new InvalidOperationException($"The localization key '{entry.Key}' exists in more than one catalog.");
        }
        return index;
    }

    private static ResourceManager CreateResourceManager(string catalog) =>
        new($"GWGUI.App.Resources.{catalog}", typeof(LocExtension).Assembly);
}

public sealed class LocalizationSource : INotifyPropertyChanged
{
    public static LocalizationSource Instance { get; } = new();
    public string this[string key] => LocExtension.Get(key);
    public event PropertyChangedEventHandler? PropertyChanged;
    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
}
