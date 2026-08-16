using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Common;

namespace GWGUI.Tests;

public sealed class ExternalCoreApiTests
{
    [Fact]
    public void GameInfo_UsesTheExpectedPointerLayout()
    {
        Assert.Equal(nint.Zero, Marshal.OffsetOf<ExternalCoreApi.GameInfo>(nameof(ExternalCoreApi.GameInfo.Path)));
        Assert.Equal((nint)IntPtr.Size, Marshal.OffsetOf<ExternalCoreApi.GameInfo>(nameof(ExternalCoreApi.GameInfo.Data)));
        Assert.Equal((nint)(IntPtr.Size * 2), Marshal.OffsetOf<ExternalCoreApi.GameInfo>(nameof(ExternalCoreApi.GameInfo.Size)));
        Assert.Equal((nint)(IntPtr.Size * 3), Marshal.OffsetOf<ExternalCoreApi.GameInfo>(nameof(ExternalCoreApi.GameInfo.Metadata)));
    }

    [Fact]
    public void GeometryAndTiming_UseSequentialNativeLayout()
    {
        Assert.Equal(LayoutKind.Sequential, typeof(ExternalCoreApi.Geometry).StructLayoutAttribute?.Value);
        Assert.Equal(LayoutKind.Sequential, typeof(ExternalCoreApi.Timing).StructLayoutAttribute?.Value);
        Assert.Equal(nint.Zero, Marshal.OffsetOf<ExternalCoreApi.Geometry>(nameof(ExternalCoreApi.Geometry.BaseWidth)));
        Assert.True(Marshal.OffsetOf<ExternalCoreApi.Timing>(nameof(ExternalCoreApi.Timing.SampleRate)).ToInt64() > 0);
    }

    [Fact]
    public void NativeCallbacks_UseCdeclCallingConvention()
    {
        var delegates = typeof(ExternalCoreApi).GetNestedTypes(BindingFlags.NonPublic)
            .Where(type => typeof(Delegate).IsAssignableFrom(type));

        Assert.NotEmpty(delegates);
        Assert.All(delegates, type => Assert.Equal(CallingConvention.Cdecl,
            type.GetCustomAttribute<UnmanagedFunctionPointerAttribute>()?.CallingConvention));
    }

    [Fact]
    public void NativeBooleanValues_UseOneByteMarshalling()
    {
        Assert.Equal(UnmanagedType.I1,
            typeof(ExternalCoreApi.SystemInfo).GetField(nameof(ExternalCoreApi.SystemInfo.NeedFullPath),
                BindingFlags.Instance | BindingFlags.NonPublic)?.GetCustomAttribute<MarshalAsAttribute>()?.Value);
        Assert.Equal(UnmanagedType.I1,
            typeof(ExternalCoreApi.SystemInfo).GetField(nameof(ExternalCoreApi.SystemInfo.BlockExtract),
                BindingFlags.Instance | BindingFlags.NonPublic)?.GetCustomAttribute<MarshalAsAttribute>()?.Value);
        Assert.Equal(UnmanagedType.I1,
            typeof(ExternalCoreApi.EnvironmentCallback).GetMethod("Invoke")?.ReturnParameter
                .GetCustomAttribute<MarshalAsAttribute>()?.Value);
        Assert.Equal(UnmanagedType.I1,
            typeof(ExternalCoreApi.LoadGame).GetMethod("Invoke")?.ReturnParameter
                .GetCustomAttribute<MarshalAsAttribute>()?.Value);
        Assert.Equal(UnmanagedType.I1,
            typeof(ExternalCoreApi.Serialize).GetMethod("Invoke")?.ReturnParameter
                .GetCustomAttribute<MarshalAsAttribute>()?.Value);
    }

    [Fact]
    public void NativeLibrary_CanResolveExportsAndBeDisposedTwice()
    {
        using var library = new ExternalCoreLibrary(Path.Combine(Environment.SystemDirectory, "version.dll"));
        Assert.Throws<EntryPointNotFoundException>(() => library.Resolve<ExternalCoreApi.VoidCall>("missing_export"));

        library.Dispose();
        library.Dispose();

        Assert.Throws<ObjectDisposedException>(() => library.Resolve<ExternalCoreApi.VoidCall>("missing_export"));
    }

    [Fact]
    public void EnvironmentCommandConstants_PreserveExperimentalFlag()
    {
        Assert.Equal(0x10000u, ExternalCoreApiConstants.ExperimentalCommandFlag);
        Assert.Equal(36u | ExternalCoreApiConstants.ExperimentalCommandFlag, ExternalCoreApiConstants.SetMemoryMaps);
        Assert.Equal(42u | ExternalCoreApiConstants.ExperimentalCommandFlag, ExternalCoreApiConstants.SetSupportAchievements);
    }
}
