using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;

namespace GWGUI.Tests;

public sealed class AtariExternalCoreCallbackTests
{
    [Fact]
    public void InstallCallbacks_UsesRequiredOrder()
    {
        var calls = new List<string>();
        var exports = CreateExports(calls);
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-callbacks-{Guid.NewGuid():N}");
        try
        {
            using var callbacks = new AtariExternalHostCallbacks(
                Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "saves"),
                new Dictionary<string, string>());

            AtariCoreFunctions.InstallCallbacks(exports, callbacks);

            Assert.Equal(new[] { "environment", "video", "audio-sample", "audio-batch", "input-poll", "input-state" }, calls);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Environment_CapturesStandardDiskControlAndReportsInterfaceVersion()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-disk-callback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        ExternalCoreApi.SetEjectState setEject = _ => true;
        ExternalCoreApi.GetEjectState getEject = () => false;
        ExternalCoreApi.GetImageIndex getIndex = () => AtariDiskControlConstants.FirstNativeImageIndex;
        ExternalCoreApi.SetImageIndex setIndex = _ => true;
        ExternalCoreApi.GetImageCount getCount = () => AtariDiskControlConstants.FirstNativeImageIndex;
        ExternalCoreApi.ReplaceImage replace = (_, _) => true;
        ExternalCoreApi.AddImage add = () => true;
        var native = new ExternalCoreApi.DiskControl
        {
            SetEjectState = Marshal.GetFunctionPointerForDelegate(setEject),
            GetEjectState = Marshal.GetFunctionPointerForDelegate(getEject),
            GetImageIndex = Marshal.GetFunctionPointerForDelegate(getIndex),
            SetImageIndex = Marshal.GetFunctionPointerForDelegate(setIndex),
            GetImageCount = Marshal.GetFunctionPointerForDelegate(getCount),
            ReplaceImage = Marshal.GetFunctionPointerForDelegate(replace),
            AddImage = Marshal.GetFunctionPointerForDelegate(add)
        };
        var controlPointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.DiskControl>());
        var versionPointer = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            Marshal.StructureToPtr(native, controlPointer, false);
            using var callbacks = new AtariExternalHostCallbacks(
                Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "saves"),
                new Dictionary<string, string>());

            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetDiskControl, controlPointer));
            Assert.True(callbacks.DiskControl.IsAvailable);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetDiskControlVersion, versionPointer));
            Assert.Equal(AtariDiskControlConstants.InterfaceVersion, Marshal.ReadInt32(versionPointer));
        }
        finally
        {
            Marshal.FreeHGlobal(versionPointer);
            Marshal.FreeHGlobal(controlPointer);
            Directory.Delete(root, recursive: true);
            GC.KeepAlive((setEject, getEject, getIndex, setIndex, getCount, replace, add));
        }
    }

    private static AtariExternalCoreExports CreateExports(ICollection<string> calls) => new(
        _ => calls.Add("environment"),
        _ => calls.Add("video"),
        _ => calls.Add("audio-sample"),
        _ => calls.Add("audio-batch"),
        _ => calls.Add("input-poll"),
        _ => calls.Add("input-state"),
        () => { }, () => { },
        (out ExternalCoreApi.SystemInfo info) => info = default,
        (out ExternalCoreApi.SystemAvInfo info) => info = default,
        (_, _) => { }, () => { }, () => { }, _ => true, () => { }, () => default,
        _ => default, _ => default, () => default, (_, _) => true, (_, _) => true);
}
