namespace GWGUI.MediaEngine.Contracts;

/// <summary>Position physique des têtes d’un lecteur, indépendante du conteneur de l’image.</summary>
/// <param name="Cylinder">Cylindre demandé, compté à partir de zéro.</param>
/// <param name="Head">Tête ou face sélectionnée, comptée à partir de zéro.</param>
/// <param name="Sector">Numéro physique du secteur, ou -1 en dehors d’un accès sectoriel.</param>
/// <param name="LogicalBlock">Adresse logique lorsqu’aucune géométrie physique n’est disponible.</param>
public readonly record struct MediaDrivePosition(int Cylinder, int Head, int Sector = -1, long LogicalBlock = -1);
