using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Constants;

/// <summary>Named byte signatures used by the content-recognition tables.</summary>
internal static class MediaContentSignatures
{
    public static MediaContentSignature AmigaHunkExecutable { get; } = new(0, MediaContentSearchDirection.Start, [0x00, 0x00, 0x03, 0xF3]);
    public static MediaContentSignature DosMzExecutable { get; } = new(0, MediaContentSearchDirection.Start, [0x4D, 0x5A]);
    public static MediaContentSignature AtariTosExecutable { get; } = new(0, MediaContentSearchDirection.Start, [0x60, 0x1A]);
    public static MediaContentSignature IffForm { get; } = new(0, MediaContentSearchDirection.Start, [0x46, 0x4F, 0x52, 0x4D]);
    public static MediaContentSignature IffIlbm { get; } = new(8, MediaContentSearchDirection.Start, [0x49, 0x4C, 0x42, 0x4D]);
    public static MediaContentSignature Iff8Svx { get; } = new(8, MediaContentSearchDirection.Start, [0x38, 0x53, 0x56, 0x58]);
    public static MediaContentSignature AnticMusicProcessor { get; } = new(0, MediaContentSearchDirection.Start, [0x41, 0x4D, 0x31]);
}
