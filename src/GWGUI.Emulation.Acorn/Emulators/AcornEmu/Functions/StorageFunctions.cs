using System.IO;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Descriptions et persistance des lecteurs natifs par les contrats de stockage déjà utilisés.</summary>
internal static partial class StorageFunctions
{
    internal static bool Connected(MachineConfiguration configuration, int drive) => configuration.Options.TryGetValue(C.Key(drive, "connected"), out var value) ? bool.Parse(value) : configuration.Media.Any(media => media.Slot == new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, drive));
    internal static bool HasInterface(MachineConfiguration configuration) => Enumerable.Range(0, C.DriveCount).Any(drive => Connected(configuration, drive));
    internal static bool UsesWd1770(MachineConfiguration configuration) => configuration.Options.GetValueOrDefault(C.ControllerOption, C.Intel8271) == C.AcornWd1770;
    internal static IReadOnlyList<string> Extensions(MachineConfiguration configuration) => UsesWd1770(configuration) ? C.DoubleDensityExtensions : C.Extensions;
    internal static FloppyDriveSettings Settings(MachineConfiguration configuration, int drive) => new(configuration.Options.GetValueOrDefault(C.Key(drive, "model"), C.DefaultModel), configuration.Options.GetValueOrDefault(C.Key(drive, "speed"), "100"), bool.Parse(configuration.Options.GetValueOrDefault(C.Key(drive, "protected"), bool.FalseString)), bool.Parse(configuration.Options.GetValueOrDefault(C.Key(drive, "redirect"), bool.FalseString)));

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineConfiguration configuration) => Enumerable.Range(0, C.DriveCount).Select(drive => new EmulationMediaDevice(new(EmulationMediaCategory.FloppyDrive, drive), EmulationMediaType.Floppy, Extensions(configuration),
        FloppyOptions: new(CreationModels(configuration), configuration.Folders.Floppies ?? string.Empty, "SSD|*.ssd|DSD|*.dsd", ".dsd", CanCreateBlankMedia: true), ConfigurationKind: EmulationStorageConfigurationKind.FloppyDrive)).ToArray();

    private static IReadOnlyList<FloppyDriveModelChoice> CreationModels(MachineConfiguration configuration)
    {
        if (!UsesWd1770(configuration)) return C.Models;
        return C.Models.Select(model =>
        {
            var formats = new Dictionary<string, string>(model.BlankFormats!, StringComparer.OrdinalIgnoreCase) { [".ads"] = "acorn.adfs.160" };
            if (model.Value is C.Single80 or C.Double80) formats[".adm"] = "acorn.adfs.320";
            if (model.Value == C.Double80) formats[".adl"] = "acorn.adfs.640";
            return model with { BlankFormats = formats };
        }).ToArray();
    }
    internal static IReadOnlyList<EmulationStorageDeviceSettings> DescribeSettings(MachineConfiguration configuration) => Enumerable.Range(0, C.DriveCount).Where(drive => Connected(configuration, drive)).Select(drive => new EmulationStorageDeviceSettings(new(EmulationMediaCategory.FloppyDrive, drive), Settings(configuration, drive))).ToArray();

    internal static MachineConfiguration SelectSlots(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots)
    {
        if (slots.Distinct().Count() != slots.Count) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(slots));
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        for (var drive = 0; drive < C.DriveCount; drive++) options[C.Key(drive, "connected")] = slots.Contains(new(EmulationMediaCategory.FloppyDrive, drive)).ToString();
        return SelectHardDiskSlots(configuration with { Options = options }, slots);
    }

    internal static MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        foreach (var entry in settings)
        {
            if (entry.Floppy is not { } floppy) continue;
            if (entry.Slot.Category != EmulationMediaCategory.FloppyDrive || entry.Slot.Index is < 0 or >= C.DriveCount || !C.Models.Any(model => model.Value == floppy.Model) || !C.Speeds.Contains(floppy.Speed) || floppy.Options?.Count > 0) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
            options[C.Key(entry.Slot.Index, "model")] = floppy.Model; options[C.Key(entry.Slot.Index, "speed")] = floppy.Speed;
            options[C.Key(entry.Slot.Index, "protected")] = floppy.WriteProtected.ToString(); options[C.Key(entry.Slot.Index, "redirect")] = floppy.RedirectWrites.ToString();
        }
        return ApplyHardDiskSettings(configuration with { Options = options }, settings);
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        ValidateHardDisks(configuration);
        if (configuration.Options.GetValueOrDefault(C.ControllerOption, C.Intel8271) is not (C.Intel8271 or C.AcornWd1770)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        for (var drive = 0; drive < C.DriveCount; drive++)
        {
            _ = Connected(configuration, drive);
            var settings = Settings(configuration, drive);
            if (!C.Models.Any(model => model.Value == settings.Model) || !C.Speeds.Contains(settings.Speed)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        }
    }

    internal static void ValidateMedia(IEmulationDiskImage image, FloppyDriveSettings settings)
    {
        var cylinders = settings.Model is C.Single40 or C.Double40 ? 40 : 80;
        var heads = settings.Model is C.Single40 or C.Single80 ? 1 : 2;
        if (image.SectorSize != 256 || image.SectorsPerTrack is not (10 or 16) || image.Cylinders > cylinders || image.Heads > heads) throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
    }
}
