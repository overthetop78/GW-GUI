using GWGUI.Emulation.Acorn.Emulators.B2.Enums;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Contracts;

public sealed record B2ModelDefinition(B2Model Model, MachineModel MachineModel, string NativeValue);
