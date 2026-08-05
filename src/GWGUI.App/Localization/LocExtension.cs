using System.Globalization;
using System.Resources;
using System.Windows.Markup;

namespace GWGUI.App.Localization;

[MarkupExtensionReturnType(typeof(string))]
public sealed class LocExtension(string key) : MarkupExtension
{
    private static readonly ResourceManager Resources = new("GWGUI.App.Resources.Strings", typeof(LocExtension).Assembly);
    public string Key { get; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider) => Get(Key);

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
