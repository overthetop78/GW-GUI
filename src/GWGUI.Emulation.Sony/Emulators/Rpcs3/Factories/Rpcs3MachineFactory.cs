using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Rpcs3.Factories;

internal sealed class Rpcs3MachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
