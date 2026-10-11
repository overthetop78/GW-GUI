namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(IReadOnlyDictionary<string, string>? Keyboard = null, IReadOnlyList<ControllerBinding>? Controllers = null, IReadOnlyDictionary<string, string>? Mouse = null, int MouseSpeedPercent = InputMappingConstants.DefaultMouseSpeedPercent);
