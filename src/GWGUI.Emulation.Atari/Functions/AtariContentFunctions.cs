using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariContentFunctions
{
    internal static string Validate(string contentPath, IReadOnlySet<string> supportedExtensions)
    {
        if (!File.Exists(contentPath))
            throw new AtariEmulationException(AtariErrorCategory.Content, AtariErrorCode.ContentNotFound,
                AtariErrorMessages.ContentFileMissing,
                new Dictionary<string, string> { [AtariConstants.PathContextKey] = contentPath });
        var extension = Path.GetExtension(contentPath).TrimStart(AtariConstants.ExtensionPrefix);
        if (!supportedExtensions.Contains(extension))
            throw new AtariEmulationException(AtariErrorCategory.Content, AtariErrorCode.ContentUnsupported,
                AtariErrorMessages.ContentExtensionUnsupported,
                new Dictionary<string, string>
                {
                    [AtariConstants.ExtensionContextKey] = extension,
                    [AtariConstants.SupportedExtensionsContextKey] = string.Join(
                        AtariContentConstants.ExtensionSeparator, supportedExtensions.Order(StringComparer.OrdinalIgnoreCase))
                });
        return Path.GetFullPath(contentPath);
    }

    internal static AtariLoadedContent Create(string contentPath, bool needsFullPath,
        IReadOnlySet<string> supportedExtensions)
    {
        var absolutePath = Validate(contentPath, supportedExtensions);
        var path = new ExternalCoreUtf8String(absolutePath);
        var data = nint.Zero;
        var gameInfo = nint.Zero;
        try
        {
            nuint size = nuint.Zero;
            if (!needsFullPath)
            {
                var bytes = File.ReadAllBytes(absolutePath);
                data = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, AtariConstants.FirstBufferIndex, data, bytes.Length);
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
            return new AtariLoadedContent(gameInfo, path, data);
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
