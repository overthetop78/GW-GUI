using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private readonly List<nint> _contentBuffers = [];
    private readonly List<ExternalCoreUtf8String> _contentPaths = [];

    private ExternalCoreUtf8String AllocateContentPath(string path)
    {
        var native = new ExternalCoreUtf8String(path);
        var owned = false;
        try { _contentPaths.Add(native); owned = true; return native; }
        finally { if (!owned) native.Dispose(); }
    }

    private nint AllocateContent(byte[] bytes)
    {
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        var owned = false;
        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            _contentBuffers.Add(pointer);
            owned = true;
            return pointer;
        }
        finally { if (!owned) Marshal.FreeHGlobal(pointer); }
    }

    private bool LoadSubsystem(CoreContentLoadPlan plan, bool needFullPath)
    {
        var size = Marshal.SizeOf<ExternalCoreApi.GameInfo>();
        var games = Marshal.AllocHGlobal(checked(size * plan.Paths.Count));
        try
        {
            for (var index = 0; index < plan.Paths.Count; index++)
            {
                var path = Path.GetFullPath(plan.Paths[index]);
                var native = AllocateContentPath(path);
                var content = needFullPath ? null : File.ReadAllBytes(path);
                Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
                {
                    Path = native.Pointer,
                    Data = content is null ? nint.Zero : AllocateContent(content),
                    Size = (nuint)(content?.Length ?? 0)
                }, games + checked(index * size), false);
            }
            return Export<ExternalCoreApi.LoadGameSpecial>(ExternalCoreExportNames.LoadGameSpecial)(plan.SubsystemId, games, (nuint)plan.Paths.Count);
        }
        finally { Marshal.FreeHGlobal(games); }
    }

    private void ReleaseContentBuffers()
    {
        var errors = new List<Exception>();
        foreach (var pointer in _contentBuffers)
            try { Marshal.FreeHGlobal(pointer); }
            catch (Exception error) { errors.Add(error); }
        _contentBuffers.Clear();
        foreach (var path in _contentPaths)
            try { path.Dispose(); }
            catch (Exception error) { errors.Add(error); }
        _contentPaths.Clear();
        if (errors.Count > 0) throw new AggregateException(errors);
    }
}
