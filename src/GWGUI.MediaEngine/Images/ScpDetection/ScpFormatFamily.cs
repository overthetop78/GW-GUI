namespace GWGUI.MediaEngine.Images.ScpDetection;

/// <summary>Identifie une famille technique de reconstruction sectorielle SCP.</summary>
internal enum ScpFormatFamily
{
    /// <summary>Secteurs compatibles avec les encodages ISO FM ou MFM.</summary>
    Iso,
    /// <summary>Secteurs Amiga MFM.</summary>
    Amiga,
    /// <summary>Secteurs Commodore GCR.</summary>
    Commodore,
    /// <summary>Secteurs Apple GCR.</summary>
    Apple,
    /// <summary>Secteurs DEC RX02.</summary>
    Dec
}
