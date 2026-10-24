using GWGUI.Emulation.Contracts;
using System.IO;
using System.Text;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class CoreStateCommandTests
{
    [Theory]
    [InlineData(3, "HardReset")]
    [InlineData(16, "SoftReset")]
    [InlineData(4, "Stop")]
    [InlineData(7, "SaveState")]
    [InlineData(8, "LoadState")]
    [InlineData(9, "SetOption")]
    [InlineData(10, "SelectDisk")]
    [InlineData(12, "SaveMediaChanges")]
    [InlineData(14, "HasUnsavedMediaChanges")]
    public void NativeSuccessFollowsTheOperationAndPrecedesItsPayload(byte command, string operation)
    {
        using var payload = new MemoryStream();
        using (var encoder = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true))
        {
            switch (command)
            {
                case 8: encoder.Write(3); encoder.Write(new byte[] { 11, 12, 13 }); break;
                case 9: encoder.Write("mode"); encoder.Write("accurate"); break;
                case 10: encoder.Write(4); break;
                case 12: case 14: EmulationHostProtocolFunctions.WriteMediaSlot(encoder, new(EmulationMediaSlot.MemoryCard0.Category, 1)); break;
            }
        }
        payload.Position = 0;
        using var reader = new BinaryReader(payload, Encoding.UTF8, leaveOpen: true);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
        var events = new List<string>();
        var core = new Core(events);
        Assert.True(EmulationHostProtocolFunctions.TryExecuteCoreStateCommand(command, reader, writer,
            () => { events.Add("ensure"); return core; },
            input => EmulationHostProtocolFunctions.ReadBytes(input, "test"),
            response => { events.Add("success"); response.Write(true); }));
        writer.Flush();
        Assert.Equal(new[] { "ensure", operation, "success" }, events);
        Assert.Equal(payload.Length, payload.Position);
        Assert.Equal(command == 7 ? new byte[] { 1, 3, 0, 0, 0, 7, 8, 9 } : command == 14 ? new byte[] { 1, 1 } : new byte[] { 1 }, output.ToArray());
        if (command == 8) Assert.Equal(new byte[] { 11, 12, 13 }, core.State);
        if (command == 9) Assert.Equal(("mode", "accurate"), core.Option);
        if (command == 10) Assert.Equal(4, core.Disk);
        if (command is 12 or 14) Assert.Equal(new EmulationMediaSlot(EmulationMediaSlot.MemoryCard0.Category, 1), core.MediaSlot);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(11)]
    [InlineData(13)]
    [InlineData(15)]
    [InlineData(255)]
    public void OtherOperationsDoNotAcquireACoreOrWriteSuccess(byte command)
    {
        using var payload = new MemoryStream(new byte[] { 42 });
        using var reader = new BinaryReader(payload);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        Assert.False(EmulationHostProtocolFunctions.TryExecuteCoreStateCommand(command, reader, writer,
            () => throw new InvalidOperationException("must not acquire"),
            _ => throw new InvalidOperationException("must not read"),
            _ => throw new InvalidOperationException("must not report success")));
        Assert.Equal(0, payload.Position);
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void MissingCoreFailsBeforeReadingArguments(byte command)
    {
        using var payload = new MemoryStream(new byte[] { 42 });
        using var reader = new BinaryReader(payload);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        var expected = new InvalidOperationException("not initialized");
        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationHostProtocolFunctions.TryExecuteCoreStateCommand(command, reader, writer,
                () => throw expected,
                input => EmulationHostProtocolFunctions.ReadBytes(input, "test"),
                response => response.Write(true)));
        Assert.Same(expected, actual);
        Assert.Equal(0, payload.Position);
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void CoreFailureDoesNotReportNativeSuccess()
    {
        using var payload = new MemoryStream();
        using var reader = new BinaryReader(payload);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        var expected = new InvalidOperationException("reset failed");
        var core = new Core([]) { Failure = expected };
        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationHostProtocolFunctions.TryExecuteCoreStateCommand(3, reader, writer, () => core,
                input => EmulationHostProtocolFunctions.ReadBytes(input, "test"),
                response => response.Write(true)));
        Assert.Same(expected, actual);
        Assert.Equal(0, output.Length);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void InvalidMediaSlotsFailBeforeAcquiringCoreAndKeepBorrowedStreams()
    {
        foreach (var invalid in new[] { new EmulationMediaSlot((GWGUI.Emulation.Enums.EmulationMediaCategory)int.MaxValue, 0), new EmulationMediaSlot(EmulationMediaSlot.MemoryCard0.Category, -1) })
        {
            using var payload = new MemoryStream();
            using var encoder = new BinaryWriter(payload, Encoding.UTF8, leaveOpen: true);
            Assert.Throws<InvalidDataException>(() => EmulationHostProtocolFunctions.WriteMediaSlot(encoder, invalid));
            Assert.Equal(0, payload.Length);
            encoder.Write((int)invalid.Category);
            encoder.Write(invalid.Index);
            encoder.Flush();
            payload.Position = 0;
            using var reader = new BinaryReader(payload, Encoding.UTF8, leaveOpen: true);
            using var output = new MemoryStream();
            using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
            Assert.Throws<InvalidDataException>(() => EmulationHostProtocolFunctions.TryExecuteCoreStateCommand(12, reader, writer,
                () => throw new Xunit.Sdk.XunitException("Invalid slot must not acquire a core"), _ => [], response => response.Write(true)));
            Assert.Equal(0, output.Length);
            Assert.True(payload.CanRead);
            Assert.True(output.CanWrite);
        }
    }
    private sealed class Core(List<string> events) : IEmulationCoreStateCommands
    {
        internal EmulationMediaSlot MediaSlot { get; private set; }
        public void SaveMediaChanges(EmulationMediaSlot slot) { Record(nameof(SaveMediaChanges)); MediaSlot = slot; }
        public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) { Record(nameof(HasUnsavedMediaChanges)); MediaSlot = slot; return true; }
        public DiskStatus GetDiskStatus() => new(0, -1, true, []);
        internal Exception? Failure { get; init; }
        internal byte[] State { get; private set; } = [];
        internal (string, string) Option { get; private set; }
        internal int Disk { get; private set; }
        private void Record(string operation)
        {
            events.Add(operation);
            if (Failure is not null) throw Failure;
        }
        public void SoftReset() => Record(nameof(SoftReset));
        public void HardReset() => Record(nameof(HardReset));
        public void Stop() => Record(nameof(Stop));
        public byte[] SaveState() { Record(nameof(SaveState)); return [7, 8, 9]; }
        public void LoadState(ReadOnlySpan<byte> state) { Record(nameof(LoadState)); State = state.ToArray(); }
        public void SetOption(string key, string value) { Record(nameof(SetOption)); Option = (key, value); }
        public void SelectDisk(int index) { Record(nameof(SelectDisk)); Disk = index; }
    }
}

