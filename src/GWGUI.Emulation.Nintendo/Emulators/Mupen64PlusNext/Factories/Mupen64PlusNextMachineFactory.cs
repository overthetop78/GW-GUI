using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Factories;

internal sealed class Mupen64PlusNextMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.Nintendo64;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
