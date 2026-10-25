namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(IReadOnlyList<ControllerBinding>? Controllers = null);
