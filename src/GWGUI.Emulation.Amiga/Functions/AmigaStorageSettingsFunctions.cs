using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static class AmigaStorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(AmigaMachineConfiguration configuration)
    {
        var model = AmigaModelCatalog.Get(configuration.Model);
        var devices = Enumerable.Range(0, model.MaximumFloppyDrives)
            .Select(index => new EmulationMediaDevice(
                new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, index),
                EmulationMediaType.Floppy, [".adf", ".adz", ".dms", ".fdi", ".ipf", ".scp"],
                IsRemovable: true,
                DisplayLabel: $"DF{index}:",
                FloppyOptions: FloppyOptions(configuration, index),
                IsPermanent: index == 0))
            .Concat(model.SupportsHardDrives
                ? Enumerable.Range(0, model.MaximumHardDrives).Select(index => new EmulationMediaDevice(
                    new EmulationMediaSlot(EmulationMediaCategory.HardDisk, index),
                    EmulationMediaType.HardDisk, [".hdf", ".hdz"], false,
                    DisplayLabel: $"DH{index}:")) : [])
            .Concat(model.HasCdDrive
                ? [new EmulationMediaDevice(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                    [".cue", ".ccd", ".chd", ".nrg", ".mds", ".iso"], DisplayLabel: "CD0:")] : [])
            .ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var floppyCount = Count(options, "gwgui_floppy_drive_count", 1, model.MaximumFloppyDrives);
        var hardDriveCount = model.SupportsHardDrives
            ? Count(options, "gwgui_hard_drive_count", 0, model.MaximumHardDrives) : 0;
        var configured = Enumerable.Range(0, floppyCount)
            .Select(index => new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, index))
            .Concat(Enumerable.Range(0, hardDriveCount)
                .Select(index => new EmulationMediaSlot(EmulationMediaCategory.HardDisk, index)))
            .Concat(model.HasCdDrive && options.GetValueOrDefault("gwgui_cd_drive_enabled") == "enabled"
                ? [EmulationMediaSlot.Cd0] : [])
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
        options["gwgui_floppy_drive_count"] = settings.ConfiguredSlots
            .Count(slot => slot.Category == EmulationMediaCategory.FloppyDrive).ToString();
        options["gwgui_hard_drive_count"] = settings.ConfiguredSlots
            .Count(slot => slot.Category == EmulationMediaCategory.HardDisk).ToString();
        options["gwgui_cd_drive_enabled"] = settings.ConfiguredSlots.Contains(EmulationMediaSlot.Cd0)
            ? "enabled" : "disabled";
        foreach (var device in settings.DeviceSettings ?? [])
        {
            if (device.Floppy is not { } floppy) continue;
            options[$"gwgui_floppy_drive_model_{device.Slot.Index}"] = floppy.Model;
            options["puae_floppy_speed"] = floppy.Speed;
            options["puae_floppy_write_protection"] = floppy.WriteProtected ? "enabled" : "disabled";
            options["puae_floppy_write_redirect"] = floppy.RedirectWrites ? "enabled" : "disabled";
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
            [new FloppyDriveModelChoice("35dd", string.Empty, "3.5\" DD", 901_120)],
            string.Empty, "*.adf;*.adz;*.dms;*.fdi;*.ipf;*.scp", ".adf");
    }

    private static EmulationStorageDeviceSettings DeviceSettings(IReadOnlyDictionary<string, string> options,
        EmulationMediaSlot slot)
    {
        if (slot.Category != EmulationMediaCategory.FloppyDrive)
            return new EmulationStorageDeviceSettings(slot);
        return new EmulationStorageDeviceSettings(slot, new FloppyDriveSettings(
            options.GetValueOrDefault($"gwgui_floppy_drive_model_{slot.Index}") ?? "35dd",
            options.GetValueOrDefault("puae_floppy_speed") ?? "100",
            options.GetValueOrDefault("puae_floppy_write_protection") == "enabled",
            options.GetValueOrDefault("puae_floppy_write_redirect") == "enabled"));
    }
}
