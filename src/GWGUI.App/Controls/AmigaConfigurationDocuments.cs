using System.IO;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

internal sealed class AmigaConfigurationDocuments(string directory, string pathBase)
{
    private readonly AmigaConfigurationStore _store = new(directory, pathBase);

    internal Task<IReadOnlyList<AmigaMachineConfiguration>> LoadAllAsync() => _store.LoadAllAsync();
    internal Task SaveAsync(AmigaMachineConfiguration configuration) => _store.SaveAsync(configuration);
    internal void Delete(Guid id) => _store.Delete(id);

    internal static string GetOption(AmigaMachineConfiguration configuration, string key, string fallback) =>
        configuration.Options?.GetValueOrDefault(key) ?? fallback;

    internal static void SelectOption(ComboBox comboBox, AmigaMachineConfiguration configuration, string key,
        string? fallback)
    {
        var value = GetOption(configuration, key, fallback ?? string.Empty);
        ComboBoxSelection.SelectByValue<object>(comboBox, value,
            item => item is OptionChoice choice ? choice.Value : item.ToString());
    }

    internal static string? OptionalFullPath(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : Path.GetFullPath(value);

    internal static void ValidateOptionalFile(string value, bool required = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required) throw new FileNotFoundException(LocExtension.Get("Emulation.FileRequired"));
            return;
        }
        if (!File.Exists(value)) throw new FileNotFoundException(LocExtension.Get("Emulation.FileMissing"), value);
    }

    internal static void ValidateOptionalMedia(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!File.Exists(value) && !Directory.Exists(value))
            throw new FileNotFoundException(LocExtension.Get("Emulation.FileMissing"), value);
    }

    internal static bool IsEncryptedKickstart(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        try
        {
            Span<byte> header = stackalloc byte[11];
            using var stream = File.OpenRead(path);
            return stream.Read(header) == header.Length &&
                System.Text.Encoding.ASCII.GetString(header) == "AMIROMTYPE1";
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }
}
