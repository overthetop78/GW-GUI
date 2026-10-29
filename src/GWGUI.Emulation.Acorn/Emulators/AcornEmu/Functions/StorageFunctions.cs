using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using System.IO;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Descriptions et persistance des lecteurs natifs par les contrats de stockage déjà utilisés.</summary>
internal static partial class StorageFunctions
{
    internal static bool Connected(MachineConfiguration configuration, int drive) => configuration.Options.TryGetValue(C.Key(drive, C.ConnectedOption), out var value) ? bool.Parse(value) : configuration.Media.Any(media => media.Slot == new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, drive));
    internal static bool HasInterface(MachineConfiguration configuration) => FloppyExtensionFunctions.HasInterface(configuration) || Enumerable.Range(0, C.DriveCount).Any(drive => Connected(configuration, drive));
    internal static bool UsesWd1770(MachineConfiguration configuration) => FloppyExtensionFunctions.UsesWd1770(configuration);
    internal static IReadOnlyList<string> Extensions(MachineConfiguration configuration) => CreationModels(configuration).SelectMany(model => model.AcceptedExtensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    private static IReadOnlyList<string> DriveExtensions(MachineConfiguration configuration, int drive) => CreationModels(configuration).Single(model => model.Value == Settings(configuration, drive).Model).AcceptedExtensions;
    internal static FloppyDriveSettings Settings(MachineConfiguration configuration, int drive) => new(Model(configuration, drive), configuration.Options.GetValueOrDefault(C.Key(drive, C.SpeedOption), C.DefaultSpeed), bool.Parse(configuration.Options.GetValueOrDefault(C.Key(drive, C.ProtectedOption), bool.FalseString)), false, new Dictionary<string, string> { [C.MechanicalSoundOption] = configuration.Options.GetValueOrDefault(C.Key(drive, C.MechanicalSoundOption), bool.TrueString), [C.SoundVolumeOption] = EmulationMechanicalSoundSettings.ReadVolume(configuration.Options, C.Key(drive, C.SoundVolumeOption)).ToString(System.Globalization.CultureInfo.InvariantCulture) });

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineConfiguration configuration) => Enumerable.Range(0, C.DriveCount).Select(drive => new EmulationMediaDevice(new(EmulationMediaCategory.FloppyDrive, drive), EmulationMediaType.Floppy, DriveExtensions(configuration, drive),
        FloppyOptions: new(CreationModels(configuration), configuration.Folders.Floppies ?? string.Empty, string.Empty, DriveExtensions(configuration, drive).First(), CanCreateBlankMedia: true, CanRedirectWrites: false) { SpeedChoices = EmulationDriveSpeedChoices.Standard, CanConfigureWriteProtection = false }, ConfigurationKind: EmulationStorageConfigurationKind.FloppyDrive, ConfigurationFields: [new(C.MechanicalSoundOption, EmulationMachineTab.Storage, StoragePresentationConstants.SoundGroup, C.SoundResource, EmulationSettingsEditor.Toggle, configuration.Options.GetValueOrDefault(C.Key(drive, C.MechanicalSoundOption), bool.TrueString), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString), SoundVolumeField(configuration, drive, false)])).ToArray();

    private static EmulationSettingsField SoundVolumeField(MachineConfiguration configuration, int index, bool hardDisk) =>
        EmulationMechanicalSoundSettings.DescribeVolume(C.SoundVolumeOption, StoragePresentationConstants.SoundGroup,
            EmulationMechanicalSoundSettings.ReadVolume(configuration.Options, hardDisk ? C.HardDiskKey(index, C.SoundVolumeOption) : C.Key(index, C.SoundVolumeOption))) with { NumericRange = new(0, S.MaximumVolumePercent, 1, C.PercentSuffix, IsVertical: true) };

    /// <summary>Identifie les réglages d’écoute sans inclure les paramètres matériels du lecteur.</summary>
    internal static bool IsMechanicalSoundOption(string key) => TryGetMechanicalSoundOption(key, out _, out _, out _);

    /// <summary>Résout le lecteur et la clé locale de son réglage sonore persistant.</summary>
    internal static bool TryGetMechanicalSoundOption(string key, out bool hardDisk, out int index, out string option)
    {
        foreach (var device in new[] { false, true })
        for (var drive = 0; drive < (device ? C.HardDiskCount : C.DriveCount); drive++)
        foreach (var name in new[] { C.MechanicalSoundOption, C.SoundVolumeOption })
            if (key == (device ? C.HardDiskKey(drive, name) : C.Key(drive, name)))
            {
                hardDisk = device; index = drive; option = name; return true;
            }
        hardDisk = false; index = default; option = string.Empty; return false;
    }
    private static IReadOnlyList<FloppyDriveModelChoice> CreationModels(MachineConfiguration configuration) => FloppyDriveModels.Available(UsesWd1770(configuration));
    private static string Model(MachineConfiguration configuration, int drive)
    {
        var requested = configuration.Options.GetValueOrDefault(C.Key(drive, C.ModelOption), C.DefaultModel);
        var profile = C.Models.SingleOrDefault(model => model.Value == requested) ?? throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var available = CreationModels(configuration);
        return available.Any(model => model.Value == requested) ? requested
            : available.Single(model => model.Tracks == profile.Tracks && model.Sides == profile.Sides && !model.DoubleDensity).Value;
    }
    internal static IReadOnlyList<EmulationStorageDeviceSettings> DescribeSettings(MachineConfiguration configuration) => Enumerable.Range(0, C.DriveCount).Where(drive => Connected(configuration, drive)).Select(drive => new EmulationStorageDeviceSettings(new(EmulationMediaCategory.FloppyDrive, drive), Settings(configuration, drive))).ToArray();

    internal static MachineConfiguration SelectSlots(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots)
    {
        if (slots.Distinct().Count() != slots.Count) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(slots));
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        for (var drive = 0; drive < C.DriveCount; drive++) options[C.Key(drive, C.ConnectedOption)] = slots.Contains(new(EmulationMediaCategory.FloppyDrive, drive)).ToString();
        return SelectHardDiskSlots(configuration with { Options = options }, slots);
    }

    internal static MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        foreach (var entry in settings)
        {
            if (entry.Floppy is not { } floppy) continue;
            if (entry.Slot.Category != EmulationMediaCategory.FloppyDrive || entry.Slot.Index is < 0 or >= C.DriveCount || !CreationModels(configuration).Any(model => model.Value == floppy.Model) || !C.Speeds.Contains(floppy.Speed) || floppy.Options?.Any(pair => pair.Key switch { C.MechanicalSoundOption => !bool.TryParse(pair.Value, out _), C.SoundVolumeOption => !int.TryParse(pair.Value, out var volume) || volume is < 0 or > S.MaximumVolumePercent, _ => true }) == true) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
            options[C.Key(entry.Slot.Index, C.ModelOption)] = floppy.Model; options[C.Key(entry.Slot.Index, C.SpeedOption)] = floppy.Speed;
            options.Remove(C.Key(entry.Slot.Index, C.ProtectedOption));
            options.Remove(C.Key(entry.Slot.Index, C.RedirectOption));
            if (floppy.Options?.TryGetValue(C.MechanicalSoundOption, out var sound) == true) options[C.Key(entry.Slot.Index, C.MechanicalSoundOption)] = sound;
            if (floppy.Options?.TryGetValue(C.SoundVolumeOption, out var volume) == true) options[C.Key(entry.Slot.Index, C.SoundVolumeOption)] = volume;
            options.Remove(C.Key(entry.Slot.Index, C.SoundProfileOption));
        }
        return ApplyHardDiskSettings(configuration with { Options = options }, settings);
    }

    /// <summary>Convertit l'ancienne politique du lecteur en protection du seul support déjà présent.</summary>
    internal static EmulationStorageSettings MigrateMediaProtection(MachineConfiguration configuration, EmulationStorageSettings settings) => settings with
    {
        MountedMedia = settings.MountedMedia.Select(media =>
        {
            if (media.Slot.Category != EmulationMediaCategory.FloppyDrive) return media;
            var requested = settings.DeviceSettings?.FirstOrDefault(entry => entry.Slot == media.Slot)?.Floppy?.WriteProtected == true;
            var previouslyMounted = configuration.Media.Any(previous => previous.Slot == media.Slot && string.Equals(previous.Path, media.Path, StringComparison.OrdinalIgnoreCase));
            var legacy = previouslyMounted && Settings(configuration, media.Slot.Index).WriteProtected;
            return requested || legacy ? media with { IsReadOnly = true } : media;
        }).ToArray()
    };

    internal static void Validate(MachineConfiguration configuration)
    {
        ValidateHardDisks(configuration);
        _ = FloppyExtensionFunctions.HasSolidisk(configuration);
        if (configuration.Options.GetValueOrDefault(C.ControllerOption, C.Intel8271) is not (C.Intel8271 or C.AcornWd1770)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        for (var drive = 0; drive < C.DriveCount; drive++)
        {
            _ = Connected(configuration, drive);
            var settings = Settings(configuration, drive);
            if (!CreationModels(configuration).Any(model => model.Value == settings.Model) || !C.Speeds.Contains(settings.Speed) || !bool.TryParse(settings.Options![C.MechanicalSoundOption], out _)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        }
    }

    internal static void ValidateMedia(IEmulationDiskImage image, FloppyDriveSettings settings)
    {
        if (!FloppyDriveModels.Supports(settings.Model, image)) throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
    }
}
