using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

internal sealed partial class EmulatorCore
{
    private readonly IEmulationDriveMechanics?[] _floppyMechanics = new IEmulationDriveMechanics?[C.DriveCount];

    /// <summary>Rend la mécanique commune à partir du moteur et des transferts du contrôleur.</summary>
    private void MixFloppySound(double[] samples)
    {
        if (_machine is not { } machine) return;
        for (var drive = 0; drive < C.DriveCount; drive++)
        {
            if (_floppyMechanics[drive] is not { } mechanics) continue;
            var settings = _floppySettings[drive];
            var enabled = _floppyConnected[drive] && settings is { Speed: C.DefaultSpeed }
                && bool.Parse(settings.Options?.GetValueOrDefault(C.MechanicalSoundOption, bool.TrueString) ?? bool.TrueString);
            mechanics.Configure(StoragePresentationConstants.FloppySound,
                EmulationMechanicalSoundSettings.ReadVolume(settings?.Options), enabled);
            mechanics.SetMotor(_floppyConnected[drive] && machine.FloppyMotor(drive));
            mechanics.SetActivity(machine.FloppyReading(drive), machine.FloppyWriting(drive));
            var sound = mechanics.RenderAudio(SampleRate, samples.Length);
            for (var index = 0; index < sound.Length; index++) samples[index] += sound[index];
        }
    }
}
