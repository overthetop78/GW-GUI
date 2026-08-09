using System.Diagnostics;
using System.IO;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Conversion;
using GWGUI.Scp.Images;

namespace GWGUI.App.Services;

public sealed class ConversionBatchExecutor(
    IGreaseweazleRunner runner,
    AppleRwts18ConversionService? appleRwts18 = null)
{
    private readonly AppleRwts18ConversionService _appleRwts18 = appleRwts18 ?? new();

    public static bool IsInternal(ConversionOutput output) =>
        AppleRwts18ConversionService.CanCreate(output.FormatId, output.Extension);

    public async Task<GwBatchExecutionResult> RunAsync(
        string sourcePath,
        IReadOnlyList<(ConversionOutput Output, GwCommand Command)> items,
        IProgress<GwOutputLine>? progress = null,
        Action<GwBatchItem>? itemStarting = null,
        CancellationToken cancellationToken = default)
    {
        var completed = new List<GwBatchItemResult>(items.Count);
        foreach (var (output, command) in items)
        {
            if (cancellationToken.IsCancellationRequested) break;
            var item = new GwBatchItem(Path.GetFileName(output.OutputPath), command);
            itemStarting?.Invoke(item);
            if (!IsInternal(output))
            {
                completed.Add(new(item, await runner.RunAsync(command, progress, cancellationToken).ConfigureAwait(false)));
                continue;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _appleRwts18.ConvertAsync(sourcePath, output.OutputPath, cancellationToken).ConfigureAwait(false);
                completed.Add(new(item, new(0, false, stopwatch.Elapsed, [])));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                completed.Add(new(item, new(-1, true, stopwatch.Elapsed, [])));
                break;
            }
            catch (Exception exception)
            {
                ErrorLog.Write(exception, "Converting an Apple II RWTS18 image");
                completed.Add(new(item, new(1, false, stopwatch.Elapsed, [])));
            }
        }
        return new(completed, cancellationToken.IsCancellationRequested || completed.Any(result => result.Result.WasCancelled));
    }
}
