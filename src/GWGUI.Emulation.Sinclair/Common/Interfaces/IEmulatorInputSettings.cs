namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

internal interface IEmulatorInputSettings
{
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
}
