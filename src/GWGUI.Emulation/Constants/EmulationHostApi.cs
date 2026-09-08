namespace GWGUI.Emulation.Constants;

public static class EmulationHostApi
{
    public const int ManifestSchemaVersion = 1;
    public const string ManifestFileName = "module.json";
    public static Version CurrentVersion { get; } = new(1, 0);
}
