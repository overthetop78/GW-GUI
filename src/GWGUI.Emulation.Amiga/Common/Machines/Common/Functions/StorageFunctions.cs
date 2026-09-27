using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var devices = Enumerable.Range(0, model.MaximumFloppyDrives)
            .Select(index => new EmulationMediaDevice(
                new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, index),
                EmulationMediaType.Floppy, [StorageSettingsFunctionsConstants.Adf, StorageSettingsFunctionsConstants.Adz, StorageSettingsFunctionsConstants.Dms, StorageSettingsFunctionsConstants.Fdi, StorageSettingsFunctionsConstants.Ipf, StorageSettingsFunctionsConstants.Scp],
                IsRemovable: true,
                DisplayLabel: $"DF{index}:",
                FloppyOptions: FloppyOptions(configuration, index),
                IsPermanent: index == 0 && model.HasBuiltInFloppyDrive,
                ConfigurationKind: EmulationStorageConfigurationKind.FloppyDrive))
            .Concat(model.SupportsHardDrives
                ? Enumerable.Range(0, model.MaximumHardDrives).Select(index => new EmulationMediaDevice(
                    new EmulationMediaSlot(EmulationMediaCategory.HardDisk, index),
                    EmulationMediaType.HardDisk, HardDiskFormats.All.Select(format => format.Extension).ToArray(), false,
                    DisplayLabel: $"DH{index}:", HardDiskFormats: HardDiskFormats.All,
                    ConfigurationKind: EmulationStorageConfigurationKind.HardDiskDrive)) : [])
            .Concat(model.HasCdDrive
                ? [new EmulationMediaDevice(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                    [StorageSettingsFunctionsConstants.Cue, StorageSettingsFunctionsConstants.Ccd, StorageSettingsFunctionsConstants.Chd, StorageSettingsFunctionsConstants.Nrg, StorageSettingsFunctionsConstants.Mds, StorageSettingsFunctionsConstants.Iso], DisplayLabel: StorageSettingsFunctionsConstants.CD0,
                    IsPermanent: true)] : [])
            .ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var floppyCount = Count(options, StorageSettingsFunctionsConstants.GwguiFloppyDriveCount,
            model.HasBuiltInFloppyDrive ? 1 : 0, model.MaximumFloppyDrives);
        var hardDriveCount = model.SupportsHardDrives
            ? Count(options, StorageSettingsFunctionsConstants.GwguiHardDriveCount, 0, model.MaximumHardDrives) : 0;
        var configured = Enumerable.Range(0, floppyCount)
            .Select(index => new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, index))
            .Concat(Enumerable.Range(0, hardDriveCount)
                .Select(index => new EmulationMediaSlot(EmulationMediaCategory.HardDisk, index)))
            .Concat(model.HasCdDrive ? [EmulationMediaSlot.Cd0] : [])
            .Concat(mounted.Select(media => media.Slot))
            .Distinct().ToArray();
        var settings = configured.Select(slot => DeviceSettings(options, slot)).ToArray();
        return new EmulationStorageSettings(devices, configured, mounted, settings);
    }

    private static int Count(IReadOnlyDictionary<string, string> options, string key,
        int fallback, int maximum) => options.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
        ? Math.Clamp(parsed, 0, maximum)
        : Math.Clamp(fallback, 0, maximum);

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>());
        options[StorageSettingsFunctionsConstants.GwguiFloppyDriveCount] = settings.ConfiguredSlots
            .Count(slot => slot.Category == EmulationMediaCategory.FloppyDrive).ToString();
        options[StorageSettingsFunctionsConstants.GwguiHardDriveCount] = settings.ConfiguredSlots
            .Count(slot => slot.Category == EmulationMediaCategory.HardDisk).ToString();
        options[StorageSettingsFunctionsConstants.GwguiCdDriveEnabled] = settings.ConfiguredSlots.Contains(EmulationMediaSlot.Cd0)
            ? StorageSettingsFunctionsConstants.Enabled : StorageSettingsFunctionsConstants.Disabled;
        foreach (var device in settings.DeviceSettings ?? [])
        {
            if (device.Floppy is not { } floppy) continue;
            options[$"gwgui_floppy_drive_model_{device.Slot.Index}"] = floppy.Model;
            options[SettingsConstants.OptionFloppySpeed] = floppy.Speed;
            options[SettingsConstants.OptionFloppyWriteProtection] = floppy.WriteProtected ? StorageSettingsFunctionsConstants.Enabled : StorageSettingsFunctionsConstants.Disabled;
            options[SettingsConstants.OptionFloppyWriteRedirect] = floppy.RedirectWrites ? StorageSettingsFunctionsConstants.Enabled : StorageSettingsFunctionsConstants.Disabled;
        }
        return configuration with
        {
            Options = options,
            InitialDiskPath = settings.MountedMedia.FirstOrDefault(media => media.Slot == EmulationMediaSlot.Floppy0)?.Path,
            Media = settings.MountedMedia.Select(media => new MediaConfiguration(media.Path, media.Type switch
            {
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.HardDisk => MediaCategory.HardDrive,
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), media.Type, null)
            }, IsReadOnly: media.IsReadOnly)).ToArray()
        };
    }

    private static FloppyDriveDialogOptions FloppyOptions(MachineConfiguration configuration, int index)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        return new FloppyDriveDialogOptions(
            [new FloppyDriveModelChoice(StorageSettingsFunctionsConstants.Value35dd, string.Empty, StorageSettingsFunctionsConstants.Value35DD, 901_120)],
            string.Empty, StorageSettingsFunctionsConstants.AdfAdzDmsFdiIpfScp, StorageSettingsFunctionsConstants.Adf);
    }

    private static EmulationStorageDeviceSettings DeviceSettings(IReadOnlyDictionary<string, string> options,
        EmulationMediaSlot slot)
    {
        if (slot.Category != EmulationMediaCategory.FloppyDrive)
            return new EmulationStorageDeviceSettings(slot);
        return new EmulationStorageDeviceSettings(slot, new FloppyDriveSettings(
            options.GetValueOrDefault($"gwgui_floppy_drive_model_{slot.Index}") ?? StorageSettingsFunctionsConstants.Value35dd,
            options.GetValueOrDefault(SettingsConstants.OptionFloppySpeed) ?? StorageSettingsFunctionsConstants.Value100,
            options.GetValueOrDefault(SettingsConstants.OptionFloppyWriteProtection) == StorageSettingsFunctionsConstants.Enabled,
            options.GetValueOrDefault(SettingsConstants.OptionFloppyWriteRedirect) == StorageSettingsFunctionsConstants.Enabled));
    }
}
