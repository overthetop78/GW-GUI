using GWGUI.MediaEngine.Images.Formats.Floppy.Woz;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Services;

/// <summary>Fournit les calculs existants de MediaEngine par leur interface publique.</summary>
internal sealed class MediaDataIntegrityService : IMediaDataIntegrity
{
    /// <inheritdoc/>
    public uint ComputeCrc32(ReadOnlySpan<byte> data) => WozCrc32.Compute(data);
}
