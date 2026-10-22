namespace GWGUI.Emulation.Sega.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorSettings
{
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
    IReadOnlyList<string> SupportedContentExtensions { get; }
}
