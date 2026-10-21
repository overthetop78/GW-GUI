using GWGUI.Emulation.Example.Common.Machines.ExampleMachineB.Constants;

namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineB.Dictionaries;

/// <summary>Déclaration matérielle du profil B, utilisée de la même façon par les trois émulateurs.</summary>
/// <remarks>La déclaration est descriptive : elle ne crée ni lecteur physique, ni cœur, ni fichier de contenu.</remarks>
internal static class ExampleMachineBModelCatalog
{
    /// <summary>Profil B et ses ports génériques, ses lecteurs et ses capacités de simulation.</summary>
    internal static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly<HardwareModelDefinition>(
    [
        new(MachineModel.ExampleMachineB, ModelConstants.Id, ModelConstants.DisplayNameResourceKey, Emulator.Internal,
            [HardwareCpu.ExampleCpu], ModelConstants.DefaultCpuFrequencyHz, ModelConstants.MainMemoryBytes,
            [HardwareRegion.Pal, HardwareRegion.Ntsc], [HardwareVideoCapability.Raster], [HardwareAudioCapability.Pcm],
            [HardwareStorageCapability.Cartridge, HardwareStorageCapability.MemoryCard],
            [new(HardwarePortCapability.Keyboard, 1), new(HardwarePortCapability.Mouse, 1), new(HardwarePortCapability.AnalogJoystick, 2)],
            [FirmwareCategory.OperatingSystem], [MediaCategory.Cartridge, MediaCategory.MemoryCard])
    ]);
}
