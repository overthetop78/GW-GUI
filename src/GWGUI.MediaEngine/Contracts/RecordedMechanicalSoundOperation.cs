namespace GWGUI.MediaEngine.Contracts;

/// <summary>Opération attestée par un échantillon ; la distance et le sens décrivent un déplacement réel.</summary>
internal sealed record RecordedMechanicalSoundOperation(string Operation, int Distance = 0, int Direction = 0, int Cylinder = -1, bool Loop = false);
