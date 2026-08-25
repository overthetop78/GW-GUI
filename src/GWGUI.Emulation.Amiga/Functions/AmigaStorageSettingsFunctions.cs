using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Functions;

internal static class AmigaStorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(AmigaMachineConfiguration configuration)
    {
        var model = AmigaModelCatalog.Get(configuration.Model);
        var devices = Enumerable.Range(0, model.MaximumFloppyDrives)
            .Select(index => new EmulationMediaDevice(
                new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, index),
                EmulationMediaType.Floppy, [AmigaStorageSettingsFunctionsConstants.Adf, AmigaStorageSettingsFunctionsConstants.Adz, AmigaStorageSettingsFunctionsConstants.Dms, AmigaStorageSettingsFunctionsConstants.Fdi, AmigaStorageSettingsFunctionsConstants.Ipf, AmigaStorageSettingsFunctionsConstants.Scp],
                IsRemovable: true,
                DisplayLabel: $"DF{index}:",
                FloppyOptions: FloppyOptions(configuration, index),
                IsPermanent: index == 0 && model.HasBuiltInFloppyDrive))
            .Concat(model.SupportsHardDrives
                ? Enumerable.Range(0, model.MaximumHardDrives).Select(index => new EmulationMediaDevice(
                    new EmulationMediaSlot(EmulationMediaCategory.HardDisk, index),
                    EmulationMediaType.HardDisk, [AmigaStorageSettingsFunctionsConstants.Hdf, AmigaStorageSettingsFunctionsConstants.Hdz], false,
                    DisplayLabel: $"DH{index}:")) : [])
            .Concat(model.HasCdDrive
                ? [new EmulationMediaDevice(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                    [AmigaStorageSettingsFunctionsConstants.Cue, AmigaStorageSettingsFunctionsConstants.Ccd, AmigaStorageSettingsFunctionsConstants.Chd, AmigaStorageSettingsFunctionsConstants.Nrg, AmigaStorageSettingsFunctionsConstants.Mds, AmigaStorageSettingsFunctionsConstants.Iso], DisplayLabel: AmigaStorageSettingsFunctionsConstants.CD0,
                    IsPermanent: true)] : [])
            .ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var floppyCount = Count(options, AmigaStorageSettingsFunctionsConstants.GwguiFloppyDriveCount,
            model.HasBuiltInFloppyDrive ? 1 : 0, model.MaximumFloppyDrives);
        var hardDriveCount = model.SupportsHardDrives
            ? Count(options, AmigaStorageSettingsFunctionsConstants.GwguiHardDriveCount, 0, model.MaximumHardDrives) : 0;
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

    internal static AmigaMachineConfiguration Apply(AmigaMachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>());
        options[AmigaStorageSettingsFunctionsConstants.GwguiFloppyDriveCount] = settings.ConfiguredSlots
            .Count(slot => slot.Category == EmulationMediaCategory.FloppyDrive).ToString();
        options[AmigaStorageSettingsFunctionsConstants.GwguiHardDriveCount] = settings.ConfiguredSlots
            .Count(slot => slot.Category == EmulationMediaCategory.HardDisk).ToString();
        options[AmigaStorageSettingsFunctionsConstants.GwguiCdDriveEnabled] = settings.ConfiguredSlots.Contains(EmulationMediaSlot.Cd0)
            ? AmigaStorageSettingsFunctionsConstants.Enabled : AmigaStorageSettingsFunctionsConstants.Disabled;
        foreach (var device in settings.DeviceSettings ?? [])
        {
            if (device.Floppy is not { } floppy) continue;
            options[$"gwgui_floppy_drive_model_{device.Slot.Index}"] = floppy.Model;
            options[AmigaStorageSettingsFunctionsConstants.OptionFloppySpeed] = floppy.Speed;
            options[AmigaStorageSettingsFunctionsConstants.OptionFloppyWriteProtection] = floppy.WriteProtected ? AmigaStorageSettingsFunctionsConstants.Enabled : AmigaStorageSettingsFunctionsConstants.Disabled;
            options[AmigaStorageSettingsFunctionsConstants.OptionFloppyWriteRedirect] = floppy.RedirectWrites ? AmigaStorageSettingsFunctionsConstants.Enabled : AmigaStorageSettingsFunctionsConstants.Disabled;
        }
        return configuration with
        {
            Options = options,
            InitialDiskPath = settings.MountedMedia.FirstOrDefault(media => media.Slot == EmulationMediaSlot.Floppy0)?.Path,
            Media = settings.MountedMedia.Select(media => new AmigaMediaConfiguration(media.Path, media.Type switch
            {
                EmulationMediaType.Floppy => AmigaMediaCategory.Floppy,
                EmulationMediaType.HardDisk => AmigaMediaCategory.HardDrive,
                EmulationMediaType.CompactDisc => AmigaMediaCategory.CompactDisc,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), media.Type, null)
            }, IsReadOnly: media.IsReadOnly)).ToArray()
        };
    }

    private static FloppyDriveDialogOptions FloppyOptions(AmigaMachineConfiguration configuration, int index)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        return new FloppyDriveDialogOptions(
            [new FloppyDriveModelChoice(AmigaStorageSettingsFunctionsConstants.Value35dd, string.Empty, AmigaStorageSettingsFunctionsConstants.Value35DD, 901_120)],
            string.Empty, AmigaStorageSettingsFunctionsConstants.AdfAdzDmsFdiIpfScp, AmigaStorageSettingsFunctionsConstants.Adf);
    }

    private static EmulationStorageDeviceSettings DeviceSettings(IReadOnlyDictionary<string, string> options,
        EmulationMediaSlot slot)
    {
        if (slot.Category != EmulationMediaCategory.FloppyDrive)
            return new EmulationStorageDeviceSettings(slot);
        return new EmulationStorageDeviceSettings(slot, new FloppyDriveSettings(
            options.GetValueOrDefault($"gwgui_floppy_drive_model_{slot.Index}") ?? AmigaStorageSettingsFunctionsConstants.Value35dd,
            options.GetValueOrDefault(AmigaStorageSettingsFunctionsConstants.OptionFloppySpeed) ?? AmigaStorageSettingsFunctionsConstants.Value100,
            options.GetValueOrDefault(AmigaStorageSettingsFunctionsConstants.OptionFloppyWriteProtection) == AmigaStorageSettingsFunctionsConstants.Enabled,
            options.GetValueOrDefault(AmigaStorageSettingsFunctionsConstants.OptionFloppyWriteRedirect) == AmigaStorageSettingsFunctionsConstants.Enabled));
    }
}
