using System.Collections.ObjectModel;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.FloppyDriveConstants;
using S = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;

/// <summary>Lecteurs BBC et formats autorisés par le matériel ; les fichiers restent créés par MediaEngine.</summary>
internal static class FloppyDriveModels
{
    private sealed record Profile(string Id, int Tracks, int Sides, bool DoubleDensity);
    private static readonly IReadOnlyList<Profile> Profiles = Array.AsReadOnly<Profile>(
    [
        new(S.Double80, C.EightyTracks, 2, false), new(S.Single80, C.EightyTracks, 1, false),
        new(S.Double40, C.FortyTracks, 2, false), new(S.Single40, C.FortyTracks, 1, false),
        new(S.Double80Dd, C.EightyTracks, 2, true), new(S.Single80Dd, C.EightyTracks, 1, true),
        new(S.Double40Dd, C.FortyTracks, 2, true), new(S.Single40Dd, C.FortyTracks, 1, true)
    ]);

    private static readonly IReadOnlyList<FloppyCreationFormatChoice> Formats = Array.AsReadOnly<FloppyCreationFormatChoice>(
    [
        Format(C.DfsSingle40, "Ssd", C.DfsName, C.FortyTracks, 1, false),
        Format(C.DfsSingle80, "Ssd", C.DfsName, C.EightyTracks, 1, false),
        Format(C.DfsDouble40, "Dsd", C.DfsName, C.FortyTracks, 2, false),
        Format(C.DfsDouble80, "Dsd", C.DfsName, C.EightyTracks, 2, false),
        Format(C.Adfs160, "Ads", C.AdfsName, C.FortyTracks, 1, true),
        Format(C.Adfs320, "Adm", C.AdfsName, C.EightyTracks, 1, true),
        Format(C.Adfs640, "Adl", C.AdfsName, C.EightyTracks, 2, true)
    ]);

    internal static IReadOnlyList<FloppyDriveModelChoice> All { get; } = Array.AsReadOnly(Profiles.Select(Describe).ToArray());
    internal static IReadOnlyList<FloppyDriveModelChoice> Available(bool doubleDensity) =>
        Array.AsReadOnly(All.Where(model => doubleDensity || !model.DoubleDensity).ToArray());

    private static FloppyCreationFormatChoice Format(string id, string extension, string name, int tracks, int sides, bool doubleDensity) =>
        new(id, CommonHost.MediaProcessing.FileExtensions[extension], name, Capacity(tracks, sides, doubleDensity))
        { Tracks = tracks, Sides = sides, DoubleDensity = doubleDensity };

    private static long Capacity(int tracks, int sides, bool doubleDensity) =>
        (long)tracks * sides * C.SectorSize * (doubleDensity ? C.DoubleDensitySectors : C.SingleDensitySectors);

    private static FloppyDriveModelChoice Describe(Profile profile)
    {
        var formats = Formats.Where(format => format.Tracks <= profile.Tracks && format.Sides <= profile.Sides && (!format.DoubleDensity || profile.DoubleDensity)).ToArray();
        var extensions = formats.Select(format => format.Extension).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (profile.DoubleDensity) extensions.Add(CommonHost.MediaProcessing.FileExtensions["Adf"]);
        return new(profile.Id, string.Empty, BlankImageSize: Capacity(profile.Tracks, profile.Sides, profile.DoubleDensity))
        {
            DiameterInches = C.DiameterInches, Tracks = profile.Tracks, Sides = profile.Sides, DoubleDensity = profile.DoubleDensity,
            CreationFormats = Array.AsReadOnly(formats), AcceptedExtensions = extensions.AsReadOnly(),
            BlankFormats = new ReadOnlyDictionary<string, string>(formats.GroupBy(format => format.Extension).ToDictionary(group => group.Key, group => group.Last().FormatId))
        };
    }

    internal static bool Supports(string modelId, IEmulationDiskImage image)
    {
        var profile = Profiles.SingleOrDefault(profile => profile.Id == modelId);
        return profile is not null && image.SectorSize == C.SectorSize
            && (image.SectorsPerTrack == C.SingleDensitySectors || profile.DoubleDensity && image.SectorsPerTrack == C.DoubleDensitySectors)
            && image.Cylinders > 0 && image.Cylinders <= profile.Tracks && image.Heads > 0 && image.Heads <= profile.Sides
            && image.SectorCount == image.Cylinders * image.Heads * image.SectorsPerTrack;
    }
}
