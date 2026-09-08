using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Sources;
using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Interfaces;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GWGUI.App.Localization.Extensions;

[MarkupExtensionReturnType(typeof(object))]
public sealed class LocExtension(string key) : MarkupExtension
{
    private static readonly IReadOnlyDictionary<string, ResourceManager> ResourcesByKey = BuildResourceIndex();
    public string Key { get; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        CreateBinding(Key).ProvideValue(serviceProvider);

    public static Binding CreateBinding(string key) =>
        new(nameof(LocalizationSource.Version))
        {
            Source = LocalizationSource.Instance,
            Mode = BindingMode.OneWay,
            Converter = new LocalizedValueConverter(key)
        };

    public static Binding CreateBinding(IEmulationModule module, string key) =>
        new(nameof(LocalizationSource.Version))
        {
            Source = LocalizationSource.Instance,
            Mode = BindingMode.OneWay,
            Converter = new LocalizedValueConverter(key, module as IEmulationModuleLocalization)
        };

    public static string GetForModule(IEmulationModule module, string key, params object[] arguments) =>
        GetLocalized(module as IEmulationModuleLocalization, key, arguments);

    public static string GetForModule(string? moduleId, string key, params object[] arguments) =>
        GetLocalized(EmulationModuleRegistry.FindLocalization(moduleId), key, arguments);

    internal static string GetLocalized(IEmulationModuleLocalization? localization,
        string key, params object[] arguments)
    {
        if (localization is null
            || !localization.TryGetString(key, LocalizationSource.Instance.UiCulture, out var value))
            return Get(key, arguments);
        return arguments.Length == 0 ? value : string.Format(LocalizationSource.Instance.Culture, value, arguments);
    }

    public static string GetInvariant(IEmulationModule module, string key) =>
        module is IEmulationModuleLocalization localization
        && localization.TryGetString(key, CultureInfo.InvariantCulture, out var value)
            ? value : GetInvariant(key);

    public static string Get(string key, params object[] arguments)
    {
        var value = ResourcesByKey.TryGetValue(key, out var resources)
            ? resources.GetString(key, LocalizationSource.Instance.UiCulture) ?? $"[{key}]"
            : $"[{key}]";
        return arguments.Length == 0 ? value : string.Format(LocalizationSource.Instance.Culture, value, arguments);
    }

    public static string GetInvariant(string key) =>
        ResourcesByKey.TryGetValue(key, out var resources)
            ? resources.GetString(key, CultureInfo.InvariantCulture) ?? $"[{key}]"
            : $"[{key}]";

    public static IReadOnlySet<string> GetDefinedKeys(CultureInfo culture)
    {
        return LocalizationCatalogNames.All.SelectMany(catalog => GetDefinedKeys(catalog, culture))
            .ToHashSet(StringComparer.Ordinal);
    }

    public static IReadOnlySet<string> GetDefinedKeys(string catalog, CultureInfo culture)
    {
        var resources = CreateResourceManager(catalog);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var neutral = resources.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false);
        if (neutral is not null) keys.UnionWith(neutral.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key));
        if (!culture.Equals(CultureInfo.InvariantCulture))
        {
            var localized = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
            if (localized is not null) keys.UnionWith(localized.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key));
        }
        return keys;
    }

    private static IReadOnlyDictionary<string, ResourceManager> BuildResourceIndex()
    {
        var index = new Dictionary<string, ResourceManager>(StringComparer.Ordinal);
        foreach (var catalog in LocalizationCatalogNames.All)
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

    private sealed class LocalizedValueConverter(string resourceKey,
        IEmulationModuleLocalization? localization = null) : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            GetLocalized(localization, resourceKey);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            DependencyProperty.UnsetValue;
    }
}
