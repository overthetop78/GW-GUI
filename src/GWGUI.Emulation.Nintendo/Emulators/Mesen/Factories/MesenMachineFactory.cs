using GWGUI.Emulation.Nintendo.Emulators.Mesen.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Mesen.Factories;

internal sealed class MesenMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.Nes or ModelConstants.FamicomDisk;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
