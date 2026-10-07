using GWGUI.Emulation.Commodore.Emulators.Common.Constants;
using System.IO;
using System.Globalization;
using GWGUI.Emulation.Commodore.Emulators.Amiberry.Constants;

namespace GWGUI.Emulation.Commodore.Emulators.Amiberry.Factories;

internal sealed class AmiberryMachineFactory : AmigaMachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.Amiberry,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorDescriptionConstants.AmigaDescriptionResourceKey,
            SupportedMachines));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl);
    internal override string? FirmwareOverrideOption => OptionConstants.Kickstart;
    internal override string ExtendedRomFileName(string model, string path) =>
        model.Equals(OptionConstants.Cd32, StringComparison.OrdinalIgnoreCase)
            ? OptionConstants.Cd32ExtendedRom : OptionConstants.CdtvExtendedRom;

    internal override IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration,
        IReadOnlyList<MediaConfiguration> media)
    {
        var options = (configuration.Options ?? new Dictionary<string, string>())
            .Where(pair => pair.Key.StartsWith(OptionConstants.Prefix, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        options[OptionConstants.Model] = configuration.Model is OptionConstants.A2000 or OptionConstants.A3000
            ? OptionConstants.A500Plus : ((AmigaModel)ModelCatalog.Get(configuration.Model)).BackendModel;
        options[OptionConstants.Kickstart] = Path.GetFullPath(configuration.FirmwarePath(SettingsConstants.KickstartPath));
        if (configuration.Options?.TryGetValue(SettingsConstants.OptionVideoStandard, out var standard) == true)
            options[OptionConstants.VideoStandard] = standard.StartsWith(SettingsValueConstants.NTSC,
                StringComparison.OrdinalIgnoreCase) ? OptionConstants.NtSc : OptionConstants.Pal;
        return options;
    }

    internal override string? PrepareContent(MachineConfiguration configuration, string session,
        IReadOnlyList<MediaConfiguration> media) => PrepareContentPath(configuration, session, media, createHardwareConfiguration: true);
}
