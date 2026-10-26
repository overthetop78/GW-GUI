using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Contracts;
using C = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Dictionaries;

/// <summary>Profils de mécanismes ; ces valeurs décrivent une synthèse, pas un enregistrement d'un modèle commercial précis.</summary>
internal static class MechanicalSoundProfiles
{
    private static readonly IReadOnlyDictionary<string, MechanicalSoundProfile> All = new System.Collections.ObjectModel.ReadOnlyDictionary<string, MechanicalSoundProfile>(new Dictionary<string, MechanicalSoundProfile>
    {
        [C.Standard] = new(C.Cassette, 55, 300, 3, 60, 90, 90, 90, 1, 0.02),
        [C.Floppy525] = new(C.Floppy, 90, 180, 2, 45, 35, 75, 120, 1, 0.025),
        [C.Floppy35] = new(C.Floppy, 120, 180, 2, 45, 35, 75, 120, 0.65, 0.025),
        [C.Winchester] = new(C.HardDisk, 60, 170, 9, 60, 20, 70, 100, 1, 0.1),
        [C.HardDisk35] = new(C.HardDisk, 120, 170, 9, 60, 20, 70, 100, 0.6, 0.1),
        [C.HardDisk25] = new(C.HardDisk, 90, 170, 9, 60, 20, 70, 100, 0.35, 0.1),
        [C.OpticalTray] = new(C.Optical, 100, 110, 5, 80, 25, 90, 110, 0.8, 0.15),
        [C.OpticalSlot] = new(C.Optical, 120, 110, 5, 80, 25, 90, 110, 0.65, 0.15)
    });
    internal static IReadOnlyList<string> ForDevice(string device)
    {
        var fallback = RecordedMechanicalSoundFunctions.BaseProfile(device);
        var profiles = RecordedMechanicalSoundFunctions.Shared.Profiles.Where(profile => profile.Device == device && profile.Samples.Any(sample => sample.Operations.Any(operation => operation.Operation == C.Motor))).OrderBy(profile => profile.Id != fallback).Select(profile => profile.Id)
            .Concat(All.Where(entry => entry.Value.Device == device).Select(entry => entry.Key)).Distinct(StringComparer.Ordinal).ToArray();
        return profiles.Length > 0 ? Array.AsReadOnly(profiles) : throw new ArgumentOutOfRangeException(nameof(device));
    }
    internal static MechanicalSoundProfile Get(string id) => All.TryGetValue(id, out var profile) ? profile : throw new ArgumentOutOfRangeException(nameof(id));
    internal static string DisplayName(string id) => RecordedMechanicalSoundFunctions.Shared.Contains(id) ? RecordedMechanicalSoundFunctions.Shared.Profile(id).DisplayName : id switch { C.Floppy525 => C.Label525, C.Floppy35 or C.HardDisk35 => C.Label35, C.HardDisk25 => C.Label25, _ => id };
}
