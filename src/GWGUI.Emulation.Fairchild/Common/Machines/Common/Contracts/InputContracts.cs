namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(IReadOnlyDictionary<string, string>? Keyboard = null, IReadOnlyList<ControllerBinding>? Controllers = null);
