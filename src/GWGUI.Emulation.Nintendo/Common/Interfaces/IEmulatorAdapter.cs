namespace GWGUI.Emulation.Nintendo.Common.Interfaces;
internal interface IEmulatorAdapter : global::GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    IReadOnlyList<string> SupportedContentExtensions { get; }
}
