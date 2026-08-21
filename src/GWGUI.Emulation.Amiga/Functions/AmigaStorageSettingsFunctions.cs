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
                DisplayLabel: $"DF{index}:"))
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
        var configured = mounted.Select(media => media.Slot).Append(EmulationMediaSlot.Floppy0).Distinct().ToArray();
        return new EmulationStorageSettings(devices, configured, mounted);
    }

    internal static AmigaMachineConfiguration Apply(AmigaMachineConfiguration configuration,
        EmulationStorageSettings settings) => configuration with
    {
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
