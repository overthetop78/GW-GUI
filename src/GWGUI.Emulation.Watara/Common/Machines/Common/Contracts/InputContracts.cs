namespace GWGUI.Emulation.Watara.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(IReadOnlyList<ControllerBinding>? Controllers = null);
