namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>Conserve l'en-tête, les descripteurs et les données d'une piste CPCEMU.</summary>
public sealed record CpcDskTrack(int Index, bool IsPresent, byte Cylinder, byte Head, byte SectorSizeCode, byte Gap3Length, byte FillerByte, IReadOnlyList<CpcDskSector> Sectors);
