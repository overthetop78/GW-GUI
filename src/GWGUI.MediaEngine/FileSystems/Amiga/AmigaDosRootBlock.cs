namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Contient la variante, le numéro et les données validées du bloc racine AmigaDOS.</summary>
public sealed record AmigaDosRootBlock(AmigaDosVariant Variant, int BlockNumber, byte[] Data, int HashTableSize);
