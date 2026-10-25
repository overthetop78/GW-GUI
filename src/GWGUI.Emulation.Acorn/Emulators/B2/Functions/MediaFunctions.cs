using GWGUI.Emulation.Acorn.Emulators.B2.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Functions;

internal static class MediaDeviceFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Describe(MachineConfiguration configuration) =>
        [new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions,
            RequiresMachineRecreation: true, DisplayLabel: MediaRuntimeConstants.DriveLabel,
            ImageDirectory: configuration.Folders.Floppies, IsPermanent: true,
            FloppyOptions: new([new(nameof(EmulationMediaType.Floppy), "Emulation.Storage.Floppy.Drive")],
                configuration.Folders.Floppies ?? string.Empty, string.Empty, MediaRuntimeConstants.FloppyExtensions[0],
                CanCreateBlankMedia: false, CanConfigureBehavior: false),
            ConfigurationKind: EmulationStorageConfigurationKind.FloppyDrive,
            ConfigurationFields: EmulatorSettingsFunctions.DeviceFields(configuration))];

    internal static IReadOnlyList<EmulationStorageDeviceSettings> Settings(MachineConfiguration configuration) =>
        [new(EmulationMediaSlot.Floppy0, new FloppyDriveSettings(nameof(EmulationMediaType.Floppy), "100", false, false,
            EmulatorSettingsFunctions.DeviceFields(configuration).ToDictionary(field => field.Id, field => field.Value ?? string.Empty, StringComparer.Ordinal)))];

    internal static MachineConfiguration Apply(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings)
    {
        var floppy = settings.SingleOrDefault(item => item.Slot == EmulationMediaSlot.Floppy0)?.Floppy;
        if (floppy is null) return configuration;
        if (floppy.Model != nameof(EmulationMediaType.Floppy) || floppy.Speed != "100" || floppy.WriteProtected || floppy.RedirectWrites)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var fields = EmulatorSettingsFunctions.DeviceFields(configuration).ToDictionary(field => field.Id, StringComparer.Ordinal);
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        foreach (var option in floppy.Options ?? new Dictionary<string, string>())
        {
            if (!fields.ContainsKey(option.Key)) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(settings));
            options[option.Key] = option.Value;
        }
        return configuration with { Options = ConfigurationOptionFunctions.Normalize(configuration.Model, options) };
    }
}
