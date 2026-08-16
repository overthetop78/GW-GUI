using System.IO.MemoryMappedFiles;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariSharedVideoWriter : IDisposable
{
    private readonly string _namePrefix;
    private MemoryMappedFile? _memory;

    internal AtariSharedVideoWriter(string namePrefix) => _namePrefix = namePrefix;

    internal MemoryMappedViewAccessor? View { get; private set; }
    internal string? Name { get; private set; }
    internal int SlotCapacity { get; private set; }

    internal void EnsureCapacity(int frameLength)
    {
        var required = AtariCoreHostFunctions.CalculateVideoSlotCapacity(frameLength);
        if (SlotCapacity == required && View is not null) return;
        View?.Dispose();
        _memory?.Dispose();
        Name = _namePrefix + AtariCoreHostConstants.VideoMapGenerationSeparator
            + Guid.NewGuid().ToString(AtariCoreHostConstants.UniqueNameFormat);
        SlotCapacity = required;
        _memory = MemoryMappedFile.CreateNew(Name,
            checked((long)SlotCapacity * EmulationHostProtocolConstants.VideoSlotCount),
            MemoryMappedFileAccess.ReadWrite);
        View = _memory.CreateViewAccessor(AtariConstants.FirstBufferIndex,
            checked((long)SlotCapacity * EmulationHostProtocolConstants.VideoSlotCount),
            MemoryMappedFileAccess.ReadWrite);
    }

    public void Dispose()
    {
        View?.Dispose();
        View = null;
        _memory?.Dispose();
        _memory = null;
        Name = null;
        SlotCapacity = default;
    }
}
