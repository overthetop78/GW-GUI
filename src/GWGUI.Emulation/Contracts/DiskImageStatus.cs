namespace GWGUI.Emulation.Contracts;

/// <summary>Image d’un jeu de disques : index du cœur, chemin éventuel et libellé éventuel.</summary>
public sealed record DiskImageStatus(int Index, string? Path, string? Label);
