using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Tests;

public sealed class AmigaInputAccumulatorTests
{
    [Fact]
    public void Consume_PreservesTransientPointerInputUntilOneFrameReadsIt()
    {
        var accumulator = new AmigaInputAccumulator();
        accumulator.Update(Snapshot(12, -4, 1, left: true));
        accumulator.Update(Snapshot(-3, 2, -2, left: true));

        var first = accumulator.Consume();
        var second = accumulator.Consume();

        Assert.Equal(new EmulationPointerState(9, -2, -1, true, false, false), first.Pointer);
        Assert.Equal(new EmulationPointerState(0, 0, 0, true, false, false), second.Pointer);
    }

    [Fact]
    public void Update_ReplacesPersistentKeyboardAndControllerState()
    {
        var accumulator = new AmigaInputAccumulator();
        accumulator.Update(Snapshot(0, 0, 0, keys: new HashSet<EmulationKey> { EmulationKey.A }));
        accumulator.Update(Snapshot(0, 0, 0, keys: new HashSet<EmulationKey> { EmulationKey.B }));

        var result = accumulator.Consume();

        Assert.DoesNotContain(EmulationKey.A, result.Keys);
        Assert.Contains(EmulationKey.B, result.Keys);
    }

    [Fact]
    public void Update_SaturatesPointerDeltasInsteadOfOverflowing()
    {
        var accumulator = new AmigaInputAccumulator();
        accumulator.Update(Snapshot(int.MaxValue, int.MinValue, int.MaxValue));
        accumulator.Update(Snapshot(1, -1, 1));

        var result = accumulator.Consume();

        Assert.Equal(int.MaxValue, result.Pointer.DeltaX);
        Assert.Equal(int.MinValue, result.Pointer.DeltaY);
        Assert.Equal(int.MaxValue, result.Pointer.Wheel);
    }

    private static EmulationInputSnapshot Snapshot(int x, int y, int wheel, bool left = false,
        IReadOnlySet<EmulationKey>? keys = null) => new(
        keys ?? new HashSet<EmulationKey>(),
        new EmulationPointerState(x, y, wheel, left, false, false),
        [EmulationControllerState.Empty, EmulationControllerState.Empty,
         EmulationControllerState.Empty, EmulationControllerState.Empty]);
}
