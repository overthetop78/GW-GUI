using System.Globalization;
using System.ComponentModel;
using System.Resources;
using System.Windows.Data;
using System.Windows.Markup;

namespace GWGUI.App.Localization;

[MarkupExtensionReturnType(typeof(object))]
public sealed class LocExtension(string key) : MarkupExtension
{
    private static readonly ResourceManager Resources = new("GWGUI.App.Resources.Strings", typeof(LocExtension).Assembly);
    public string Key { get; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new Binding($"[{Key}]") { Source = LocalizationSource.Instance, Mode = BindingMode.OneWay }
            .ProvideValue(serviceProvider);

    public static string Get(string key, params object[] arguments)
    {
        var value = Resources.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]";
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }

    public static IReadOnlySet<string> GetDefinedKeys(CultureInfo culture)
    {
        var set = Resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        return set is null ? new HashSet<string>() : set.Cast<System.Collections.DictionaryEntry>().Select(x => (string)x.Key).ToHashSet(StringComparer.Ordinal);
    }
}

public sealed class LocalizationSource : INotifyPropertyChanged
{
    public static LocalizationSource Instance { get; } = new();
    public string this[string key] => LocExtension.Get(key);
    public event PropertyChangedEventHandler? PropertyChanged;
    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
}
