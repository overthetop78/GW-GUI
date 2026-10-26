using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

internal sealed partial class EmulatorCore
{
    private readonly long[] _floppySoundOffsets = new long[C.DriveCount], _floppySeekSteps = new long[C.DriveCount], _floppyEventOffsets = new long[C.DriveCount];
    private readonly Queue<string>[] _floppySoundEvents = [new(), new()];
    private readonly bool[] _floppyWasRunning = new bool[C.DriveCount];
    private readonly long[] _floppyReadBytes = new long[C.DriveCount], _floppyWrittenBytes = new long[C.DriveCount];

    /// <summary>Consomme les états mécaniques réels ; les échantillons produits par MediaEngine restent séparés des secteurs.</summary>
    private void MixFloppySound(double[] samples)
    {
        if (_machine is not { } machine) return;
        for (var drive = 0; drive < C.DriveCount; drive++)
        {
            var steps = machine.FloppySeekSteps(drive);
            var pending = Math.Clamp(steps - _floppySeekSteps[drive], 0, S.EventQueueCapacity);
            _floppySeekSteps[drive] = steps;
            var readBytes = machine.FloppyReadBytes(drive); var writtenBytes = machine.FloppyWrittenBytes(drive);
            var read = readBytes != _floppyReadBytes[drive]; var written = writtenBytes != _floppyWrittenBytes[drive];
            _floppyReadBytes[drive] = readBytes; _floppyWrittenBytes[drive] = writtenBytes;
            var settings = _floppySettings[drive];
            var profile = settings?.Options?.GetValueOrDefault(C.SoundProfileOption, S.Floppy525);
            var volume = EmulationMechanicalSoundSettings.ReadVolume(settings?.Options);
            var enabled = _floppyConnected[drive] && settings is { Speed: C.DefaultSpeed } && bool.Parse(settings.Options?.GetValueOrDefault(C.MechanicalSoundOption, bool.TrueString) ?? bool.TrueString);
            var running = machine.FloppyMotor(drive);
            if (!enabled)
            {
                _floppySoundEvents[drive].Clear(); _floppyEventOffsets[drive] = 0; _floppyWasRunning[drive] = running;
                continue;
            }
            if (running)
            {
                var motorOperation = written || machine.FloppyWriting(drive) ? S.Write : read || machine.FloppyReading(drive) ? S.Read : S.Motor;
                Mix(CommonHost.MediaProcessing.RenderMechanicalSound(S.Floppy, motorOperation, SampleRate, samples.Length, _floppySoundOffsets[drive], profile, volume), 0);
                _floppySoundOffsets[drive] += samples.Length;
            }
            else
            {
                if (_floppyWasRunning[drive]) QueueFloppySound(drive, S.Stop);
                _floppySoundOffsets[drive] = 0;
            }
            _floppyWasRunning[drive] = running;
            for (var step = 0; step < pending; step++) QueueFloppySound(drive, S.Seek);
            var destination = 0;
            while (destination < samples.Length && _floppySoundEvents[drive].TryPeek(out var eventOperation))
            {
                var duration = Math.Max(1, (int)Math.Ceiling(SampleRate * CommonHost.MediaProcessing.MechanicalSoundDurationSeconds(S.Floppy, profile ?? S.Floppy525, eventOperation)));
                var count = Math.Min(samples.Length - destination, duration - (int)_floppyEventOffsets[drive]);
                Mix(CommonHost.MediaProcessing.RenderMechanicalSound(S.Floppy, eventOperation, SampleRate, count, _floppyEventOffsets[drive], profile, volume), destination);
                destination += count; _floppyEventOffsets[drive] += count;
                if (_floppyEventOffsets[drive] >= duration) { _floppySoundEvents[drive].Dequeue(); _floppyEventOffsets[drive] = 0; }
            }
        }
        void Mix(short[] noise, int start)
        {
            for (var index = 0; index < noise.Length; index++) samples[start + index] += noise[index];
        }
    }

    private void QueueFloppySound(int drive, string operation)
    {
        if (_floppySoundEvents[drive].Count < S.EventQueueCapacity) _floppySoundEvents[drive].Enqueue(operation);
    }
}
