namespace GWGUI.MediaEngine.Contracts;

/// <summary>Secteur lu dans un conteneur, avec adresse physique et état d'intégrité conservés.</summary>
public sealed record MediaDiskSector(long Index, ReadOnlyMemory<byte> Data, int Cylinder, int Head, int Number, bool? IntegrityValid = null, byte? DiagnosticCode = null);
