namespace GWGUI.MediaEngine.Contracts;

/// <summary>Paramètres fixes d'un profil sonore synthétique, indépendants du signal enregistré sur le média.</summary>
internal sealed record MechanicalSoundProfile(string Device, double Frequency, double MotorLevel, double Harmonic, double HarmonicLevel, double MotorNoise, double ReadNoise, double WriteNoise, double Gain, double StartSeconds);
