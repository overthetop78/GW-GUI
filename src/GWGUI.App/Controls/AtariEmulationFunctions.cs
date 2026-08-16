using System.Globalization;
using System.IO;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariEmulationFunctions
{
    internal static void ValidateConfiguration(AtariMachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var required = AtariFirmwareCatalog.ForModel(configuration.Model)
            .Where(item => item.RequiresExternalFile && item.Kind is not null)
            .Select(item => item.Kind!.Value)
            .Distinct()
            .ToArray();
        foreach (var kind in required)
        {
            var configured = configuration.Firmwares.FirstOrDefault(item => item.Kind == kind);
            if (configured is null)
                throw new AtariEmulationException(AtariErrorKind.Firmware, AtariErrorCode.FirmwareMissing,
                    string.Format(CultureInfo.CurrentCulture, AtariEmulationConstants.MissingFirmwareFormat,
                        kind, configuration.Model));
        }
        foreach (var firmware in configuration.Firmwares)
            if (!File.Exists(firmware.Path))
                throw new AtariEmulationException(AtariErrorKind.Firmware, AtariErrorCode.FirmwareMissing,
                    string.Format(CultureInfo.CurrentCulture, AtariEmulationConstants.MissingFirmwareFileFormat,
                        firmware.Kind, firmware.Path));
        foreach (var media in configuration.Media)
            if (!File.Exists(media.Path) && !Directory.Exists(media.Path))
                throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentNotFound,
                    string.Format(CultureInfo.CurrentCulture, AtariEmulationConstants.MissingMediaFileFormat,
                        media.Path));
        if (string.IsNullOrWhiteSpace(Environment.ProcessPath))
            throw new AtariEmulationException(AtariErrorKind.Host, AtariErrorCode.HostProtocolFailure,
                AtariEmulationConstants.MissingHostExecutable);
    }

    internal static string DisplayName(AtariMachineConfiguration configuration, string modelName) =>
        $"{modelName} · {configuration.Id.ToString(AtariEmulationConstants.IdentifierFormat)
            [..AtariEmulationConstants.DisplayIdentifierLength]}";
}
