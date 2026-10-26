using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using System.IO;
using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Descriptions et persistance des lecteurs natifs par les contrats de stockage déjà utilisés.</summary>
internal static partial class StorageFunctions
{
    internal static bool Connected(MachineConfiguration configuration, int drive) => configuration.Options.TryGetValue(C.Key(drive, C.ConnectedOption), out var value) ? bool.Parse(value) : configuration.Media.Any(media => media.Slot == new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, drive));
    internal static bool HasInterface(MachineConfiguration configuration) => Enumerable.Range(0, C.DriveCount).Any(drive => Connected(configuration, drive));
    internal static bool UsesWd1770(MachineConfiguration configuration) => configuration.Options.GetValueOrDefault(C.ControllerOption, C.Intel8271) == C.AcornWd1770;
    internal static IReadOnlyList<string> Extensions(MachineConfiguration configuration) => UsesWd1770(configuration) ? C.DoubleDensityExtensions : C.Extensions;
    private static IReadOnlyList<string> DriveExtensions(MachineConfiguration configuration, int drive) => CreationModels(configuration).Single(model => model.Value == Settings(configuration, drive).Model).AcceptedExtensions;
    internal static FloppyDriveSettings Settings(MachineConfiguration configuration, int drive) => new(configuration.Options.GetValueOrDefault(C.Key(drive, C.ModelOption), C.DefaultModel), configuration.Options.GetValueOrDefault(C.Key(drive, C.SpeedOption), C.DefaultSpeed), bool.Parse(configuration.Options.GetValueOrDefault(C.Key(drive, C.ProtectedOption), bool.FalseString)), false, new Dictionary<string, string> { [C.ControllerOption] = configuration.Options.GetValueOrDefault(C.ControllerOption, C.Intel8271), [C.MechanicalSoundOption] = configuration.Options.GetValueOrDefault(C.Key(drive, C.MechanicalSoundOption), bool.TrueString), [C.SoundProfileOption] = configuration.Options.GetValueOrDefault(C.Key(drive, C.SoundProfileOption), S.Floppy525), [C.SoundVolumeOption] = EmulationMechanicalSoundSettings.ReadVolume(configuration.Options, C.Key(drive, C.SoundVolumeOption)).ToString(System.Globalization.CultureInfo.InvariantCulture) });

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineConfiguration configuration) => Enumerable.Range(0, C.DriveCount).Select(drive => new EmulationMediaDevice(new(EmulationMediaCategory.FloppyDrive, drive), EmulationMediaType.Floppy, DriveExtensions(configuration, drive),
        FloppyOptions: new(CreationModels(configuration), configuration.Folders.Floppies ?? string.Empty, string.Empty, DriveExtensions(configuration, drive).First(), CanCreateBlankMedia: true, CanRedirectWrites: false), ConfigurationKind: EmulationStorageConfigurationKind.FloppyDrive, ConfigurationFields: [ControllerField(configuration), new(C.MechanicalSoundOption, EmulationMachineTab.Storage, C.FloppyPrefix, C.SoundResource, EmulationSettingsEditor.Toggle, configuration.Options.GetValueOrDefault(C.Key(drive, C.MechanicalSoundOption), bool.TrueString), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString), SoundProfileField(configuration, drive, false), SoundVolumeField(configuration, drive, false)])).ToArray();

    private static EmulationSettingsField SoundVolumeField(MachineConfiguration configuration, int index, bool hardDisk) =>
        EmulationMechanicalSoundSettings.DescribeVolume(C.SoundVolumeOption, hardDisk ? S.HardDisk : S.Floppy,
            EmulationMechanicalSoundSettings.ReadVolume(configuration.Options, hardDisk ? C.HardDiskKey(index, C.SoundVolumeOption) : C.Key(index, C.SoundVolumeOption)));

    private static EmulationSettingsField SoundProfileField(MachineConfiguration configuration, int index, bool hardDisk)
    {
        var device = hardDisk ? S.HardDisk : S.Floppy;
        var key = hardDisk ? C.HardDiskKey(index, C.SoundProfileOption) : C.Key(index, C.SoundProfileOption);
        var profiles = CommonHost.MediaProcessing.MechanicalSoundProfiles(device);
        return new(C.SoundProfileOption, EmulationMachineTab.Storage, device, C.SoundProfileResource, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(key, profiles[0]), profiles.Select(profile => new EmulationSettingsChoice(profile, string.Empty, CommonHost.MediaProcessing.MechanicalSoundProfileDisplayName(profile))).ToArray(), RequiresRestart: true);
    }

    /// <summary>Le dialogue du lecteur règle la carte de contrôle commune aux deux lecteurs BBC.</summary>
    private static EmulationSettingsField ControllerField(MachineConfiguration configuration) => new(C.ControllerOption, EmulationMachineTab.Storage, C.ControllerOption, C.InterfaceResource, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(C.ControllerOption, C.Intel8271), [new(C.Intel8271, string.Empty, C.Intel8271Display), new(C.AcornWd1770, string.Empty, C.Wd1770Display)], RequiresRestart: true, RefreshSettingsOnChange: true);

    private static IReadOnlyList<FloppyDriveModelChoice> CreationModels(MachineConfiguration configuration) => CommonHost.MediaProcessing.FloppyDriveModels(C.FloppyFamily, UsesWd1770(configuration));
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
        var controllerChanges = settings.Where(entry => entry.Floppy?.Options is not null).SelectMany(entry => entry.Floppy!.Options!).Where(pair => pair.Key == C.ControllerOption && pair.Value != configuration.Options.GetValueOrDefault(C.ControllerOption, C.Intel8271)).Select(pair => pair.Value).Distinct(StringComparer.Ordinal).ToArray();
        if (controllerChanges.Length > 1) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        if (controllerChanges.Length == 1) options[C.ControllerOption] = controllerChanges[0];
        foreach (var entry in settings)
        {
            if (entry.Floppy is not { } floppy) continue;
            if (entry.Slot.Category != EmulationMediaCategory.FloppyDrive || entry.Slot.Index is < 0 or >= C.DriveCount || !C.Models.Any(model => model.Value == floppy.Model) || !C.Speeds.Contains(floppy.Speed) || floppy.Options?.Any(pair => pair.Key switch { C.ControllerOption => pair.Value is not (C.Intel8271 or C.AcornWd1770), C.MechanicalSoundOption => !bool.TryParse(pair.Value, out _), C.SoundProfileOption => !CommonHost.MediaProcessing.MechanicalSoundProfiles(S.Floppy).Contains(pair.Value), C.SoundVolumeOption => !int.TryParse(pair.Value, out var volume) || volume is < 0 or > S.MaximumVolumePercent, _ => true }) == true) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
            options[C.Key(entry.Slot.Index, C.ModelOption)] = floppy.Model; options[C.Key(entry.Slot.Index, C.SpeedOption)] = floppy.Speed;
            options[C.Key(entry.Slot.Index, C.ProtectedOption)] = floppy.WriteProtected.ToString(); options.Remove(C.Key(entry.Slot.Index, C.RedirectOption));
            if (floppy.Options?.TryGetValue(C.MechanicalSoundOption, out var sound) == true) options[C.Key(entry.Slot.Index, C.MechanicalSoundOption)] = sound;
            if (floppy.Options?.TryGetValue(C.SoundVolumeOption, out var volume) == true) options[C.Key(entry.Slot.Index, C.SoundVolumeOption)] = volume;
            if (floppy.Options?.TryGetValue(C.SoundProfileOption, out var profile) == true) options[C.Key(entry.Slot.Index, C.SoundProfileOption)] = profile;
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
            if (!CreationModels(configuration).Any(model => model.Value == settings.Model) || !C.Speeds.Contains(settings.Speed) || !bool.TryParse(settings.Options![C.MechanicalSoundOption], out _) || !CommonHost.MediaProcessing.MechanicalSoundProfiles(S.Floppy).Contains(settings.Options[C.SoundProfileOption])) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        }
    }

    internal static void ValidateMedia(IEmulationDiskImage image, FloppyDriveSettings settings)
    {
        if (!CommonHost.MediaProcessing.SupportsFloppyImage(settings.Model, image)) throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
    }
}
