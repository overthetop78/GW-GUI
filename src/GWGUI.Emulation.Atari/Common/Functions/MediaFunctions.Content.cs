using GWGUI.Emulation;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class ContentFunctions
{
    internal static string Validate(string contentPath, IReadOnlySet<string> supportedExtensions)
    {
        if (!File.Exists(contentPath))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentNotFound,
                ErrorMessages.ContentFileMissing,
                new Dictionary<string, string> { [CommonConstants.PathContextKey] = contentPath });
        var extension = Path.GetExtension(contentPath).TrimStart(CommonConstants.ExtensionPrefix);
        if (!supportedExtensions.Contains(extension))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                ErrorMessages.ContentExtensionUnsupported,
                new Dictionary<string, string>
                {
                    [CommonConstants.ExtensionContextKey] = extension,
                    [CommonConstants.SupportedExtensionsContextKey] = string.Join(
                        ContentConstants.ExtensionSeparator, supportedExtensions.Order(StringComparer.OrdinalIgnoreCase))
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
        try
        {
            nuint size = nuint.Zero;
            if (!needsFullPath)
            {
                var bytes = File.ReadAllBytes(absolutePath);
                data = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, CommonConstants.FirstBufferIndex, data, bytes.Length);
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
            return new LoadedContent(gameInfo, path, data);
        }
        catch
        {
            if (gameInfo != nint.Zero) Marshal.FreeHGlobal(gameInfo);
            if (data != nint.Zero) Marshal.FreeHGlobal(data);
            path.Dispose();
            throw;
        }
    }
}
