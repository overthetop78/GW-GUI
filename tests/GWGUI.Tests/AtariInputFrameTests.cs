using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

public sealed class AtariInputFrameTests
{
    [Fact]
    public void Poll_FreezesCompleteImmutableSnapshotUntilNextPoll()
    {
        var keys = new HashSet<EmulationKey> { EmulationKey.A };
        var controllers = new List<EmulationControllerState> { Controller(AtariInputFrameTestConstants.FirstButtons) };
        var store = new AtariInputFrameStore();
        store.Update(new EmulationInputSnapshot(keys, Pointer(), controllers));
        store.Poll();
        keys.Clear();
        controllers[AtariInputFrameTestConstants.FirstControllerIndex] =
            Controller(AtariInputFrameTestConstants.SecondButtons);
        store.Update(Snapshot(AtariInputFrameTestConstants.SecondButtons));

        Assert.Equal(AtariInputConstants.ActiveState, store.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.FirstButtonId));
        Assert.Equal(AtariInputConstants.InactiveState, store.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.SecondButtonId));

        store.Poll();
        Assert.Equal(AtariInputConstants.ActiveState, store.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.SecondButtonId));
    }

    [Fact]
    public async Task ConcurrentMidFrameUpdate_IsVisibleOnlyAfterFollowingPoll()
    {
        var store = new AtariInputFrameStore();
        store.Update(Snapshot(AtariInputFrameTestConstants.FirstButtons));
        store.Poll();

        await Task.Run(() => store.Update(Snapshot(AtariInputFrameTestConstants.SecondButtons)));

        Assert.Equal(AtariInputConstants.ActiveState, store.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.FirstButtonId));
        store.Poll();
        Assert.Equal(AtariInputConstants.ActiveState, store.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.SecondButtonId));
    }

    [Fact]
    public void State_MapsJoypadMaskButtonsAndAnalogAxesAndRejectsUnknownCoordinates()
    {
        var store = new AtariInputFrameStore();
        store.Update(Snapshot(AtariInputFrameTestConstants.FirstButtons));
        store.Poll();

        Assert.Equal((short)AtariInputFrameTestConstants.FirstButtons,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputConstants.JoypadDevice,
                AtariInputConstants.LeftAnalogIndex, AtariInputConstants.JoypadMaskId));
        Assert.Equal(AtariInputFrameTestConstants.LeftX,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputConstants.AnalogDevice,
                AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogXId));
        Assert.Equal(AtariInputFrameTestConstants.LeftY,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputConstants.AnalogDevice,
                AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogYId));
        Assert.Equal(AtariInputFrameTestConstants.RightX,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputConstants.AnalogDevice,
                AtariInputConstants.RightAnalogIndex, AtariInputConstants.AnalogXId));
        Assert.Equal(AtariInputFrameTestConstants.RightY,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputConstants.AnalogDevice,
                AtariInputConstants.RightAnalogIndex, AtariInputConstants.AnalogYId));
        Assert.Equal(AtariInputConstants.InactiveState,
            store.State(AtariInputFrameTestConstants.UnknownPort, AtariInputConstants.JoypadDevice,
                AtariInputConstants.LeftAnalogIndex, AtariInputFrameTestConstants.FirstButtonId));
        Assert.Equal(AtariInputConstants.InactiveState,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputFrameTestConstants.UnknownDevice,
                AtariInputConstants.LeftAnalogIndex, AtariInputFrameTestConstants.FirstButtonId));
        Assert.Equal(AtariInputConstants.InactiveState,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputConstants.AnalogDevice,
                AtariInputFrameTestConstants.UnknownIndex, AtariInputConstants.AnalogXId));
        Assert.Equal(AtariInputConstants.InactiveState,
            store.State(AtariInputFrameTestConstants.FirstPort, AtariInputConstants.AnalogDevice,
                AtariInputConstants.LeftAnalogIndex, AtariInputFrameTestConstants.UnknownId));
    }

    [Fact]
    public void HostProtocol_RoundTripsTheExactFrozenSnapshot()
    {
        var original = Snapshot(AtariInputFrameTestConstants.FirstButtons);
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            AtariCoreHostFunctions.WriteInput(writer, original);
        stream.Position = AtariInputFrameTestConstants.FirstStreamPosition;
        using var reader = new BinaryReader(stream);

        var restored = AtariCoreHostFunctions.ReadInput(reader);

        Assert.Equal(original.Keys, restored.Keys);
        Assert.Equal(original.Pointer, restored.Pointer);
        Assert.Equal(original.Controllers, restored.Controllers);
    }

    [Fact]
    public void TwoFrameStores_AreCompletelyIndependent()
    {
        var first = new AtariInputFrameStore();
        var second = new AtariInputFrameStore();
        first.Update(Snapshot(AtariInputFrameTestConstants.FirstButtons));
        second.Update(Snapshot(AtariInputFrameTestConstants.SecondButtons));
        first.Poll();
        second.Poll();

        Assert.Equal(AtariInputConstants.ActiveState, first.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.FirstButtonId));
        Assert.Equal(AtariInputConstants.InactiveState, second.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.FirstButtonId));
        Assert.Equal(AtariInputConstants.ActiveState, second.State(AtariInputFrameTestConstants.FirstPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariInputFrameTestConstants.SecondButtonId));
    }

    private static EmulationInputSnapshot Snapshot(uint buttons) => new(
        new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.LeftShift }, Pointer(), [Controller(buttons)]);

    private static EmulationPointerState Pointer() => new(AtariInputFrameTestConstants.PointerX,
        AtariInputFrameTestConstants.PointerY, AtariInputFrameTestConstants.PointerWheel, true, false, true);

    private static EmulationControllerState Controller(uint buttons) => new(buttons,
        AtariInputFrameTestConstants.LeftX, AtariInputFrameTestConstants.LeftY,
        AtariInputFrameTestConstants.RightX, AtariInputFrameTestConstants.RightY,
        AtariInputFrameTestConstants.LeftTrigger, AtariInputFrameTestConstants.RightTrigger);
}

internal static class AtariInputFrameTestConstants
{
    internal const int FirstControllerIndex = 0;
    internal const uint FirstPort = 0;
    internal const uint FirstButtonId = 0;
    internal const uint SecondButtonId = 1;
    internal const uint FirstButtons = 1u << (int)FirstButtonId;
    internal const uint SecondButtons = 1u << (int)SecondButtonId;
    internal const short LeftX = 101;
    internal const short LeftY = -102;
    internal const short RightX = 201;
    internal const short RightY = -202;
    internal const short LeftTrigger = 301;
    internal const short RightTrigger = 302;
    internal const int PointerX = 11;
    internal const int PointerY = -12;
    internal const int PointerWheel = 13;
    internal const uint UnknownPort = 99;
    internal const uint UnknownDevice = 99;
    internal const uint UnknownIndex = 99;
    internal const uint UnknownId = 99;
    internal const long FirstStreamPosition = 0;
}
