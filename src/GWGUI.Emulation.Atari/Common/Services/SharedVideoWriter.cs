using System.IO.MemoryMappedFiles;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class SharedVideoWriter : IDisposable
{
    private readonly string _namePrefix;
    private MemoryMappedFile? _memory;

    internal SharedVideoWriter(string namePrefix) => _namePrefix = namePrefix;

    internal MemoryMappedViewAccessor? View { get; private set; }
    internal string? Name { get; private set; }
    internal int SlotCapacity { get; private set; }

    internal void EnsureCapacity(int frameLength)
    {
        var required = CoreHostFunctions.CalculateVideoSlotCapacity(frameLength);
        if (SlotCapacity == required && View is not null) return;
        View?.Dispose();
        _memory?.Dispose();
        Name = _namePrefix + CoreHostConstants.VideoMapGenerationSeparator
            + Guid.NewGuid().ToString(CoreHostConstants.UniqueNameFormat);
        SlotCapacity = required;
        _memory = MemoryMappedFile.CreateNew(Name,
            checked((long)SlotCapacity * EmulationHostProtocolConstants.VideoSlotCount),
            MemoryMappedFileAccess.ReadWrite);
        View = _memory.CreateViewAccessor(BufferConstants.FirstBufferIndex,
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
