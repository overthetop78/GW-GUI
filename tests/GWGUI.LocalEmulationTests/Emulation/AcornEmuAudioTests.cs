using System.Reflection;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;
using GWGUI.Emulation.Enums;
using BbcAudio = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services.Audio;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Factories;
using CommonAudioOutput = GWGUI.Emulation.Acorn.Common.Services.AudioOutputController;

namespace GWGUI.Tests.Emulation;

/// <summary>Contrôles autonomes du son BBC : bus et ROM synthétiques, aucune sortie physique ni fichier externe.</summary>
public sealed class AcornEmuAudioTests
{
    [Fact]
    public async Task FactoryConnectsTheProvidedAudioOutputAndSessionDisposesIt()
    {
        var output = new Output();
        var calls = 0;
        var configuration = new MachineConfiguration(MachineModel.BbcMicroModelA, core: Emulator.AcornEmu);
        var context = new EmulatorCreationContext<MachineConfiguration>("synthetic-audio-sessions", "", "", () => { calls++; return output; }, null);
        await using (var machine = new AcornEmuMachineFactory().Create(configuration, context))
        {
            // Vérifie la fabrique et le relais Common sans démarrer de CPU, lire de firmware ou ouvrir de périphérique.
            typeof(GWGUI.Emulation.Acorn.Common.Services.Machine).GetMethod("StartAudio", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(machine, null);
            var audio = (CommonAudioOutput)typeof(GWGUI.Emulation.Acorn.Common.Services.Machine).GetField("_audio", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(machine)!;
            audio.Write(new AudioChunk(new short[] { -123, 456 }, 44100, 2, 1, TimeSpan.Zero, EmulationAudioChannelLayout.Mono));
            Assert.Equal(1, calls); Assert.Equal(44100, output.SampleRate);
            Assert.Equal(EmulationAudioChannelLayout.Mono, output.Layout);
            Assert.Equal(new short[] { -123, 456 }, output.Samples);
        }
        Assert.True(output.Disposed);
    }

    /// <summary>Sortie mémoire possédée par Common : conserve le format et les échantillons reçus, sans matériel.</summary>
    private sealed class Output : IMultichannelAudioOutput
    {
        internal int SampleRate;
        internal EmulationAudioChannelLayout Layout;
        internal short[] Samples = [];
        internal bool Disposed;
        public void Start(int sampleRate) => Start(sampleRate, EmulationAudioChannelLayout.Stereo);
        public void Start(int sampleRate, EmulationAudioChannelLayout channelLayout) { SampleRate = sampleRate; Layout = channelLayout; }
        public void Write(ReadOnlySpan<short> samples) => Samples = samples.ToArray();
        public void Flush() { }
        public void Stop() { }
        public void Dispose() => Disposed = true;
    }

    [Theory]
    [InlineData(250, 500.0)]
    [InlineData(284, 440.1408450704225)]
    [InlineData(0, 122.0703125)]
    public void PcmPreservesHardwarePitchAndSampleRate(int divisor, double expected)
    {
        var audio = new BbcAudio();
        Tone(audio.Write, divisor);
        for (var tick = 0; tick < 2_000_000; tick++) audio.Tick();
        var samples = audio.DrainSamples();
        Assert.Equal(44100, samples.Length);
        var crossings = Enumerable.Range(4411, samples.Length - 4411).Where(index => samples[index - 1] <= 0 && samples[index] > 0).ToArray();
        Assert.True(crossings.Length > 100);
        var measured = 44100.0 * (crossings.Length - 1) / (crossings[^1] - crossings[0]);
        Assert.InRange(measured, expected - 0.06, expected + 0.06);
    }

    [Fact]
    public void PartialChunksKeepTheSamplingPhaseAndMutedChipIsSilent()
    {
        var audio = new BbcAudio();
        Mute(audio.Write);
        var total = 0;
        for (var tick = 1; tick <= 2_000_000; tick++)
        {
            audio.Tick();
            if (tick % 1337 != 0) continue;
            var samples = audio.DrainSamples(); total += samples.Length;
            Assert.All(samples, sample => Assert.Equal(0, sample));
        }
        total += audio.DrainSamples().Length;
        Assert.Equal(44100, total);
        audio.Reset();
        Assert.Empty(audio.DrainSamples());
    }

    [Fact]
    public void ChipUsesThreeIndependentTonesAndTwoDecibelAttenuation()
    {
        for (var channel = 0; channel < 3; channel++)
        {
            var chip = new Sn76489(); Mute(chip.Write);
            chip.Write((byte)(0x80 | channel << 5 | 10)); chip.Write(0);
            chip.Write((byte)(0x90 | channel << 5));
            for (var tick = 0; tick < 16; tick++) chip.Tick();
            var full = chip.Output; Assert.True(full > 0);
            chip.Write((byte)(0x91 | channel << 5));
            Assert.InRange((double)chip.Output / full, 0.794, 0.795);
            chip.Write(15); // Un octet sans bit de sélection modifie le volume déjà sélectionné.
            Assert.Equal(0, chip.Output);
        }
    }

    [Fact]
    public void NoiseIsDeterministicAndWhiteNoiseDiffersFromPeriodicNoise()
    {
        int[] Sequence(byte mode)
        {
            var chip = new Sn76489(); Mute(chip.Write); chip.Write(mode); chip.Write(0xf0);
            var values = new int[96];
            for (var index = 0; index < values.Length; index++)
            {
                for (var tick = 0; tick < (index == 0 ? 16 : 512); tick++) chip.Tick();
                values[index] = chip.Output;
            }
            return values;
        }
        var periodic = Sequence(0xe0); var white = Sequence(0xe4);
        Assert.Equal(periodic, Sequence(0xe0)); Assert.Equal(white, Sequence(0xe4));
        Assert.Contains(periodic, value => value != 0); Assert.Contains(white, value => value != 0);
        Assert.False(periodic.SequenceEqual(white));
        for (var index = 16; index < periodic.Length; index++) Assert.Equal(periodic[index - 15], periodic[index]);
    }

    [Fact]
    public void ViaWritesProduceMonoAndResetClearsPendingAudio()
    {
        var machine = new ModelAMachine(new byte[16384], new byte[16384]);
        void Write(byte value)
        {
            machine.Write(0xfe41, value); // Bus PA : données.
            machine.Write(0xfe40, 0);     // IC32 sortie 0 basse : validation PSG.
            machine.Write(0xfe40, 8);     // Relâche la validation avant l'octet suivant.
        }
        machine.Write(0xfe43, 255); machine.Write(0xfe42, 15);
        Tone(Write, 284);
        using var core = new EmulatorCore(MachineModel.BbcMicroModelA);
        // Assemble une carte synthétique sans passer par la lecture des ROM externes de la fabrique.
        typeof(EmulatorCore).GetField("_machine", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(core, machine);
        for (var frame = 0; frame < 4; frame++) core.RunFrame();
        var chunks = new List<GWGUI.Emulation.Contracts.AudioChunk>();
        while (core.TryDequeueAudio(out var chunk)) chunks.Add(Assert.IsType<GWGUI.Emulation.Contracts.AudioChunk>(chunk));
        Assert.Equal(4, chunks.Count);
        Assert.All(chunks, chunk => { Assert.True(chunk.HasValidLength); Assert.Equal(EmulationAudioChannelLayout.Mono, chunk.ChannelLayout); Assert.Equal(44100, chunk.SampleRate); Assert.Equal(chunk.FrameCount, chunk.InterleavedSamples.Length); });
        Assert.Contains(chunks.SelectMany(chunk => chunk.InterleavedSamples.ToArray()), sample => sample != 0);
        for (var index = 1; index < chunks.Count; index++)
            Assert.InRange((chunks[index].Timestamp - chunks[index - 1].Timestamp).TotalSeconds, chunks[index - 1].FrameCount / 44100.0 - 1e-6, chunks[index - 1].FrameCount / 44100.0 + 1e-6);
        Assert.Equal(0, core.BufferedAudioFrames);
        core.RunFrame(); core.HardReset();
        Assert.Null(core.LatestAudioChunk); Assert.False(core.TryDequeueAudio(out _));
        core.RunFrame(); core.Stop();
        Assert.False(core.TryDequeueAudio(out _));
    }

    /// <summary>Programme un ton par les deux octets matériels, après coupure des quatre voies.</summary>
    private static void Tone(Action<byte> write, int divisor)
    {
        Mute(write); write((byte)(0x80 | (divisor & 15))); write((byte)(divisor >> 4)); write(0x90);
    }

    /// <summary>Atténuation 15 : silence matériel de chaque voie, indépendamment du volume de l'application.</summary>
    private static void Mute(Action<byte> write) { write(0x9f); write(0xbf); write(0xdf); write(0xff); }
}
