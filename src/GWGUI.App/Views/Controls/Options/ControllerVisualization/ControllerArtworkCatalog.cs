using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static readonly IReadOnlyDictionary<ControllerVisualModel, string> ModelResources =
        new Dictionary<ControllerVisualModel, string>
        {
            [ControllerVisualModel.GenericGamepad] = ControllerArtworkFileNames.GenericGamepad,
            [ControllerVisualModel.XboxSeries] = ControllerArtworkFileNames.XboxSeries,
            [ControllerVisualModel.XboxOne] = ControllerArtworkFileNames.XboxOne,
            [ControllerVisualModel.Xbox360] = ControllerArtworkFileNames.Xbox360Black,
            [ControllerVisualModel.Xbox360White] = ControllerArtworkFileNames.Xbox360White,
            [ControllerVisualModel.XboxRematchCore] = ControllerArtworkFileNames.XboxRematchCore,
            [ControllerVisualModel.PlayStation4] = ControllerArtworkFileNames.Playstation4,
            [ControllerVisualModel.PlayStation5] = ControllerArtworkFileNames.Playstation5,
            [ControllerVisualModel.MasterSystem] = ControllerArtworkFileNames.MasterSystem,
            [ControllerVisualModel.NintendoEntertainmentSystem] = ControllerArtworkFileNames.NintendoEntertainmentSystem,
            [ControllerVisualModel.Nintendo64] = ControllerArtworkFileNames.Nintendo64,
            [ControllerVisualModel.SuperNintendo] = ControllerArtworkFileNames.SuperNintendo,
            [ControllerVisualModel.MegaDrive3] = ControllerArtworkFileNames.MegaDrive3,
            [ControllerVisualModel.MegaDrive6] = ControllerArtworkFileNames.MegaDrive6,
            [ControllerVisualModel.PlayStation1] = ControllerArtworkFileNames.Playstation1,
            [ControllerVisualModel.PlayStation2] = ControllerArtworkFileNames.Playstation2,
            [ControllerVisualModel.Saturn] = ControllerArtworkFileNames.Saturn,
            [ControllerVisualModel.Dreamcast] = ControllerArtworkFileNames.Dreamcast,
            [ControllerVisualModel.RacingWheel] = ControllerArtworkFileNames.RacingWheel,
            [ControllerVisualModel.FlightStick] = ControllerArtworkFileNames.FlightStick
        };


    private static readonly Dictionary<ControllerVisualModel, ImageSource> ModelCache = [];
    private static readonly Dictionary<string, ControllerArtworkProfile> ProfileCache =
        new(StringComparer.Ordinal);

    internal static bool TryGet(ControllerVisualModel model, out ImageSource artwork)
    {
        if (ModelCache.TryGetValue(model, out artwork!)) return true;
        if (!ModelResources.TryGetValue(model, out var fileName))
        {
            artwork = null!;
            return false;
        }

        artwork = Load(fileName);
        ModelCache[model] = artwork;
        return true;
    }

    internal static bool TryGetProfile(string visualId, out ControllerArtworkProfile profile)
    {
        if (ProfileCache.TryGetValue(visualId, out profile!)) return true;
        if (!ProfileDefinitions.TryGetValue(visualId, out var definition))
        {
            profile = null!;
            return false;
        }

        profile = new ControllerArtworkProfile(visualId, Load(definition.FileName), definition.Zones);
        ProfileCache[visualId] = profile;
        return true;
    }

    internal static IReadOnlyList<ControllerArtworkProfile> AvailableProfiles(
        IReadOnlyList<string>? compatibleVisualIds)
    {
        if (compatibleVisualIds is null || compatibleVisualIds.Count == 0) return [];

        var profiles = new List<ControllerArtworkProfile>(compatibleVisualIds.Count);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (var visualId in compatibleVisualIds)
            if (visited.Add(visualId) && TryGetProfile(visualId, out var profile))
                profiles.Add(profile);
        return profiles;
    }

    private static ImageSource Load(string fileName)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(
            $"pack://application:,,,/gwgui.app;component/Assets/Controllers/{fileName}",
            UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private sealed record ProfileDefinition(
        string FileName,
        IReadOnlyList<ControllerVisualZone> Zones);
}
