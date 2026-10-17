using System.IO;
using System.IO.MemoryMappedFiles;
using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Services;

internal class EmulationSharedVideoWriter(string namePrefix, Func<int, int> capacity, string separator, string nameFormat) : IDisposable
{
    private MemoryMappedFile? _memory;

    internal MemoryMappedViewAccessor? View { get; private set; }
    internal string? Name { get; private set; }
    internal int SlotCapacity { get; private set; }

    internal void EnsureCapacity(int frameLength)
    {
        var required = capacity(frameLength);
        if (SlotCapacity == required && View is not null) return;
        Dispose();
        Name = namePrefix + separator + Guid.NewGuid().ToString(nameFormat);
        SlotCapacity = required;
        _memory = MemoryMappedFile.CreateNew(Name, checked((long)SlotCapacity * EmulationHostProtocolConstants.VideoSlotCount), MemoryMappedFileAccess.ReadWrite);
        var completed = false;
        try
        {
            View = _memory.CreateViewAccessor(0, checked((long)SlotCapacity * EmulationHostProtocolConstants.VideoSlotCount), MemoryMappedFileAccess.ReadWrite);
            completed = true;
        }
        finally { if (!completed) Dispose(); }
    }

    public void Dispose()
    {
        var view = View;
        var memory = _memory;
        View = null;
        _memory = null;
        Name = null;
        SlotCapacity = default;
        try { view?.Dispose(); }
        finally { memory?.Dispose(); }
    }
}
