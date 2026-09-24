namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Adresse d'un secteur préparé pour l'injection dans une image cible.</summary>
public sealed record MediaSectorAddress(int Cylinder, int Head, int Number);
