namespace GWGUI.MediaEngine.Contracts;

/// <summary>Événement mécanique accompagné du déplacement physique qui sélectionne son enregistrement.</summary>
internal readonly record struct MediaDriveSoundEvent(string Operation, int Distance = 1, int Direction = 1, int Cylinder = -1);
