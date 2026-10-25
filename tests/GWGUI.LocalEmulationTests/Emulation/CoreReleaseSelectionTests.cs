using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreReleaseSelectionTests
{
    [Fact]
    public void SelectionWithoutPredicateKeepsTheReleaseForAnyEmulator()
    {
        var expected = new Release(42);
        var releases = new Dictionary<string, Release> { ["build"] = expected };

        Assert.Same(expected, EmulationSdk.CoreReleases.SelectRelease(releases, Requested("build")));
    }

    [Fact]
    public void SelectionWithoutPredicateKeepsTheMissingReleaseException()
    {
        var releases = new Dictionary<string, Release>();
        var error = Assert.Throws<ArgumentException>(() =>
            EmulationSdk.CoreReleases.SelectRelease(releases, Requested("missing")));

        Assert.Equal("release", error.Message);
        Assert.Null(error.ParamName);
    }

    [Theory]
    [InlineData("build")]
    [InlineData("BUILD")]
    public void SelectsExactIdentifierAndKeepsTheStoredRelease(string identifier)
    {
        var lower = new Release(1);
        var upper = new Release(2);
        var releases = new Dictionary<string, Release>(StringComparer.Ordinal)
        {
            ["build"] = lower,
            ["BUILD"] = upper
        };
        var expected = identifier == "build" ? lower : upper;
        var predicateCalls = 0;

        var selected = EmulationSdk.CoreReleases.SelectRelease(releases, Requested(identifier), value =>
        {
            predicateCalls++;
            Assert.Same(expected, value);
            return true;
        });

        Assert.Same(expected, selected);
        Assert.Equal(1, predicateCalls);
    }

    [Fact]
    public void MissingIdentifierDoesNotEvaluateEmulatorPredicate()
    {
        var releases = new Dictionary<string, Release>();
        var error = Assert.Throws<ArgumentException>(() =>
            EmulationSdk.CoreReleases.SelectRelease(releases, Requested("missing"),
                _ => throw new InvalidOperationException("Predicate must not run.")));
        Assert.Equal("release", error.Message);
        Assert.Null(error.ParamName);
    }

    [Fact]
    public void RejectsAStoredReleaseForAnotherEmulator()
    {
        var releases = new Dictionary<string, Release> { ["build"] = new(2) };
        var error = Assert.Throws<ArgumentException>(() =>
            EmulationSdk.CoreReleases.SelectRelease(releases, Requested("build"),
                value => value.Emulator == 1));
        Assert.Equal("release", error.Message);
        Assert.Null(error.ParamName);
    }

    [Fact]
    public void KeepsTheOriginalPredicateException()
    {
        var releases = new Dictionary<string, Release> { ["build"] = new(1) };
        var expected = new InvalidOperationException("Emulator lookup failed.");
        var observed = Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.CoreReleases.SelectRelease(releases, Requested("build"),
                _ => throw expected));
        Assert.Same(expected, observed);
    }

    [Fact]
    public void MissingProgressDoesNotCreateAnAdapterOrProjectValues()
    {
        Assert.Null(EmulationSdk.CoreReleases.CreateInstallProgress<Update>(null,
            _ => throw new InvalidOperationException("Projection must not run.")));
    }

    [Theory]
    [InlineData(null, 0d)]
    [InlineData(0d, 0d)]
    [InlineData(0.25d, 0.25d)]
    [InlineData(-0.5d, -0.5d)]
    [InlineData(1.5d, 1.5d)]
    public void ProgressKeepsItsCapturedContextAndUnclampedFraction(double? fraction, double expected)
    {
        var previous = SynchronizationContext.Current;
        var context = new QueuedContext();
        var recorded = new List<double>();
        var projections = 0;
        try
        {
            SynchronizationContext.SetSynchronizationContext(context);
            var adapter = EmulationSdk.CoreReleases.CreateInstallProgress<Update>(
                new RecordingProgress<double>(recorded.Add), value =>
                {
                    projections++;
                    return value.Fraction;
                });
            SynchronizationContext.SetSynchronizationContext(null);
            Assert.NotNull(adapter);

            adapter.Report(new Update(fraction));

            Assert.Empty(recorded);
            Assert.Equal(0, projections);
            Assert.Equal(1, context.Pending);
            context.Drain();
            Assert.Equal(expected, Assert.Single(recorded));
            Assert.Equal(1, projections);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }

    private static EmulationEmulatorRelease Requested(string identifier) => new(identifier, identifier, "1");
    private sealed record Release(int Emulator);
    private sealed record Update(double? Fraction);
    private sealed class RecordingProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private sealed class QueuedContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback Callback, object? State)> pending = new();
        internal int Pending => pending.Count;
        public override void Post(SendOrPostCallback callback, object? state) => pending.Enqueue((callback, state));
        internal void Drain()
        {
            while (pending.TryDequeue(out var work))
                work.Callback(work.State);
        }
    }
}

