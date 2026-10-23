using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Interfaces;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Common.Services;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Interop;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class ControllerFeedbackTests
{
    [Theory]
    [InlineData("rpcs3", "PlayStation3", PeripheralCategory.Joystick, true)]
    [InlineData("pcee2", "PlayStation2", PeripheralCategory.Joystick, true)]
    [InlineData("pcsx2", "PlayStation2", PeripheralCategory.Joystick, true)]
    [InlineData("mednafen_psx", "PlayStation", PeripheralCategory.DualShock, true)]
    [InlineData("mednafen_psx_hw", "PlayStation", PeripheralCategory.DualShock, true)]
    [InlineData("pcsx_rearmed", "PlayStation", PeripheralCategory.DualShock, true)]
    [InlineData("swanstation", "PlayStation", PeripheralCategory.DualShock, true)]
    [InlineData("mednafen_psx", "PlayStation", PeripheralCategory.Joystick, false)]
    [InlineData("mednafen_psx", "PlayStation", PeripheralCategory.AnalogController, false)]
    [InlineData("mednafen_psx", "PlayStation", PeripheralCategory.NeGcon, false)]
    [InlineData("play", "PlayStation2", PeripheralCategory.Joystick, false)]
    [InlineData("ppsspp", "Psp", PeripheralCategory.Joystick, false)]
    public void CapabilitiesBelongToTheSelectedCoreAndPeripheral(string emulator, string model, PeripheralCategory type, bool expected)
    {
        var configuration = new MachineConfiguration(model, emulator);
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = settings.ControllerPorts.First(port => !port.IsUsb);
        var choice = Assert.Single(port.ControllerChoices, item => item.Id == type.ToString());
        if (expected)
        {
            var outputs = choice.VisualOutputDefinitions![choice.DefaultVisualId!];
            Assert.Equal(new[] { EmulationControllerFeedbackCommands.Strong, EmulationControllerFeedbackCommands.Weak }, outputs.Select(item => item.Id));
        }
        else Assert.Null(choice.VisualOutputDefinitions);
        Assert.All(settings.ControllerPorts, port => Assert.Empty(port.OutputBindings));
    }

    [Fact]
    public void OutputBindingsRoundTripAndRejectUnsupportedVisualsAndCommands()
    {
        var configuration = new MachineConfiguration("PlayStation3", "rpcs3");
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = settings.ControllerPorts[0] with
        {
            SelectedControllerId = nameof(PeripheralCategory.Joystick),
            VisualId = EmulationControllerVisualIds.SonyDualShock3,
            OutputBindings = new Dictionary<string, EmulationControllerOutputBinding>
            {
                [EmulationControllerFeedbackCommands.Strong] = new("physical-device", EmulationControllerMotor.LowFrequency),
                ["Unknown"] = new("physical-device", EmulationControllerMotor.HighFrequency)
            }
        };
        var applied = InputSettingsFunctions.Apply(configuration, settings with { ControllerPorts = [port] });
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(applied))!;
        var saved = Assert.Single(InputSettingsFunctions.Describe(restored).ControllerPorts[0].OutputBindings);
        Assert.Equal(EmulationControllerFeedbackCommands.Strong, saved.Key);
        Assert.Equal("physical-device", saved.Value.DeviceId);
        Assert.Equal(EmulationControllerMotor.LowFrequency, saved.Value.Motor);
        var wrongVisual = InputSettingsFunctions.Apply(configuration, settings with { ControllerPorts = [port with { VisualId = EmulationControllerVisualIds.SonyDualShock2 }] });
        Assert.Empty(InputSettingsFunctions.Describe(wrongVisual).ControllerPorts[0].OutputBindings);
    }

    [Fact]
    public void NativeCallbackKeepsEffectsIndependentAndRejectsInvalidIndexes()
    {
        using var feedback = new NativeControllerFeedback();
        var destination = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.RumbleInterface>());
        try
        {
            Assert.False(feedback.Register(nint.Zero));
            Assert.True(feedback.Register(destination));
            var native = Marshal.PtrToStructure<ExternalCoreApi.RumbleInterface>(destination);
            var callback = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetRumbleState>(native.SetState);
            Assert.True(callback(0, 0, 32768));
            Assert.True(callback(0, 1, ushort.MaxValue));
            Assert.False(callback(8, 0, 1));
            Assert.False(callback(0, 2, 1));
            var states = feedback.Snapshot();
            Assert.Equal(2, states.Count);
            Assert.InRange(states[0].Strength, .5f, .501f);
            Assert.Equal(1f, states[1].Strength);
            Assert.True(callback(0, 0, 0));
            Assert.Equal(EmulationControllerFeedbackCommands.Weak, Assert.Single(feedback.Snapshot()).CommandId);
            feedback.ResetPort(0);
            Assert.Empty(feedback.Snapshot());
            feedback.Dispose();
            Assert.False(callback(0, 1, 1));
            GC.KeepAlive(callback);
        }
        finally { Marshal.FreeHGlobal(destination); }
    }

    [Fact]
    public void ProtocolRoundTripsACompleteSnapshotWithoutConsumingTheNextField()
    {
        ControllerFeedbackState[] states = [new(0, EmulationControllerFeedbackCommands.Strong, .25f), new(7, EmulationControllerFeedbackCommands.Weak, 1f)];
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        EmulationSdk.HostProtocol.WriteControllerFeedback(writer, states);
        writer.Write(123456);
        writer.Flush();
        stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        Assert.Equal(states, EmulationSdk.HostProtocol.ReadControllerFeedback(reader));
        Assert.Equal(123456, reader.ReadInt32());
    }

    [Theory]
    [InlineData(-1, 0, .5f)]
    [InlineData(21, 0, .5f)]
    [InlineData(1, 10, .5f)]
    [InlineData(1, -1, .5f)]
    [InlineData(1, 0, -1f)]
    [InlineData(1, 0, 2f)]
    [InlineData(1, 0, float.NaN)]
    public void ProtocolRejectsInvalidCountsPortsAndStrengths(int count, int port, float strength)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(count); writer.Write(port); writer.Write(true); writer.Write(strength); writer.Flush();
        stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        Assert.Throws<InvalidDataException>(() => EmulationSdk.HostProtocol.ReadControllerFeedback(reader));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MachineEmitsOnlyExplicitBindingsAndClearsThemOutsideRunning(bool assigned)
    {
        var outputs = assigned ? new Dictionary<string, EmulationControllerOutputBinding> { [EmulationControllerFeedbackCommands.Strong] = new("device", EmulationControllerMotor.LowFrequency) } : null;
        var configuration = new MachineConfiguration("PlayStation3", "rpcs3", AudioEnabled: false,
            Input: new InputConfiguration(ControllerBindings: [new(0, PeripheralCategory.Joystick, VisualId: EmulationControllerVisualIds.SonyDualShock3, OutputBindings: outputs)]));
        var core = new FeedbackCore();
        var machine = new Machine(Guid.NewGuid(), configuration, core, [], "unused-session");
        try
        {
            Assert.Empty(machine.ControllerOutputs);
            await machine.StartAsync();
            if (assigned) Assert.Equal(.5f, Assert.Single(machine.ControllerOutputs).Strength);
            else { Assert.Empty(machine.ControllerOutputs); Assert.Equal(0, core.FeedbackReads); }
            await machine.PauseAsync();
            Assert.Empty(machine.ControllerOutputs);
            await machine.ResumeAsync();
            if (assigned) Assert.Single(machine.ControllerOutputs);
            await machine.StopAsync();
            Assert.Empty(machine.ControllerOutputs);
        }
        finally { await machine.DisposeAsync(); }
        Assert.True(core.Disposed);
    }

    private sealed class FeedbackCore : IEmulatorCore
    {
        internal int FeedbackReads;
        internal bool Disposed;
        public IReadOnlyList<ControllerFeedbackState> ControllerFeedback { get { FeedbackReads++; return [new(0, EmulationControllerFeedbackCommands.Strong, .5f), new(0, EmulationControllerFeedbackCommands.Weak, 1f)]; } }
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<CoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => "Test";
        public string CoreVersion => "1";
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public string CoreSha256 => string.Empty;
        public double FramesPerSecond => 60;
        public int SampleRate => 44100;
        public int DiskCount => 0;
        public int CurrentDiskIndex => -1;
        public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null) { }
        public void RunFrame() { }
        public void Stop() { }
        public void Dispose() => Disposed = true;
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void HardReset() { }
        public void SoftReset() { }
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, PeripheralCategory type) { }
        public void InsertMedia(string path) => throw new NotSupportedException();
        public void EjectMedia() => throw new NotSupportedException();
        public void SelectDisk(int index) => throw new NotSupportedException();
        public byte[] SaveState() => throw new NotSupportedException();
        public void LoadState(ReadOnlySpan<byte> state) => throw new NotSupportedException();
        public void SetOption(string key, string value) => throw new NotSupportedException();
    }
}
