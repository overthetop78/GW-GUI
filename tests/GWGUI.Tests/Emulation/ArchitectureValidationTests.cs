using GWGUI.Emulation;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class ArchitectureValidationTests
{
    [Theory]
    [InlineData(nameof(EmulationSdk.CreateAudioBuffer), typeof(IEmulationAudioBuffer))]
    [InlineData(nameof(EmulationSdk.CreateVideoBufferSet), typeof(IEmulationVideoBufferSet))]
    [InlineData(nameof(EmulationSdk.CreateSharedVideoWriter), typeof(IEmulationSharedVideoWriter))]
    public void BufferFactoriesExposePublicInterfaces(string factoryName, Type contract)
    {
        var factory = typeof(EmulationSdk).GetMethod(factoryName)!;
        Assert.True(factory.IsPublic);
        Assert.Equal(contract, factory.ReturnType);
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
    }

    [Theory]
    [InlineData(nameof(EmulationSdk.AudioProcessing), typeof(IEmulationAudioProcessing))]
    [InlineData(nameof(EmulationSdk.SessionDirectories), typeof(IEmulationSessionDirectories))]
    [InlineData(nameof(EmulationSdk.StateFiles), typeof(IEmulationStateFiles))]
    [InlineData(nameof(EmulationSdk.InputProcessing), typeof(IEmulationInputProcessing))]
    [InlineData(nameof(EmulationSdk.HostProtocol), typeof(IEmulationHostProtocol))]
    [InlineData(nameof(EmulationSdk.CoreReleases), typeof(IEmulationCoreReleases))]
    [InlineData(nameof(EmulationSdk.CoreDiagnostics), typeof(IEmulationCoreDiagnostics))]
    [InlineData(nameof(EmulationSdk.NativeResources), typeof(IEmulationNativeResources))]
    public void SharedProcessingApisExposePublicInterfacesAndInternalImplementations(string serviceName, Type contract)
    {
        var entry = typeof(EmulationSdk).GetProperty(serviceName)!;
        Assert.Equal(contract, entry.PropertyType);
        Assert.True(contract.IsInterface);
        Assert.True(contract.IsPublic);
        Assert.False(entry.GetValue(null)!.GetType().IsVisible);
    }

    [Fact]
    public void NativeStringPreservesUtf8CapacityAndReleasesItsPointer()
    {
        var text = EmulationSdk.NativeResources.CreateString("été", minimumByteCapacity: 32);
        try
        {
            Assert.False(text.GetType().IsVisible);
            Assert.Equal("été", Marshal.PtrToStringUTF8(text.Pointer));
            var bytes = new byte[32];
            Marshal.Copy(text.Pointer, bytes, 0, bytes.Length);
            Assert.Equal(new byte[] { 0xc3, 0xa9, 0x74, 0xc3, 0xa9 }, bytes[..5]);
            Assert.All(bytes[5..], value => Assert.Equal((byte)0, value));
        }
        finally { text.Dispose(); }
        Assert.Equal(nint.Zero, text.Pointer);
        text.Dispose();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeStringRetainsTheRequestedPathEncoding(bool useWindowsAnsi)
    {
        var text = EmulationSdk.NativeResources.CreateString("été", useWindowsAnsi);
        try
        {
            var decoded = useWindowsAnsi && OperatingSystem.IsWindows() ? Marshal.PtrToStringAnsi(text.Pointer) : Marshal.PtrToStringUTF8(text.Pointer);
            Assert.Equal("été", decoded);
        }
        finally { text.Dispose(); }
    }

    [Fact]
    public void NativeStringRegistrationTransfersOwnershipOnlyAfterSuccessfulAdd()
    {
        var owner = new Dictionary<string, IEmulationNativeString>(StringComparer.Ordinal);
        try
        {
            var text = EmulationSdk.NativeResources.AddString(owner, "mode", "normal");
            Assert.Same(text, owner["mode"]);
            Assert.Throws<ArgumentException>(() => EmulationSdk.NativeResources.AddString(owner, "mode", "replacement"));
            Assert.Same(text, owner["mode"]);
            Assert.Equal("normal", Marshal.PtrToStringUTF8(text.Pointer));
        }
        finally
        {
            foreach (var text in owner.Values) text.Dispose();
        }
        Assert.All(owner.Values, text => Assert.Equal(nint.Zero, text.Pointer));
    }

    [Fact]
    public void NativeStringRejectsInvalidArgumentsBeforeAllocating()
    {
        Assert.Throws<ArgumentNullException>(() => EmulationSdk.NativeResources.CreateString(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmulationSdk.NativeResources.CreateString("text", -1));
    }

    [Fact]
    public void LoadedContentPathOnlyKeepsTheDescriptorAliveUntilDisposal()
    {
        var path = Path.Combine(Path.GetTempPath(), "exemple-é.bin");
        var content = EmulationSdk.NativeResources.CreateLoadedContent(path, needsFullPath: true, useWindowsAnsiPath: false);
        try
        {
            Assert.False(content.GetType().IsVisible);
            var descriptor = Marshal.PtrToStructure<ExternalCoreApi.GameInfo>(content.GameInfo);
            Assert.Equal(path, Marshal.PtrToStringUTF8(descriptor.Path));
            Assert.Equal(nint.Zero, descriptor.Data);
            Assert.Equal((nuint)0, descriptor.Size);
            Assert.Equal(nint.Zero, descriptor.Metadata);
        }
        finally { content.Dispose(); }
        Assert.Equal(nint.Zero, content.GameInfo);
        content.Dispose();
    }

    [Theory]
    [InlineData("EmulationInputMappingFunctions")]
    [InlineData("ExternalCoreKeyboardFunctions")]
    [InlineData("EmulationHostProtocolFunctions")]
    [InlineData("EmulationChildProcessLifetime")]
    public void SharedAlgorithmsRemainInternalBehindThePublicProcessingContract(string implementation)
    {
        var owner = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Functions." + implementation, throwOnError: true)!;
        Assert.False(owner.IsVisible);
        Assert.Empty(owner.GetMethods(BindingFlags.Public | BindingFlags.Static));
    }

    [Fact]
    public void SavedStateBinaryAlgorithmsRemainInternalBehindThePublicInterface()
    {
        var implementation = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Functions.EmulationStateFileFunctions", throwOnError: true)!;
        Assert.False(implementation.IsVisible);
        foreach (var name in new[] { "Read", "ReadFile", "Write", "WriteFile" })
        {
            Assert.Null(implementation.GetMethod(name, BindingFlags.Public | BindingFlags.Static));
            Assert.True(implementation.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.IsAssembly);
        }
    }

    [Fact]
    public void ReleaseApiExposesAnInterfaceAndKeepsItsImplementationInternal()
    {
        var entry = typeof(EmulationSdk)
            .GetProperty(nameof(EmulationSdk.CoreReleases))!;
        Assert.Equal(typeof(IEmulationCoreReleases), entry.PropertyType);
        Assert.True(entry.PropertyType.IsPublic);
        var implementation = EmulationSdk.CoreReleases.GetType();
        Assert.False(implementation.IsVisible);
        Assert.Null(implementation.GetMethod("ReadJsonAsync", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
    }

    [Fact]
    public void DiagnosticApiExposesAnInterfaceAndKeepsItsImplementationInternal()
    {
        var entry = typeof(EmulationSdk)
            .GetProperty(nameof(EmulationSdk.CoreDiagnostics))!;
        Assert.Equal(typeof(IEmulationCoreDiagnostics), entry.PropertyType);
        Assert.True(entry.PropertyType.IsPublic);
        var implementation = EmulationSdk.CoreDiagnostics.GetType();
        Assert.False(implementation.IsVisible);
        foreach (var name in new[] { "ReadArchitecture", "ReadDeclaredVersion", "ReadExports" })
            Assert.Null(implementation.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
    }

    [Theory]
    [InlineData("x64", true)]
    [InlineData("X64", false)]
    [InlineData("x64 ", false)]
    [InlineData("arm64", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ReadsTheErrorOnlyOnceAndOnlyAfterOrdinalRejection(string? actual, bool accepted)
    {
        var reads = 0;
        string Message()
        {
            reads++;
            return "architecture refusée";
        }

        if (accepted)
        {
            EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message);
            Assert.Equal(0, reads);
        }
        else
        {
            var error = Assert.Throws<InvalidDataException>(() =>
                EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message));
            Assert.Equal("architecture refusée", error.Message);
            Assert.Equal(1, reads);
        }
    }

    [Fact]
    public void PreservesTheMessageProviderFailureAfterRejection()
    {
        var expected = new InvalidOperationException("message unavailable");
        string Message() => throw expected;

        EmulationSdk.CoreDiagnostics.RequireArchitecture("x64", "x64", Message);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.CoreDiagnostics.RequireArchitecture("unknown", "x64", Message)));
    }
}
