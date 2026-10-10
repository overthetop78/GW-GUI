using GWGUI.Emulation.Mattel.Emulators.FreeIntv.Constants;

namespace GWGUI.Emulation.Mattel.Emulators.FreeIntv.Factories;

internal sealed class FreeIntvMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;
}
