using GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;
using GWGUI.Emulation.Apple.Emulators.AppleWin.Dictionaries;
using NativeVideo = GWGUI.Emulation.Apple.Emulators.AppleWin.Constants.VideoConstants;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Functions;

internal static class ConfigurationFunctions
{
    internal static void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options)
    {
        if (configuration.Family == MachineFamily.AppleII) options.TryAdd(OptionConstants.Machine, AppleWinModelCatalog.GetNativeValue(configuration.Model));
        ExpansionFunctions.InitializeStorage(configuration, options);
        options[OptionConstants.VideoMode] = NativeVideo.SourceMode;
        options[OptionConstants.VideoStyle] = NativeVideo.SourceStyle;
    }

    internal static void ValidateConfiguration(MachineConfiguration configuration)
    {
        foreach (var option in AppleWinOptionCatalog.All)
            if (configuration.Options.TryGetValue(option.Key, out var value) && !option.Values.Any(choice => string.Equals(choice.Value, value, StringComparison.Ordinal))) throw new ArgumentException(nameof(configuration.Options));
        if (configuration.Options.TryGetValue(OptionConstants.Machine, out var nativeModel) && !string.Equals(nativeModel, AppleWinModelCatalog.GetNativeValue(configuration.Model), StringComparison.Ordinal)) throw new ArgumentException(nameof(configuration.Model));
        if (configuration.Firmwares.Count != 0) throw new ArgumentException(nameof(configuration.Firmwares));
    }
}
