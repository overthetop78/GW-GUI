using System.IO;
using GWGUI.Emulation;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class ContentFunctions
{
    internal static string Validate(string contentPath, IReadOnlySet<string> supportedExtensions)
    {
        if (!File.Exists(contentPath))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentNotFound,
                ErrorMessages.ContentFileMissing,
                new Dictionary<string, string> { [ErrorContextConstants.Path] = contentPath });
        var extension = Path.GetExtension(contentPath).TrimStart(MediaConstants.ExtensionPrefix);
        if (!supportedExtensions.Contains(extension))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                ErrorMessages.ContentExtensionUnsupported,
                new Dictionary<string, string>
                {
                    [ErrorContextConstants.Extension] = extension,
                    [ErrorContextConstants.SupportedExtensions] = string.Join(MediaConstants.ExtensionListSeparator, supportedExtensions.Order(StringComparer.OrdinalIgnoreCase))
                });
        return Path.GetFullPath(contentPath);
    }

    internal static LoadedContent Create(string contentPath, bool needsFullPath,
        IReadOnlySet<string> supportedExtensions, bool useWindowsAnsiPath = false)
    {
        var absolutePath = Validate(contentPath, supportedExtensions);
        var path = new ContentPath(absolutePath, useWindowsAnsiPath);
        var data = nint.Zero;
        var gameInfo = nint.Zero;
        var completed = false;
        try
        {
            nuint size = nuint.Zero;
            if (!needsFullPath)
            {
                var bytes = File.ReadAllBytes(absolutePath);
                data = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, data, bytes.Length);
                size = (nuint)bytes.Length;
            }
            gameInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
            {
                Path = path.Pointer,
                Data = data,
                Size = size,
                Metadata = nint.Zero
            }, gameInfo, false);
            var result = new LoadedContent(gameInfo, path, data);
            completed = true;
            return result;
        }
        finally
        {
            if (!completed)
            {
                try
                {
                    if (gameInfo != nint.Zero) Marshal.FreeHGlobal(gameInfo);
                }
                finally
                {
                    try { if (data != nint.Zero) Marshal.FreeHGlobal(data); }
                    finally { path.Dispose(); }
                }
            }
        }
    }
}
