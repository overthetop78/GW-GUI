using GWGUI.Emulation;
using System.Globalization;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Enums;

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
        Assert.True(EmulationSdk.EnvironmentProcessing.TryHandleStatelessCommand(
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
            Assert.True(EmulationSdk.EnvironmentProcessing.WritePointer(destination, expected));
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
        Assert.False(EmulationSdk.EnvironmentProcessing.WritePointer(nint.Zero, nint.Zero));
        Assert.False(EmulationSdk.EnvironmentProcessing.WritePointer(nint.Zero, (nint)(-1)));
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetCanDuplicateFrames, true)]
    [InlineData(ExternalCoreApiConstants.GetInputBitmasks, true)]
    [InlineData(ExternalCoreApiConstants.GetCoreOptionsVersion, true)]
    [InlineData(ExternalCoreApiConstants.GetMessageInterfaceVersion, false)]
    public void LegacyNullRepliesPreserveTheOriginalQueryPolicy(uint command, bool expected)
    {
        Assert.True(EmulationSdk.EnvironmentProcessing.TryHandleLegacyQuery(command, nint.Zero,
            7, 11, out var result));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetCanDuplicateFrames, 1, 1)]
    [InlineData(ExternalCoreApiConstants.GetInputBitmasks, 1, 1)]
    [InlineData(ExternalCoreApiConstants.GetCoreOptionsVersion, 7, 4)]
    [InlineData(ExternalCoreApiConstants.GetMessageInterfaceVersion, 11, 4)]
    public void LegacyRepliesKeepTheProfileValuesAndNativeWriteWidths(uint command, int expected, int width) =>
        WithBuffer(data =>
        {
            var before = unchecked((ulong)Marshal.ReadInt64(data));
            Assert.True(EmulationSdk.EnvironmentProcessing.TryHandleLegacyQuery(command, data,
                7, 11, out var result));
            Assert.True(result);
            var mask = width == 1 ? 0xffUL : 0xffffffffUL;
            Assert.Equal((before & ~mask) | (uint)expected, unchecked((ulong)Marshal.ReadInt64(data)));
        });

    [Theory]
    [InlineData(ExternalCoreApiConstants.GetOverscan)]
    [InlineData(ExternalCoreApiConstants.GetFastForwarding)]
    [InlineData(ExternalCoreApiConstants.GetInputDeviceCapabilities)]
    [InlineData(ExternalCoreApiConstants.GetLanguage)]
    [InlineData(ExternalCoreApiConstants.GetPerformanceInterface)]
    [InlineData(ExternalCoreApiConstants.GetVariable)]
    [InlineData(ExternalCoreApiConstants.SetPixelFormat)]
    [InlineData(ExternalCoreApiConstants.SetVariables)]
    [InlineData(uint.MaxValue)]
    public void LegacyQueryGuardLeavesOtherCommandsAndTheirOutputUntouched(uint command) =>
        WithBuffer(data =>
        {
            var before = Marshal.ReadInt64(data);
            Assert.False(EmulationSdk.EnvironmentProcessing.TryHandleLegacyQuery(command, data,
                7, 11, out var result));
            Assert.False(result);
            Assert.Equal(before, Marshal.ReadInt64(data));
        });
    [Theory]
    [InlineData(1, EmulationPixelFormat.Xrgb8888)]
    [InlineData(2, EmulationPixelFormat.Rgb565)]
    public void StrictAndBroadPixelsShareSupportedFormatsWithoutConstructingErrors(
        int value, EmulationPixelFormat expected) => WithBuffer(data =>
    {
        Marshal.WriteInt32(data, value);
        var before = Marshal.ReadInt64(data);
        var actual = EmulationPixelFormat.Rgb1555;
        Assert.True(EmulationSdk.EnvironmentProcessing.SetPixelFormat(data, ref actual));
        Assert.Equal(expected, actual);
        Assert.Equal(expected, EmulationSdk.EnvironmentProcessing.ReadLegacyPixelFormat(data,
            _ => throw new InvalidOperationException("must not create an error")));
        Assert.Equal(before, Marshal.ReadInt64(data));
    });

    [Fact]
    public void Rgb1555RemainsAvailableOnlyToTheBroadProfile() => WithBuffer(data =>
    {
        Marshal.WriteInt32(data, 0);
        var actual = EmulationPixelFormat.Rgb565;
        Assert.True(EmulationSdk.EnvironmentProcessing.SetPixelFormat(data, ref actual));
        Assert.Equal(EmulationPixelFormat.Rgb1555, actual);
        var error = new NotSupportedException("local profile error");
        var calls = 0;
        Assert.Same(error, Assert.Throws<NotSupportedException>(() =>
            EmulationSdk.EnvironmentProcessing.ReadLegacyPixelFormat(data, value =>
            {
                Assert.Equal(0, value);
                calls++;
                return error;
            })));
        Assert.Equal(1, calls);
    });

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void UnsupportedPixelsPreserveStateAndTheOriginalDeferredError(int value) => WithBuffer(data =>
    {
        Marshal.WriteInt32(data, value);
        var before = Marshal.ReadInt64(data);
        var actual = EmulationPixelFormat.Rgb565;
        Assert.False(EmulationSdk.EnvironmentProcessing.SetPixelFormat(data, ref actual));
        Assert.Equal(EmulationPixelFormat.Rgb565, actual);
        var error = new NotSupportedException("local profile error");
        var calls = 0;
        Assert.Same(error, Assert.Throws<NotSupportedException>(() =>
            actual = EmulationSdk.EnvironmentProcessing.ReadLegacyPixelFormat(data, unsupported =>
            {
                Assert.Equal(value, unsupported);
                calls++;
                return error;
            })));
        Assert.Equal(1, calls);
        Assert.Equal(EmulationPixelFormat.Rgb565, actual);
        Assert.Equal(before, Marshal.ReadInt64(data));
    });

    [Fact]
    public void BroadPixelNullRefusalDoesNotModifyTheCurrentFormat()
    {
        var actual = EmulationPixelFormat.Rgb565;
        Assert.False(EmulationSdk.EnvironmentProcessing.SetPixelFormat(0, ref actual));
        Assert.Equal(EmulationPixelFormat.Rgb565, actual);
    }

    private static bool Handle(uint command, nint data, out bool result) =>
        EmulationSdk.EnvironmentProcessing.TryHandleStatelessCommand(command, data, 7, 11, 0, out result);

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
