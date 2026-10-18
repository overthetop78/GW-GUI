using System.Globalization;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class StatelessEnvironmentCommandTests
{
    [Theory]
    [InlineData(ExternalCoreApiConstants.GetOverscan, false)]
    [InlineData(ExternalCoreApiConstants.GetFastForwarding, false)]
    [InlineData(ExternalCoreApiConstants.GetCanDuplicateFrames, true)]
    [InlineData(ExternalCoreApiConstants.GetInputBitmasks, true)]
    public void BooleanRepliesKeepTheNativeByteValues(uint command, bool value) => WithBuffer(data =>
    {
        Assert.True(Handle(command, data, out var result));
        Assert.True(result);
        Assert.Equal(value ? (byte)1 : (byte)0, Marshal.ReadByte(data));
    });

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetCoreOptionsVersion, 7)]
    [InlineData(ExternalCoreApiConstants.GetMessageInterfaceVersion, 11)]
    public void InterfaceVersionsComeFromTheHostProfile(uint command, int expected) => WithBuffer(data =>
    {
        Assert.True(Handle(command, data, out var result));
        Assert.True(result);
        Assert.Equal(expected, Marshal.ReadInt32(data));
    });

    [Fact]
    public void CapabilitiesKeepAllSixtyFourBits() => WithBuffer(data =>
    {
        const ulong capabilities = 0x9ABCDEF012345678;
        Assert.True(ExternalCoreEnvironmentFunctions.TryHandleStatelessCommand(
            ExternalCoreApiConstants.GetInputDeviceCapabilities, data, 7, 11, capabilities, out var result));
        Assert.True(result);
        Assert.Equal(capabilities, unchecked((ulong)Marshal.ReadInt64(data)));
    });

    [Fact]
    public void LanguageUsesTheCurrentApplicationCulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            WithBuffer(data =>
            {
                Assert.True(Handle(ExternalCoreApiConstants.GetLanguage, data, out var result));
                Assert.True(result);
                Assert.Equal(2, Marshal.ReadInt32(data));
            });
        }
        finally { CultureInfo.CurrentUICulture = previous; }
    }

    [Fact]
    public void MissingOutputPointerIsAHandledRefusal()
    {
        uint[] commands = [ExternalCoreApiConstants.GetOverscan, ExternalCoreApiConstants.GetFastForwarding,
            ExternalCoreApiConstants.GetCanDuplicateFrames, ExternalCoreApiConstants.GetInputBitmasks,
            ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.GetMessageInterfaceVersion,
            ExternalCoreApiConstants.GetInputDeviceCapabilities, ExternalCoreApiConstants.GetLanguage];
        foreach (var command in commands)
        {
            Assert.True(Handle(command, 0, out var result));
            Assert.False(result);
        }
    }

    [Fact]
    public void UnsupportedPerformanceInterfaceIsRecognizedWithoutWriting() => WithBuffer(data =>
    {
        var before = Marshal.ReadInt64(data);
        Assert.True(Handle(ExternalCoreApiConstants.GetPerformanceInterface, data, out var result));
        Assert.False(result);
        Assert.Equal(before, Marshal.ReadInt64(data));
    });

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetDiskControlVersion)]
    [InlineData(uint.MaxValue)]
    public void OtherCommandsRemainWithTheModule(uint command) => WithBuffer(data =>
    {
        var before = Marshal.ReadInt64(data);
        Assert.False(Handle(command, data, out var result));
        Assert.False(result);
        Assert.Equal(before, Marshal.ReadInt64(data));
    });

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(long.MaxValue)]
    public void PointerRepliesPreserveNativeBitsAndBufferBoundaries(long value)
    {
        var bytes = Enumerable.Repeat((byte)0xa5, 3 * IntPtr.Size).ToArray();
        var allocation = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, allocation, bytes.Length);
            var destination = allocation + IntPtr.Size;
            var expected = unchecked((nint)value);
            Assert.True(ExternalCoreEnvironmentFunctions.WritePointer(destination, expected));
            Assert.Equal(expected, Marshal.ReadIntPtr(destination));
            Marshal.Copy(allocation, bytes, 0, bytes.Length);
            Assert.All(bytes.Take(IntPtr.Size), item => Assert.Equal((byte)0xa5, item));
            Assert.All(bytes.Skip(2 * IntPtr.Size), item => Assert.Equal((byte)0xa5, item));
        }
        finally { Marshal.FreeHGlobal(allocation); }
    }

    [Fact]
    public void PointerRepliesRefuseANullDestination()
    {
        Assert.False(ExternalCoreEnvironmentFunctions.WritePointer(nint.Zero, nint.Zero));
        Assert.False(ExternalCoreEnvironmentFunctions.WritePointer(nint.Zero, (nint)(-1)));
    }

    private static bool Handle(uint command, nint data, out bool result) =>
        ExternalCoreEnvironmentFunctions.TryHandleStatelessCommand(command, data, 7, 11, 0, out result);

    private static void WithBuffer(Action<nint> run)
    {
        var data = Marshal.AllocHGlobal(sizeof(long));
        try
        {
            Marshal.WriteInt64(data, 0x123456789ABCDEF);
            run(data);
        }
        finally { Marshal.FreeHGlobal(data); }
    }
}
