using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class AtomicInputFrameStoreTests
{
    [Fact]
    public void UpdateCopiesBorrowedCollectionsAndPollKeepsAbsoluteState()
    {
        var keys = new HashSet<EmulationKey> { EmulationKey.A };
        var controllers = new[] { EmulationControllerState.Empty with { Buttons = 1 } };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = keys, Controllers = controllers,
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true, DeltaX = 3, DeltaY = -4, Wheel = 2, HorizontalWheel = -2 }
        };
        var store = new EmulationInputFrameStore();
        store.Update(snapshot);
        keys.Clear();
        controllers[0] = EmulationControllerState.Empty;
        store.Poll();
        var first = store.Polled;
        Assert.Contains(EmulationKey.A, first.Keys);
        Assert.Equal(1u, first.Controllers[0].Buttons);
        Assert.NotSame(snapshot.Pointer, first.Pointer);
        Assert.Equal((3, -4, 2, -2), Relative(first));
        store.Poll();
        Assert.Equal((0, 0, 0, 0), Relative(store.Polled));
        Assert.True(store.Polled.Pointer.Left);
        Assert.Contains(EmulationKey.A, store.Polled.Keys);
        Assert.Equal(1u, store.Polled.Controllers[0].Buttons);
    }

    [Fact]
    public void AccumulationUsesTheLatestAbsoluteStateAndAllFourRelativeComponents()
    {
        var store = new EmulationInputFrameStore();
        store.Update(EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true, DeltaX = int.MaxValue, DeltaY = int.MinValue, Wheel = 2, HorizontalWheel = -2 }
        });
        store.Update(EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.B },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Right = true, DeltaX = 1, DeltaY = -1, Wheel = -1, HorizontalWheel = 1 }
        });
        store.Poll();
        Assert.Equal((int.MaxValue, int.MinValue, 1, -1), Relative(store.Polled));
        Assert.DoesNotContain(EmulationKey.A, store.Polled.Keys);
        Assert.Contains(EmulationKey.B, store.Polled.Keys);
        Assert.False(store.Polled.Pointer.Left);
        Assert.True(store.Polled.Pointer.Right);
    }

    [Fact]
    public void BandaiKeepsReplacementAndDoesNotConsumeRelativeComponents()
    {
        var store = new GWGUI.Emulation.Bandai.Common.Services.InputFrameStore();
        store.Update(EmulationInputSnapshot.Empty with { Pointer = EmulationInputSnapshot.Empty.Pointer with { DeltaX = 3, Wheel = 2 } });
        store.Update(EmulationInputSnapshot.Empty with { Pointer = EmulationInputSnapshot.Empty.Pointer with { DeltaX = 5, Wheel = 4 } });
        store.Poll();
        Assert.Equal((5, 0, 4, 0), Relative(store.Polled));
        store.Poll();
        Assert.Equal((5, 0, 4, 0), Relative(store.Polled));
    }

    [Fact]
    public void NativeAppleAndAtariDecodersReadTheSharedPolledSnapshot()
    {
        var apple = new GWGUI.Emulation.Apple.Emulators.Common.Interop.Services.InputFrameStore();
        var atari = new GWGUI.Emulation.Atari.Common.Services.InputFrameStore();
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [EmulationControllerState.Empty with { Buttons = 1 }] };
        apple.Update(snapshot);
        atari.Update(snapshot);
        Assert.Equal((short)0, apple.State(0, 1, 0, 0));
        Assert.Equal((short)0, atari.State(0, 1, 0, 0));
        apple.Poll();
        atari.Poll();
        Assert.Equal((short)1, apple.State(0, 1, 0, 0));
        Assert.Equal((short)1, atari.State(0, 1, 0, 0));
    }

    [Theory]
    [InlineData(int.MaxValue, 1, int.MaxValue)]
    [InlineData(int.MinValue, -1, int.MinValue)]
    [InlineData(int.MaxValue, int.MinValue, -1)]
    [InlineData(-7, 3, -4)]
    public void SharedAdditionKeepsSignedSaturation(int left, int right, int expected)
        => Assert.Equal(expected, EmulationSdk.InputProcessing.SaturatingAdd(left, right));

    [Fact]
    public async Task ConcurrentUpdatesAndPollingDoNotLoseRelativeMovement()
    {
        var store = new EmulationInputFrameStore();
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Pointer = EmulationInputSnapshot.Empty.Pointer with { DeltaX = 1, DeltaY = -1, Wheel = 1, HorizontalWheel = -1 }
        };
        var producers = new List<Task>();
        var x = 0;
        var y = 0;
        var wheel = 0;
        var horizontal = 0;
        void Poll()
        {
            store.Poll();
            var pointer = store.Polled.Pointer;
            x += pointer.DeltaX;
            y += pointer.DeltaY;
            wheel += pointer.Wheel;
            horizontal += pointer.HorizontalWheel;
        }
        try
        {
            for (var producer = 0; producer < 4; producer++)
                producers.Add(Task.Run(() => { for (var update = 0; update < 500; update++) store.Update(snapshot); }));
            var completion = Task.WhenAll(producers);
            while (!completion.IsCompleted) { Poll(); await Task.Yield(); }
            await completion;
            Poll();
            Assert.Equal((2000, -2000, 2000, -2000), (x, y, wheel, horizontal));
            Poll();
            Assert.Equal((2000, -2000, 2000, -2000), (x, y, wheel, horizontal));
        }
        finally { await Task.WhenAll(producers); }
    }

    private static (int, int, int, int) Relative(EmulationInputSnapshot snapshot) =>
        (snapshot.Pointer.DeltaX, snapshot.Pointer.DeltaY, snapshot.Pointer.Wheel, snapshot.Pointer.HorizontalWheel);
}
