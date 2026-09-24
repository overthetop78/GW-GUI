namespace GWGUI.App.Services.DiskImages;

/// <summary>
/// Coordinates the independent asynchronous work performed by Explorer and Visualizer.
/// Starting a new operation cancels only the older operation of the same kind.
/// </summary>
public sealed class DiskImageCancellationScope : IDisposable
{
    private readonly object _sharedLoadSync = new();
    private CancellationTokenSource? _sharedLoad;
    private CancellationTokenSource? _explorer;
    private CancellationTokenSource? _visualization;
    private CancellationTokenSource? _scp;
    private CancellationTokenSource? _inspector;
    private readonly SemaphoreSlim _visualizationConversionGate = new(1, 1);

    public CancellationTokenSource BeginExplorer() => Replace(ref _explorer);
    public CancellationTokenSource BeginVisualization() => Replace(ref _visualization);
    public CancellationTokenSource BeginScp() => Replace(ref _scp);
    public CancellationTokenSource BeginInspector() => Replace(ref _inspector);

    public CancellationTokenSource BeginSharedLoad()
    {
        lock (_sharedLoadSync)
        {
            if (_sharedLoad is not null)
            {
                throw new InvalidOperationException("The previous shared media load has not finished.");
            }

            return _sharedLoad = new CancellationTokenSource();
        }
    }

    public void CompleteSharedLoad(CancellationTokenSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        lock (_sharedLoadSync)
        {
            if (ReferenceEquals(_sharedLoad, source)) _sharedLoad = null;
        }
        source.Dispose();
    }

    public bool IsCurrentExplorer(CancellationTokenSource source) => ReferenceEquals(_explorer, source);
    public bool IsCurrentVisualization(CancellationTokenSource source) => ReferenceEquals(_visualization, source);
    public bool IsCurrentScp(CancellationTokenSource source) => ReferenceEquals(_scp, source);
    public bool IsCurrentInspector(CancellationTokenSource source) => ReferenceEquals(_inspector, source);

    public Task EnterVisualizationConversionAsync(CancellationToken cancellationToken) =>
        _visualizationConversionGate.WaitAsync(cancellationToken);

    public void ExitVisualizationConversion() => _visualizationConversionGate.Release();

    public void CancelScp() => _scp?.Cancel();
    public void CancelInspector() => _inspector?.Cancel();

    public void CancelAll()
    {
        lock (_sharedLoadSync) _sharedLoad?.Cancel();
        _explorer?.Cancel();
        _visualization?.Cancel();
        _scp?.Cancel();
        _inspector?.Cancel();
    }

    public void Dispose()
    {
        lock (_sharedLoadSync)
        {
            _sharedLoad?.Cancel();
            _sharedLoad?.Dispose();
            _sharedLoad = null;
        }
        CancelAndDispose(ref _explorer);
        CancelAndDispose(ref _visualization);
        CancelAndDispose(ref _scp);
        CancelAndDispose(ref _inspector);
        _visualizationConversionGate.Dispose();
    }

    private static CancellationTokenSource Replace(ref CancellationTokenSource? current)
    {
        CancelAndDispose(ref current);
        return current = new CancellationTokenSource();
    }

    private static void CancelAndDispose(ref CancellationTokenSource? source)
    {
        source?.Cancel();
        source?.Dispose();
        source = null;
    }
}
