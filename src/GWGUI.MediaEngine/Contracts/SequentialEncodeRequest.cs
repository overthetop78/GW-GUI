using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes validated blocks and retained source segments to encode into one sequential target format.</summary>
public sealed class SequentialEncodeRequest
{
    public SequentialEncodeRequest(
        string targetFormatId,
        string machineId,
        IReadOnlyList<SequentialDecodedBlock> blocks,
        IReadOnlyList<SequentialMediaSegment>? retainedSegments = null,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(machineId);
        ArgumentNullException.ThrowIfNull(blocks);

        TargetFormatId = targetFormatId;
        MachineId = machineId;
        Blocks = new ReadOnlyCollection<SequentialDecodedBlock>(blocks.ToArray());
        RetainedSegments = new ReadOnlyCollection<SequentialMediaSegment>((retainedSegments ?? []).ToArray());
        Parameters = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(parameters ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public string TargetFormatId { get; }
    public string MachineId { get; }
    public IReadOnlyList<SequentialDecodedBlock> Blocks { get; }
    public IReadOnlyList<SequentialMediaSegment> RetainedSegments { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }
}
