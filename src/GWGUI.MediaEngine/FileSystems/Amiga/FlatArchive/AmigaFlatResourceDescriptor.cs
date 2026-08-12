namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Décrit une ressource nommée et sa longueur dans le flux concaténé.</summary>
internal sealed record AmigaFlatResourceDescriptor(string Name, uint Length);
