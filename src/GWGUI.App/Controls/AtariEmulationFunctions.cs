using System.IO;
using GWGUI.App.Localization;
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
                    LocExtension.Get(AtariEmulationConstants.MissingFirmwareResource,
                        kind, configuration.Model), new Dictionary<string, string>
                    {
                        [AtariEmulationConstants.FirmwareRoleContextKey] = kind.ToString(),
                        [AtariEmulationConstants.ModelContextKey] = configuration.Model.ToString()
                    });
        }
        foreach (var firmware in configuration.Firmwares)
            if (!File.Exists(firmware.Path))
                throw new AtariEmulationException(AtariErrorKind.Firmware, AtariErrorCode.FirmwareMissing,
                    LocExtension.Get(AtariEmulationConstants.MissingFirmwareFileResource,
                        firmware.Kind, firmware.Path), new Dictionary<string, string>
                    {
                        [AtariEmulationConstants.FirmwareRoleContextKey] = firmware.Kind.ToString(),
                        [AtariEmulationConstants.PathContextKey] = firmware.Path
                    });
        foreach (var media in configuration.Media)
            if (!File.Exists(media.Path) && !Directory.Exists(media.Path))
                throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentNotFound,
                    LocExtension.Get(AtariEmulationConstants.MissingMediaFileResource, media.Path),
                    new Dictionary<string, string> { [AtariEmulationConstants.PathContextKey] = media.Path });
        if (string.IsNullOrWhiteSpace(Environment.ProcessPath))
            throw new AtariEmulationException(AtariErrorKind.Host, AtariErrorCode.HostProtocolFailure,
                LocExtension.Get(AtariEmulationConstants.MissingHostExecutableResource));
    }

    internal static string DisplayName(AtariMachineConfiguration configuration, string modelName) =>
        EmulationConfigurationDisplayFunctions.Atari(configuration, modelName);
}
