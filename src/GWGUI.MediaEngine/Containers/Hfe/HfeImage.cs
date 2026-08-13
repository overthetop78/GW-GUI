namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Contient l'en-tête HFE validé et toutes ses faces de pistes.</summary>
public sealed record HfeImage(byte Revision, int Cylinders, int Heads, byte Encoding, ushort BitRate, IReadOnlyList<HfeTrack> Tracks);
