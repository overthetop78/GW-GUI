namespace GWGUI.MediaEngine.Constants;

public static class ImageFormatFallbackExtensions
{
    public const string Scp = ".scp";
    public const string Img = ".img";
    public const string Ima = ".ima";
    public const string Hfe = ".hfe";

    public static IReadOnlySet<string> All { get; } =
        new HashSet<string>([Scp, Img, Ima, Hfe], StringComparer.OrdinalIgnoreCase);
}
