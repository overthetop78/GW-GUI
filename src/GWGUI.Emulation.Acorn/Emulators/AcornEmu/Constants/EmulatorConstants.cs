namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Identité stable du cœur interne ; ses ROM sont toujours fournies séparément par l'utilisateur.</summary>
internal static class EmulatorConstants
{
    internal const string Id = "acornemu";
    internal const string Name = "AcornEmu";
    internal const string Version = "0.1.0";
    internal const string SystemFirmware = "acornemu.mos";
    internal const string BasicFirmware = "acornemu.basic";
    internal const string CharacterFirmware = "acornemu.characters";
    /// <summary>Choix de raccordement matériel, indépendant des filtres de présentation de l'application.</summary>
    internal const string VideoOutput = "acornemu.video-output";
    internal const string UhfOutput = "uhf";
    internal const string MonochromeOutput = "composite-mono";
    internal const string ColourCompositeOutput = "composite-colour";
    internal const string RgbOutput = "rgb";
    internal static IReadOnlyList<string> VideoOutputs { get; } = Array.AsReadOnly<string>([UhfOutput, MonochromeOutput, ColourCompositeOutput, RgbOutput]);
}
