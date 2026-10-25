using GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Constants;

namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Dictionaries;

/// <summary>Déclaration matérielle du profil A, utilisée de la même façon par les trois émulateurs.</summary>
/// <remarks>La déclaration est descriptive : elle ne crée ni lecteur physique, ni cœur, ni fichier de contenu.</remarks>
internal static class ExampleMachineAModelCatalog
{
    /// <summary>Profil A et ses ports génériques, ses lecteurs et ses capacités de simulation.</summary>
    internal static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly<HardwareModelDefinition>(
    [
        new(MachineModel.ExampleMachineA, ModelConstants.Id, ModelConstants.DisplayNameResourceKey, Emulator.Internal,
            [HardwareCpu.ExampleCpu], ModelConstants.DefaultCpuFrequencyHz, ModelConstants.MainMemoryBytes,
            [HardwareRegion.Pal, HardwareRegion.Ntsc], [HardwareVideoCapability.Raster], [HardwareAudioCapability.Pcm],
            [HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette, HardwareStorageCapability.HardDisk],
            [new(HardwarePortCapability.Keyboard, 1), new(HardwarePortCapability.Mouse, 1), new(HardwarePortCapability.AnalogJoystick, 2)],
            [FirmwareCategory.OperatingSystem, FirmwareCategory.VoiceSamples], [MediaCategory.Floppy, MediaCategory.Cassette, MediaCategory.HardDisk])
    ]);
}
