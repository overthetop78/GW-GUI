namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Construit les avertissements propres aux catalogues ADFS.</summary>
public static class AcornAdfsWarnings
{
    /// <summary>Signale une profondeur excessive.</summary>
    public static string DepthLimit(int depth) => $"The ADFS directory nesting limit was reached at depth {depth}.";
    /// <summary>Signale un cycle ou une seconde référence.</summary>
    public static string CyclicDirectory(int address) => $"The ADFS directory at address {address} is cyclic or referenced more than once.";
    /// <summary>Signale une adresse ou une longueur invalide.</summary>
    public static string InvalidDataRange(string name, int address, long offset, long length) => $"{name}: ADFS address {address}, offset {offset}, length {length} is invalid.";
    /// <summary>Signale un bloc absent.</summary>
    public static string MissingBlock(string name, int address, long offset, int block) => $"{name}: ADFS address {address} at offset {offset} requires missing block {block}.";
}
