using GWGUI.Emulation.Nintendo.Emulators.Citra.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra.Factories;

internal sealed class CitraMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.Nintendo3Ds;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
