using GWGUI.App.Services;

namespace GWGUI.Tests;

public sealed class DiskImageCancellationScopeTests
{
    [Fact]
    public void BeginningAnOperationCancelsOnlyThePreviousOperationOfTheSameKind()
    {
        using var scope = new DiskImageCancellationScope();
        var firstExplorer = scope.BeginExplorer();
        var visualization = scope.BeginVisualization();

        var secondExplorer = scope.BeginExplorer();

        Assert.True(firstExplorer.IsCancellationRequested);
        Assert.False(secondExplorer.IsCancellationRequested);
        Assert.False(visualization.IsCancellationRequested);
        Assert.True(scope.IsCurrentExplorer(secondExplorer));
    }

    [Fact]
    public void CancelAllCancelsEveryIndependentOperation()
    {
        using var scope = new DiskImageCancellationScope();
        var explorer = scope.BeginExplorer();
        var visualization = scope.BeginVisualization();
        var scp = scope.BeginScp();
        var inspector = scope.BeginInspector();

        scope.CancelAll();

        Assert.True(explorer.IsCancellationRequested);
        Assert.True(visualization.IsCancellationRequested);
        Assert.True(scp.IsCancellationRequested);
        Assert.True(inspector.IsCancellationRequested);
    }
}
