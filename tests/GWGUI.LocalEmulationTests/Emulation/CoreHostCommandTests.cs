using GWGUI.Emulation;
using System.IO;
using System.Text;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class CoreHostCommandTests
{
    [Theory]
    [InlineData(3, "HardReset")]
    [InlineData(16, "SoftReset")]
    [InlineData(4, "Stop")]
    [InlineData(5, "InsertMedia")]
    [InlineData(6, "EjectMedia")]
    [InlineData(7, "SaveState")]
    [InlineData(8, "LoadState")]
    [InlineData(9, "SetOption")]
    [InlineData(10, "SelectDisk")]
    [InlineData(12, "SaveMediaChanges")]
    [InlineData(14, "HasUnsavedMediaChanges")]
    [InlineData(13, "GetDiskStatus")]
    [InlineData(15, "SetControllerPortDevice")]
    public void NativeCommandsKeepTheirPayloadAndResponse(byte command, string expectedCall)
    {
        using var payload = new MemoryStream();
        using (var encoder = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true))
        {
            switch (command)
            {
                case 5: encoder.Write("disk.adf"); break;
                case 6: EmulationSdk.HostProtocol.WriteMediaSlot(encoder, new(EmulationMediaSlot.MemoryCard0.Category, 1)); break;
                case 8: encoder.Write(3); encoder.Write(new byte[] { 4, 5, 6 }); break;
                case 9: encoder.Write("mode"); encoder.Write("accurate"); break;
                case 10: encoder.Write(4); break;
                case 12: case 14: EmulationSdk.HostProtocol.WriteMediaSlot(encoder, new(EmulationMediaSlot.MemoryCard0.Category, 1)); break;
                case 15: encoder.Write(2); encoder.Write((int)TestPeripheral.Pointer); break;
            }
        }
        payload.Position = 0;
        using var reader = new BinaryReader(payload, Encoding.UTF8, leaveOpen: true);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response, Encoding.UTF8, leaveOpen: true);
        var core = new Core();
        var acquired = 0;
        Assert.True(Handle(command, reader, writer, () => { acquired++; return core; }));
        writer.Flush();
        Assert.Equal(1, acquired);
        Assert.Equal(new[] { expectedCall }, core.Calls);
        Assert.Equal(payload.Length, payload.Position);
        switch (command)
        {
            case 5: Assert.Equal("disk.adf", core.Media); break;
            case 6: Assert.Equal(new EmulationMediaSlot(EmulationMediaSlot.MemoryCard0.Category, 1), core.Slot); break;
            case 7: Assert.Equal(new byte[] { 3, 0, 0, 0, 7, 8, 9 }, response.ToArray()); break;
            case 8: Assert.Equal(new byte[] { 4, 5, 6 }, core.State); break;
            case 9: Assert.Equal(("mode", "accurate"), core.Option); break;
            case 10: Assert.Equal(4, core.Disk); break;
            case 12: case 14: Assert.Equal(new EmulationMediaSlot(EmulationMediaSlot.MemoryCard0.Category, 1), core.Slot); break;
            case 13:
                response.Position = 0;
                using (var decoder = new BinaryReader(response, Encoding.UTF8, leaveOpen: true))
                    Assert.Equal("disk:4", decoder.ReadString());
                break;
            case 15: Assert.Equal((2, TestPeripheral.Pointer), core.Controller); break;
        }
        if (command == 14) Assert.Equal(new byte[] { 1 }, response.ToArray());
        else if (command is not (7 or 13)) Assert.Equal(0, response.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(11)]
    [InlineData(255)]
    public void CommandsOwnedByTheHostDoNotAcquireACoreOrReadArguments(byte command)
    {
        using var payload = new MemoryStream(new byte[] { 42 });
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        Assert.False(Handle(command, reader, writer, () => throw new InvalidOperationException("must not acquire")));
        Assert.Equal(0, payload.Position);
        Assert.Equal(0, response.Length);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(15)]
    public void MissingCoreFailsBeforeReadingAnyArgument(byte command)
    {
        using var payload = new MemoryStream(new byte[] { 42 });
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var expected = new InvalidOperationException("not initialized");
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(
            () => Handle(command, reader, writer, () => throw expected)));
        Assert.Equal(0, payload.Position);
        Assert.Equal(0, response.Length);
    }

    [Fact]
    public void TruncatedStateUsesTheExistingBinaryCodecAndNeverReachesTheCore()
    {
        using var payload = new MemoryStream(new byte[] { 3, 0, 0, 0, 9 });
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core();
        var acquired = 0;
        Assert.Throws<EndOfStreamException>(() => Handle(8, reader, writer, () => { acquired++; return core; }));
        Assert.Equal(1, acquired);
        Assert.Empty(core.Calls);
        Assert.Empty(core.State);
        Assert.Equal(0, response.Length);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(14)]
    [InlineData(5)]
    [InlineData(6)]
    public void PathMediaCommandsPreserveTheirPayloadAndSuccessAcknowledgement(byte command)
    {
        using var payload = new MemoryStream();
        using (var encoder = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true))
        {
            if (command == 5) encoder.Write("disk.adf");
            if (command is 12 or 14) EmulationSdk.HostProtocol.WriteMediaSlot(encoder, new(EmulationMediaSlot.MemoryCard0.Category, 1));
            encoder.Write(42);
        }
        payload.Position = 0;
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core();
        var acquired = 0;
        Assert.True(HandlePathMedia(command, reader, writer, () => { acquired++; return core; }));
        Assert.Equal(1, acquired);
        Assert.Equal(new[] { command switch { 5 => "InsertMedia", 6 => "EjectMedia", 12 => "SaveMediaChanges", _ => "HasUnsavedMediaChanges" } }, core.Calls);
        if (command == 5) Assert.Equal("disk.adf", core.Media);
        Assert.Equal(42, reader.ReadInt32());
        Assert.Equal(payload.Length, payload.Position);
        Assert.Equal(command == 14 ? new byte[] { 1, 1 } : new byte[] { 1 }, response.ToArray());
        if (command is 12 or 14) Assert.Equal(new EmulationMediaSlot(EmulationMediaSlot.MemoryCard0.Category, 1), core.Slot);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(11)]
    [InlineData(13)]
    [InlineData(15)]
    [InlineData(255)]
    public void PathMediaCommandsLeaveHostAndNativeOnlyCommandsUntouched(byte command)
    {
        using var payload = new MemoryStream(new byte[] { 42 });
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        Assert.False(HandlePathMedia(command, reader, writer,
            () => throw new InvalidOperationException("must not acquire")));
        Assert.Equal(0, payload.Position);
        Assert.Equal(0, response.Length);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    public void PathMediaMissingCoreDoesNotReadPayloadOrAcknowledge(byte command)
    {
        using var payload = new MemoryStream(new byte[] { 42 });
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var failure = new InvalidOperationException("not initialized");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(
            () => HandlePathMedia(command, reader, writer, () => throw failure)));
        Assert.Equal(0, payload.Position);
        Assert.Equal(0, response.Length);
    }

    [Fact]
    public void PathMediaTruncatedPathNeverReachesCoreOrAcknowledges()
    {
        using var payload = new MemoryStream(new byte[] { 3, 65 });
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core();
        Assert.Throws<EndOfStreamException>(() => HandlePathMedia(5, reader, writer, () => core));
        Assert.Empty(core.Calls);
        Assert.Equal(0, response.Length);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    public void PathMediaFailureNeverSendsSuccess(byte command)
    {
        using var payload = new MemoryStream();
        using (var encoder = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true))
            encoder.Write("disk.adf");
        payload.Position = 0;
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core { FailMedia = true };
        Assert.Throws<InvalidOperationException>(() => HandlePathMedia(command, reader, writer, () => core));
        Assert.Equal(0, response.Length);
    }

    [Fact]
    public void PathMediaDelegatesStateCommandToExistingOwner()
    {
        using var payload = new MemoryStream();
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core();
        Assert.True(HandlePathMedia(7, reader, writer, () => core));
        Assert.Equal(new[] { "SaveState" }, core.Calls);
        Assert.Equal(new byte[] { 1, 3, 0, 0, 0, 7, 8, 9 }, response.ToArray());
    }

    [Fact]
    public void DiskSelectionWritesCountBeforeIndexWithoutSuccessPrefix()
    {
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core();
        EmulationSdk.HostProtocol.WriteDiskSelection(writer, core);
        Assert.Equal(new[] { "DiskCount", "CurrentDiskIndex" }, core.Calls);
        response.Position = 0;
        using var reader = new BinaryReader(response);
        Assert.Equal(3, reader.ReadInt32());
        Assert.Equal(2, reader.ReadInt32());
        Assert.Equal(response.Length, response.Position);
    }

    private static bool HandlePathMedia(byte command, BinaryReader reader, BinaryWriter writer,
        Func<Core> acquire) =>
        EmulationSdk.HostProtocol.TryExecutePathMediaCommand(command, reader, writer, () => acquire(),
            static input => EmulationHostProtocolFunctions.ReadBytes(input, "test"),
            static output => output.Write(true));
    private static bool Handle(byte command, BinaryReader reader, BinaryWriter writer, Func<Core> acquire) =>
        EmulationSdk.HostProtocol.TryExecuteCoreCommand<string, TestPeripheral>(
            command, reader, writer, () => acquire(), static input => input.ReadString(),
            static input => EmulationHostProtocolFunctions.ReadBytes(input, "test"),
            static (output, status) => output.Write("disk:" + status.ImageCount), static value => (TestPeripheral)value);

    private enum TestPeripheral { Pointer = 37 }

    [Fact]
    public void PathSoftResetUsesTheSharedStateDispatcher()
    {
        using var payload = new MemoryStream();
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core();
        Assert.True(HandlePathMedia(16, reader, writer, () => core));
        Assert.Equal(new[] { "SoftReset" }, core.Calls);
        Assert.Equal(new byte[] { 1 }, response.ToArray());
        Assert.Equal(0, payload.Position);
    }
    [Fact]
    public void PathDiskStatusPreservesTheSharedRecordAndSuccessAcknowledgement()
    {
        using var payload = new MemoryStream();
        using var reader = new BinaryReader(payload);
        using var response = new MemoryStream();
        using var writer = new BinaryWriter(response);
        var core = new Core();
        Assert.True(EmulationSdk.HostProtocol.TryExecutePathMediaCommand(13, reader, writer, () => core,
            _ => throw new InvalidOperationException("Aucun argument binaire ne doit être lu."),
            output => output.Write(true), (output, status) => output.Write(System.Text.Json.JsonSerializer.Serialize(status))));
        response.Position = 0;
        using var decoder = new BinaryReader(response, Encoding.UTF8, leaveOpen: true);
        Assert.True(decoder.ReadBoolean());
        var status = System.Text.Json.JsonSerializer.Deserialize<DiskStatus>(decoder.ReadString())!;
        Assert.Equal(new[] { "GetDiskStatus" }, core.Calls);
        Assert.Equal(4, status.ImageCount);
        Assert.Equal(2, status.CurrentIndex);
        Assert.False(status.IsEjected);
        var image = Assert.Single(status.Images);
        Assert.Equal(2, image.Index);
        Assert.Equal("disk-two.adf", image.Path);
        Assert.Equal("Disque 2", image.Label);
        Assert.Equal(0, payload.Position);
        Assert.Equal(response.Length, response.Position);
    }
    private sealed class Core : IEmulationCoreCommands<string, TestPeripheral>, IEmulationCorePathMediaCommands
    {
        internal List<string> Calls { get; } = [];
        internal string? Media { get; private set; }
        internal EmulationMediaSlot Slot { get; private set; }
        internal byte[] State { get; private set; } = [];
        internal (string, string) Option { get; private set; }
        internal int Disk { get; private set; }
        internal (int, TestPeripheral) Controller { get; private set; }
        internal bool FailMedia { get; init; }
        public void SaveMediaChanges(EmulationMediaSlot slot) { Calls.Add(nameof(SaveMediaChanges)); Slot = slot; }
        public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) { Calls.Add(nameof(HasUnsavedMediaChanges)); Slot = slot; return true; }
        public int DiskCount { get { Calls.Add(nameof(DiskCount)); return 3; } }
        public int CurrentDiskIndex { get { Calls.Add(nameof(CurrentDiskIndex)); return 2; } }
        public void EjectMedia()
        {
            if (FailMedia) throw new InvalidOperationException("media");
            Calls.Add(nameof(EjectMedia));
        }
        public void SoftReset() => Calls.Add(nameof(SoftReset));
        public void HardReset() => Calls.Add(nameof(HardReset));
        public void Stop() => Calls.Add(nameof(Stop));
        public void InsertMedia(string media) { if (FailMedia) throw new InvalidOperationException("media"); Calls.Add(nameof(InsertMedia)); Media = media; }
        public void EjectMedia(EmulationMediaSlot slot) { Calls.Add(nameof(EjectMedia)); Slot = slot; }
        public byte[] SaveState() { Calls.Add(nameof(SaveState)); return [7, 8, 9]; }
        public void LoadState(ReadOnlySpan<byte> state) { Calls.Add(nameof(LoadState)); State = state.ToArray(); }
        public void SetOption(string key, string value) { Calls.Add(nameof(SetOption)); Option = (key, value); }
        public void SelectDisk(int index) { Calls.Add(nameof(SelectDisk)); Disk = index; }
        public DiskStatus GetDiskStatus() { Calls.Add(nameof(GetDiskStatus)); return new(4, 2, false, [new(2, "disk-two.adf", "Disque 2")]); }
        public void SetControllerPortDevice(int port, TestPeripheral peripheral)
        { Calls.Add(nameof(SetControllerPortDevice)); Controller = (port, peripheral); }
    }
}

