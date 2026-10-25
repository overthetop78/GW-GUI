using System.IO;
using GWGUI.Emulation.Sinclair.Emulators.Fuse.Constants;
using GWGUI.Emulation.Sinclair.Emulators.Fuse.Dictionaries;

namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Defaults(MachineModel model)
    {
        if (!RuntimeOptionConstants.Models.TryGetValue(model, out var nativeModel)) throw new ArgumentOutOfRangeException(nameof(model));
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        result[OptionConstants.Machine] = nativeModel;
        result[OptionConstants.AutoMachine] = RuntimeOptionConstants.Disabled;
        return result;
    }

    internal static bool IsAvailable(MachineConfiguration configuration, string key)
    {
        if (!RuntimeOptionConstants.Models.ContainsKey(configuration.Model) || !OptionCatalog.Options.Any(option => option.Key == key)) return false;
        return key != OptionConstants.Uspeech || HasSpeechFirmware(configuration, FirmwareCategory.USpeech) && HasSpeechFirmware(configuration, FirmwareCategory.Sp0256);
    }

    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = new Dictionary<string, string>(Defaults(configuration.Model), StringComparer.Ordinal);
        foreach (var option in OptionCatalog.Options.Where(option => option.IsVisible)) if (configuration.Options.TryGetValue(option.Key, out var value)) result[option.Key] = value;
        result[OptionConstants.Machine] = RuntimeOptionConstants.Models[configuration.Model];
        result[OptionConstants.AutoMachine] = RuntimeOptionConstants.Disabled;
        result[OptionConstants.AutoSizeSavestate] = RuntimeOptionConstants.Enabled;
        if (!IsAvailable(configuration, OptionConstants.Uspeech)) result[OptionConstants.Uspeech] = RuntimeOptionConstants.Disabled;
        return result;
    }

    private static bool HasSpeechFirmware(MachineConfiguration configuration, FirmwareCategory role)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == role);
        return firmware is not null && File.Exists(firmware.Path) && new FileInfo(firmware.Path).Length == FirmwareConstants.SpeechBankBytes && string.Equals(Path.GetFileName(firmware.Path), FirmwareFunctions.FileName(role), StringComparison.OrdinalIgnoreCase);
    }
}
