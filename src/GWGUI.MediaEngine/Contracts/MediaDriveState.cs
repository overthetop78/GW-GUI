namespace GWGUI.MediaEngine.Contracts;

/// <summary>État mécanique d’un lecteur : position, moteur et activité sur le support.</summary>
public readonly record struct MediaDriveState(
    bool MotorRunning,
    bool MediaInserted,
    bool Reading,
    bool Writing,
    MediaDrivePosition Position);
