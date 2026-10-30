using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Views.Controls.Emulation.Machine;

internal static class EmulationAssetFunctions
{
    internal static ImageSource? Load(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(path)) return null;
        using var stream = File.OpenRead(path);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
