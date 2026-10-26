namespace GWGUI.Emulation.Contracts;

/// <summary>Secteur fourni à un contrôleur émulé ; les données et diagnostics restent distincts.</summary>
public sealed record EmulationDiskSector(long Index, ReadOnlyMemory<byte> Data, int Cylinder, int Head, int Number, bool? IntegrityValid = null, byte? DiagnosticCode = null);
