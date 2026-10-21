using System.IO;
using System.IO.MemoryMappedFiles;
using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Services;

/// <summary>Mémoire vidéo partagée redimensionnable, possédée par l’hôte de cœur qui crée cet écrivain.</summary>
/// <remarks>Le propriétaire appelle Dispose dans son finally après la fin des écritures ; le remplacement de capacité libère aussi la mémoire précédente.</remarks>
internal class EmulationSharedVideoWriter(string namePrefix, Func<int, int> capacity, string separator, string nameFormat) : IEmulationSharedVideoWriter
{
    private MemoryMappedFile? _memory;

    /// <summary>Vue empruntée par les codecs ; sa destruction appartient à cet écrivain.</summary>
    public MemoryMappedViewAccessor? View { get; private set; }
    /// <summary>Nom transmis au lecteur de l’autre processus, ou null après destruction.</summary>
    public string? Name { get; private set; }
    /// <summary>Capacité d’un slot vidéo dans la mémoire courante.</summary>
    public int SlotCapacity { get; private set; }

    /// <summary>Conserve la mémoire de capacité suffisante ou crée son remplacement avec nettoyage garanti en cas d’échec.</summary>
    public void EnsureCapacity(int frameLength)
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
