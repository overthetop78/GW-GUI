namespace GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;

internal sealed record ControllerFeedbackState(int Port, string CommandId, float Strength);
