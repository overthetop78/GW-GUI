namespace GWGUI.Emulation.Constants;

public static class EmulationHostApi
{
    public const int ManifestSchemaVersion = 2;
    public const string ManifestFileName = "module.json";
    public static Version CurrentVersion { get; } = new(1, 3);
    /// <summary>Première version de module acceptée par cette génération de l’application.</summary>
    public static Version MinimumModuleVersion { get; } = new(1, 3, 0);
}
