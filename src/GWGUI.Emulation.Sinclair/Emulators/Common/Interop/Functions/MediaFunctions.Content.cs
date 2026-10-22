using CommonCoreHostFunctions = GWGUI.Emulation.Sinclair.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Services;
using System.IO;
using GWGUI.Emulation;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;

internal static class ContentFunctions
{
    internal static string Validate(string contentPath, IReadOnlySet<string> supportedExtensions)
    {
        if (!File.Exists(contentPath)) throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentNotFound, ErrorMessages.ContentFileMissing, new Dictionary<string, string> { [ErrorContextConstants.Path] = contentPath });
        var extension = Path.GetExtension(contentPath).TrimStart(MediaConstants.ExtensionPrefix);
        if (!supportedExtensions.Contains(extension)) throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentExtensionUnsupported, new Dictionary<string, string>
                {
                    [ErrorContextConstants.Extension] = extension,
                    [ErrorContextConstants.SupportedExtensions] = string.Join( MediaConstants.ExtensionListSeparator, supportedExtensions.Order(StringComparer.OrdinalIgnoreCase))
                });
        return Path.GetFullPath(contentPath);
    }

    internal static LoadedContent Create(string contentPath, bool needsFullPath, IReadOnlySet<string> supportedExtensions, bool useWindowsAnsiPath = false)
    => CommonCoreHostFunctions.NativeResources.CreateLoadedContent(Validate(contentPath, supportedExtensions), needsFullPath, useWindowsAnsiPath);
}
