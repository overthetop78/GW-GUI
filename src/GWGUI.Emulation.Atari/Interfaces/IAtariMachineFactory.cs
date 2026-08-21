using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal interface IAtariMachineFactory
{
    AtariEmulator Emulator { get; }
    IEmulatedMachine Create(AtariMachineConfiguration configuration, AtariMachineCreationContext context);
}
