using GWGUI.App.Services.Input.GameInput;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static class ControllerArtworkCatalog
{
    private static readonly IReadOnlyDictionary<ControllerVisualModel, string> Resources =
        new Dictionary<ControllerVisualModel, string>
        {
            [ControllerVisualModel.GenericGamepad] = "generic-gamepad.png",
            [ControllerVisualModel.XboxSeries] = "xbox-series.png",
            [ControllerVisualModel.XboxOne] = "xbox-one.png",
            [ControllerVisualModel.XboxRematchCore] = "xbox-rematch-core.png",
            [ControllerVisualModel.PlayStation4] = "playstation-4.png",
            [ControllerVisualModel.PlayStation5] = "playstation-5.png",
            [ControllerVisualModel.MasterSystem] = "master-system.png",
            [ControllerVisualModel.NintendoEntertainmentSystem] = "nintendo-entertainment-system.png",
            [ControllerVisualModel.Nintendo64] = "nintendo-64.png",
            [ControllerVisualModel.SuperNintendo] = "super-nintendo.png",
            [ControllerVisualModel.MegaDrive3] = "mega-drive-3.png",
            [ControllerVisualModel.MegaDrive6] = "mega-drive-6.png",
            [ControllerVisualModel.PlayStation1] = "playstation-1.png",
            [ControllerVisualModel.PlayStation2] = "playstation-2.png",
            [ControllerVisualModel.Saturn] = "saturn.png",
            [ControllerVisualModel.Dreamcast] = "dreamcast.png",
            [ControllerVisualModel.RacingWheel] = "racing-wheel.png",
            [ControllerVisualModel.FlightStick] = "flight-stick.png",
            [ControllerVisualModel.ArcadeStick] = "arcade-stick.png"
        };
    private static readonly Dictionary<ControllerVisualModel, ImageSource> Cache = [];

    internal static bool TryGet(ControllerVisualModel model, out ImageSource artwork)
    {
        if (Cache.TryGetValue(model, out artwork!)) return true;
        if (!Resources.TryGetValue(model, out var fileName))
        {
            artwork = null!;
            return false;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri($"pack://application:,,,/gwgui.app;component/Assets/Controllers/{fileName}", UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        Cache[model] = image;
        artwork = image;
        return true;
    }
}
