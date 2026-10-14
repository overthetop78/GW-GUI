using System.Runtime.InteropServices;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class NativeLogTests
{
    [Theory]
    [InlineData("[PCSX2] %.*s", "échec supplémentaire", 6, "[PCSX2] échec")]
    [InlineData("message %s", "cœur arrêté", -1, "message cœur arrêté")]
    [InlineData("literal %% complete", "unused", -1, "literal % complete")]
    [InlineData("unsupported %d %s", "unused", -1, "unsupported %d %s")]
    [InlineData("%s %s %s", "unused", -1, "%s %s %s")]
    public void FormatsOnlyTheSupportedArgumentsAndBoundsUtf8Precision(string format, string value, int precision, string expected)
    {
        var formatPointer = Marshal.StringToCoTaskMemUTF8(format);
        var valuePointer = nint.Zero;
        try
        {
            valuePointer = Marshal.StringToCoTaskMemUTF8(value);
            Assert.Equal(expected, NativeLogFunctions.Read(formatPointer, precision >= 0 ? precision : valuePointer, precision >= 0 ? valuePointer : nint.Zero));
        }
        finally
        {
            if (valuePointer != nint.Zero) Marshal.FreeCoTaskMem(valuePointer);
            Marshal.FreeCoTaskMem(formatPointer);
        }
    }

    [Fact]
    public void FormatsTwoStringArgumentsWithoutTreatingTheirContentsAsAFormat()
    {
        var format = Marshal.StringToCoTaskMemUTF8("[%s] %s");
        var first = nint.Zero;
        var second = nint.Zero;
        try
        {
            first = Marshal.StringToCoTaskMemUTF8("BIOS");
            second = Marshal.StringToCoTaskMemUTF8("100% chargé");
            Assert.Equal("[BIOS] 100% chargé", NativeLogFunctions.Read(format, first, second));
        }
        finally
        {
            if (second != nint.Zero) Marshal.FreeCoTaskMem(second);
            if (first != nint.Zero) Marshal.FreeCoTaskMem(first);
            Marshal.FreeCoTaskMem(format);
        }
    }
}
