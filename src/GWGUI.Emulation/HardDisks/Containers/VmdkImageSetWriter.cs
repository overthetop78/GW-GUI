using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

public enum VmdkImageSetKind { VmfsFlat, VmfsSparse, SplitFlat, SplitSparse, MonolithicFlat }

/// <summary>Builds a VMDK descriptor and its extent as a set of named streams.</summary>
public static class VmdkImageSetWriter
{
    /// <summary>The callback consumes each stream synchronously. Streams are disposed after the callback returns.</summary>
    public static void Write(long capacity,string baseName,VmdkImageSetKind kind,Action<string,Stream> emit,
        Action<Stream>? initialize=null,DiscUtils.Vmdk.DiskAdapterType adapter=DiscUtils.Vmdk.DiskAdapterType.LsiLogicScsi)
    {
        if (kind == VmdkImageSetKind.MonolithicFlat)
        {
            VmdkFlatImageSetWriter.Write(capacity, baseName, emit, initialize, adapter);
            return;
        }
        if (kind is VmdkImageSetKind.SplitFlat or VmdkImageSetKind.SplitSparse)
        {
            VmdkSplitImageWriter.Write(capacity, baseName, kind == VmdkImageSetKind.SplitSparse, emit, initialize, adapter);
            return;
        }
        ArgumentNullException.ThrowIfNull(emit);
        if(capacity<512 || capacity%512!=0 || capacity>1L<<40)throw new ArgumentOutOfRangeException(nameof(capacity));
        if(string.IsNullOrWhiteSpace(baseName) || baseName.Length>100 ||
            baseName.Any(c=>!(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            throw new ArgumentException("The image set name must contain only ASCII letters, digits, '-' or '_'.",nameof(baseName));
        if(!Enum.IsDefined(kind) || !Enum.IsDefined(adapter))throw new ArgumentOutOfRangeException(nameof(kind));
        using var content=SparseImageContent.Create(capacity,initialize);
        var builder=new DiscUtils.Vmdk.DiskBuilder
        {
            Content=content,AdapterType=adapter,
            DiskType=kind==VmdkImageSetKind.VmfsFlat?DiscUtils.Vmdk.DiskCreateType.Vmfs:DiscUtils.Vmdk.DiskCreateType.VmfsSparse
        };
        // Extents precede the descriptor so a publisher can expose the descriptor last.
        var files=builder.Build(baseName).ToArray();
        foreach(var file in files.Skip(1).Concat(files.Take(1)))
        {
            using var source=file.OpenStream();
            emit(file.Name,source);
        }
    }
}
