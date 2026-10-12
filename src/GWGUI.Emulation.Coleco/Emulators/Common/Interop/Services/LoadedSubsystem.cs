using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services;

// Owns the contiguous native descriptors and their backing content until unload.
internal sealed class LoadedSubsystem : IDisposable
{
    private readonly LoadedContent?[] _contents;
    internal nint GameInfos { get; private set; }
    internal nuint Count => (nuint)_contents.Length;

    private LoadedSubsystem(LoadedContent?[] contents, nint gameInfos)
    {
        _contents = contents;
        GameInfos = gameInfos;
    }

    internal static LoadedSubsystem Create(IReadOnlyList<string?> paths, bool needsFullPath, IReadOnlySet<string> supportedExtensions)
    {
        var contents = new LoadedContent?[paths.Count];
        var stride = Marshal.SizeOf<ExternalCoreApi.GameInfo>();
        var gameInfos = nint.Zero;
        var completed = false;
        try
        {
            gameInfos = Marshal.AllocHGlobal(checked(stride * paths.Count));
            for (var index = 0; index < paths.Count; index++)
            {
                var info = default(ExternalCoreApi.GameInfo);
                if (!string.IsNullOrWhiteSpace(paths[index]))
                {
                    var content = ContentFunctions.Create(paths[index]!, needsFullPath, supportedExtensions);
                    contents[index] = content;
                    info = Marshal.PtrToStructure<ExternalCoreApi.GameInfo>(content.GameInfo);
                }
                Marshal.StructureToPtr(info, gameInfos + checked(index * stride), false);
            }
            var result = new LoadedSubsystem(contents, gameInfos);
            completed = true;
            return result;
        }
        finally
        {
            if (!completed)
            {
                try { if (gameInfos != nint.Zero) Marshal.FreeHGlobal(gameInfos); }
                finally { DisposeContents(contents, 0); }
            }
        }
    }

    public void Dispose()
    {
        var pointer = GameInfos;
        GameInfos = nint.Zero;
        try { if (pointer != nint.Zero) Marshal.FreeHGlobal(pointer); }
        finally { DisposeContents(_contents, 0); }
    }

    private static void DisposeContents(LoadedContent?[] contents, int index)
    {
        if (index == contents.Length) return;
        var content = contents[index];
        contents[index] = null;
        try { content?.Dispose(); }
        finally { DisposeContents(contents, index + 1); }
    }
}
