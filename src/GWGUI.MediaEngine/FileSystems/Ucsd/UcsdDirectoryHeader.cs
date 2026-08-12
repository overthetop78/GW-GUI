namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Contient les champs validés de l'en-tête de répertoire UCSD.</summary>
/// <param name="EndDirectory">Borne de fin exclusive du répertoire.</param>
/// <param name="VolumeName">Nom du volume.</param>
/// <param name="TotalBlocks">Nombre total de blocs annoncé.</param>
/// <param name="DeclaredFiles">Nombre de fichiers annoncé.</param>
/// <param name="VolumeDate">Date du volume, si elle est valide.</param>
/// <param name="ByteOrder">Ordre des octets détecté.</param>
/// <param name="DirectoryBlockCount">Nombre de blocs occupés par le répertoire.</param>
internal sealed record UcsdDirectoryHeader(int EndDirectory, string VolumeName, int TotalBlocks, int DeclaredFiles, DateTimeOffset? VolumeDate, UcsdByteOrder ByteOrder, int DirectoryBlockCount);
