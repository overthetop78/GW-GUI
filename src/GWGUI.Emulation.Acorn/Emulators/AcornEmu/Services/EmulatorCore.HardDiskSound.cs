using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

internal sealed partial class EmulatorCore
{
    private readonly int[] _hardDiskSoundVolume = new int[C.HardDiskCount];
    private readonly bool[] _hardDiskSoundEnabled = new bool[C.HardDiskCount];
    private readonly string?[] _hardDiskSoundProfiles = new string?[C.HardDiskCount];
    private readonly long[] _hardDiskSoundOffsets = new long[C.HardDiskCount], _hardDiskReadBlocks = new long[C.HardDiskCount], _hardDiskWrittenBlocks = new long[C.HardDiskCount], _hardDiskSeekCount = new long[C.HardDiskCount];
    private readonly int[] _hardDiskSeekRemaining = new int[C.HardDiskCount], _hardDiskSeekOffset = new int[C.HardDiskCount];

    /// <summary>Le disque tourne pendant son alimentation ; les accès SCSI et les mouvements réels déclenchent les autres composantes.</summary>
    private void MixHardDiskSound(double[] samples)
    {
        if (_machine is not { } machine) return;
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            var read = machine.HardDiskReadBlocks(unit); var written = machine.HardDiskWrittenBlocks(unit); var seeks = machine.HardDiskSeekCount(unit);
            var operation = written != _hardDiskWrittenBlocks[unit] ? S.Write : read != _hardDiskReadBlocks[unit] ? S.Read : S.Motor;
            var pending = (int)Math.Clamp(seeks - _hardDiskSeekCount[unit], 0, S.HardDiskEventQueueCapacity);
            _hardDiskReadBlocks[unit] = read; _hardDiskWrittenBlocks[unit] = written; _hardDiskSeekCount[unit] = seeks;
            if (!_hardDiskConnected[unit] || !_hardDiskSoundEnabled[unit] || _hardDiskSettings[unit].Speed != C.DefaultSpeedPercent || !machine.HardDiskMotor(unit))
            {
                _hardDiskSoundOffsets[unit] = 0; _hardDiskSeekRemaining[unit] = _hardDiskSeekOffset[unit] = 0;
                continue;
            }
            var profile = _hardDiskSoundProfiles[unit];
            var volume = _hardDiskSoundVolume[unit];
            Mix(CommonHost.MediaProcessing.RenderMechanicalSound(S.HardDisk, operation, SampleRate, samples.Length, _hardDiskSoundOffsets[unit], profile, volume), 0);
            _hardDiskSoundOffsets[unit] += samples.Length;
            _hardDiskSeekRemaining[unit] = Math.Min(S.HardDiskEventQueueCapacity, _hardDiskSeekRemaining[unit] + pending);
            var destination = 0;
            while (destination < samples.Length && _hardDiskSeekRemaining[unit] > 0)
            {
                var duration = Math.Max(1, (int)Math.Ceiling(SampleRate * CommonHost.MediaProcessing.MechanicalSoundDurationSeconds(S.HardDisk, profile ?? S.Winchester, S.Seek)));
                var count = Math.Min(samples.Length - destination, duration - _hardDiskSeekOffset[unit]);
                Mix(CommonHost.MediaProcessing.RenderMechanicalSound(S.HardDisk, S.Seek, SampleRate, count, _hardDiskSeekOffset[unit], profile, volume), destination);
                destination += count; _hardDiskSeekOffset[unit] += count;
                if (_hardDiskSeekOffset[unit] >= duration) { _hardDiskSeekRemaining[unit]--; _hardDiskSeekOffset[unit] = 0; }
            }
        }
        void Mix(short[] noise, int start)
        {
            for (var index = 0; index < noise.Length; index++) samples[start + index] += noise[index];
        }
    }
}
