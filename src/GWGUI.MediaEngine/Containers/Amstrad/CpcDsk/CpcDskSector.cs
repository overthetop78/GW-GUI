namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>Conserve un descripteur CPCEMU et les données stockées qui lui correspondent.</summary>
public sealed record CpcDskSector(byte Cylinder, byte Head, byte Id, byte SizeCode, byte Status1, byte Status2, IReadOnlyList<byte> Data);
