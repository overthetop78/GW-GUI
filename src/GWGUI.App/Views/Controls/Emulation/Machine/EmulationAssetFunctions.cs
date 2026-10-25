using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;

namespace GWGUI.App.Views.Controls.Emulation.Machine;

internal static class EmulationAssetFunctions
{
    internal static ImageSource? Load(IEmulationModule module, string? resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName)) return null;
        using var stream = module.GetType().Assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
