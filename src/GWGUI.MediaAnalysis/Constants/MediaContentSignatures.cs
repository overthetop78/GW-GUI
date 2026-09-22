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
    public static MediaContentSignature AtariBinaryLoadMarker { get; } = new(0, MediaContentSearchDirection.Start, [0xFF, 0xFF]);
    public static MediaContentSignature AtariAbcRuntimeCode { get; } = new(426, MediaContentSearchDirection.Start, [0xCA, 0xCA, 0xCA, 0xA5, 0x84, 0x95, 0x00, 0xA5, 0x85, 0x95, 0x01, 0xA5, 0x86, 0x95, 0x02, 0x60, 0xB5, 0x03, 0x48, 0xB5, 0x04, 0x48, 0xB5, 0x03, 0x15, 0x04, 0xD0, 0x07, 0x68, 0x95, 0x04, 0x68, 0x95, 0x03, 0x60, 0xA1, 0x06, 0x81, 0x00, 0xF6, 0x06, 0xD0, 0x02, 0xF6, 0x07, 0xF6, 0x00, 0xD0, 0x02, 0xF6, 0x01, 0x38, 0xB5, 0x03, 0xE9, 0x01, 0x95, 0x03, 0xB0, 0x02, 0xD6, 0x04, 0x4C, 0xB6]);
    public static MediaContentSignature AtariAbcRuntimeRunVector { get; } = new(5, MediaContentSearchDirection.End, [0xE0, 0x02, 0xE1, 0x02, 0x00, 0x00]);
    public static MediaContentSignature AtariAbcRelocationLoader { get; } = new(0, MediaContentSearchDirection.Start, [0xFF, 0xFF, 0x00, 0x06, 0x9E, 0x06]);
    public static MediaContentSignature AtariAbcRelocationInitVector { get; } = new(165, MediaContentSearchDirection.Start, [0xE2, 0x02, 0xE3, 0x02, 0x00, 0x06]);
    public static MediaContentSignature AtariBasicTokenizedProgram { get; } = new(0, MediaContentSearchDirection.Start, [0x00, 0x00, 0x00, 0x01]);
}
