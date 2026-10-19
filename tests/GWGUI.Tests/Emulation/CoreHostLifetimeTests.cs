using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreHostLifetimeTests
{
    [Fact]
    public void MissingPreviousCoreNeedsNoDisposal()
    {
        Core? slot = null;
        EmulationHostProtocolFunctions.DisposePreviousCore(ref slot);
        Assert.Null(slot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReinitializationDetachesBeforeDisposalAndClearsTheReentrantSlot(bool fail)
    {
        var replacement = new Core();
        var error = new InvalidOperationException("disposal failure");
        Core? slot = new Core();
        var previous = slot;
        previous.OnDispose = () =>
        {
            Assert.Null(slot);
            slot = replacement;
            if (fail) throw error;
        };

        if (fail)
            Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
                EmulationHostProtocolFunctions.DisposePreviousCore(ref slot)));
        else
            EmulationHostProtocolFunctions.DisposePreviousCore(ref slot);

        Assert.Null(slot);
        Assert.Equal(1, previous.DisposeCount);
        Assert.Equal(0, replacement.DisposeCount);
    }

    private sealed class Core : IDisposable
    {
        internal Action? OnDispose { get; set; }
        internal int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            OnDispose?.Invoke();
        }
    }
}
