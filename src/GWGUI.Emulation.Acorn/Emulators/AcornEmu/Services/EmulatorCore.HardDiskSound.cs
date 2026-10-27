using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

internal sealed partial class EmulatorCore
{
    private readonly int[] _hardDiskSoundVolume = new int[C.HardDiskCount];
    private readonly bool[] _hardDiskSoundEnabled = new bool[C.HardDiskCount];
    private readonly IEmulationDriveMechanics?[] _hardDiskMechanics = new IEmulationDriveMechanics?[C.HardDiskCount];

    /// <summary>Rend les mouvements et la rotation du Winchester par la mécanique commune.</summary>
    private void MixHardDiskSound(double[] samples)
    {
        if (_machine is not { } machine) return;
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            if (_hardDiskMechanics[unit] is not { } mechanics) continue;
            mechanics.Configure(GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StoragePresentationConstants.WinchesterSound, _hardDiskSoundVolume[unit],
                _hardDiskConnected[unit] && _hardDiskSoundEnabled[unit] && _hardDiskSettings[unit].Speed == C.DefaultSpeedPercent);
            mechanics.SetMotor(_hardDiskConnected[unit] && machine.HardDiskMotor(unit));
            mechanics.SetActivity(machine.HardDiskActive(unit) && !machine.HardDiskWriting(unit), machine.HardDiskWriting(unit));
            var sound = mechanics.RenderAudio(SampleRate, samples.Length);
            for (var index = 0; index < sound.Length; index++) samples[index] += sound[index];
        }
    }
}
