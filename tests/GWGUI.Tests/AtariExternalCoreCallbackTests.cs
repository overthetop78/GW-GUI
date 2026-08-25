using GWGUI.Emulation.Atari;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AtariExternalCoreCallbackTests
{
    public static TheoryData<AtariMachineModel> CoreProfiles => new()
    {
        AtariMachineModel.St,
        AtariMachineModel.Atari800,
        AtariMachineModel.Atari2600,
        AtariMachineModel.Atari7800,
        AtariMachineModel.Lynx,
        AtariMachineModel.Jaguar
    };

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
                Path.Combine(root, "assets"),
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
                Path.Combine(root, "assets"),
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

    [Theory]
    [MemberData(nameof(CoreProfiles))]
    public void Load_UsesExactPostInitializationOrderForEveryCoreProfile(AtariMachineModel model)
    {
        var calls = new List<string>();
        var exports = CreateExports(calls, recordLifecycle: true);
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-lifecycle-{Guid.NewGuid():N}");
        try
        {
            using var callbacks = new AtariExternalHostCallbacks(
                Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "saves"),
                Path.Combine(root, "assets"), new Dictionary<string, string>());

            AtariCoreLifecycleFunctions.Load(exports, callbacks, new AtariMachineConfiguration(model),
                new nint(AtariCoreLifecycleTestConstants.GameInfoPointer));

            Assert.Equal("load", calls[0]);
            Assert.All(calls.Skip(1).Take(calls.Count - 2), call => Assert.StartsWith("controller-", call));
            Assert.Equal("av-info", calls[^1]);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Cleanup_ReversesOnlySuccessfulNativeStages()
    {
        var calls = new List<string>();
        var exports = CreateExports(calls, recordLifecycle: true);

        AtariCoreLifecycleFunctions.Cleanup(exports, gameLoaded: true, initialized: true,
            () => calls.Add("callbacks-dispose"), () => calls.Add("library-dispose"));

        Assert.Equal(new[] { "unload", "deinitialize", "callbacks-dispose", "library-dispose" }, calls);
    }

    private static AtariExternalCoreExports CreateExports(ICollection<string> calls,
        bool recordLifecycle = false) => new(
        _ => calls.Add("environment"),
        _ => calls.Add("video"),
        _ => calls.Add("audio-sample"),
        _ => calls.Add("audio-batch"),
        _ => calls.Add("input-poll"),
        _ => calls.Add("input-state"),
        () => { }, () => { if (recordLifecycle) calls.Add("deinitialize"); },
        (out ExternalCoreApi.SystemInfo info) => info = default,
        (out ExternalCoreApi.SystemAvInfo info) => { if (recordLifecycle) calls.Add("av-info"); info = default; },
        (port, _) => { if (recordLifecycle) calls.Add($"controller-{port}"); }, () => { }, () => { },
        _ => { if (recordLifecycle) calls.Add("load"); return true; },
        () => { if (recordLifecycle) calls.Add("unload"); }, () => default,
        _ => default, _ => default, () => default, (_, _) => true, (_, _) => true);
}

internal static class AtariCoreLifecycleTestConstants
{
    internal const int GameInfoPointer = 1;
}
