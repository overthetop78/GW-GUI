using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class NativeLogFunctions
{
    private const int MaximumMessageBytes = 4096;

    // Windows x64 reserves slots for the first four integer/pointer arguments,
    // including variadic calls. Do not apply this signature to another ABI.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void WindowsX64Callback(int level, nint format, nint firstArgument = default, nint secondArgument = default);

    internal static string? Read(nint format, nint firstArgument, nint secondArgument)
    {
        if (format == nint.Zero) return null;
        var text = ReadString(format, MaximumMessageBytes);
        var tokens = Regex.Matches(text, @"%(?:\.\*s|s|%)");
        if (Regex.Replace(text, @"%(?:\.\*s|s|%)", string.Empty).Contains('%')) return text;
        var required = tokens.Sum(token => token.Value switch { "%.*s" => 2, "%s" => 1, _ => 0 });
        if (required > 2) return text;
        var arguments = new[] { firstArgument, secondArgument };
        var argument = 0;
        return Regex.Replace(text, @"%(?:\.\*s|s|%)", match =>
        {
            if (match.Value == "%%") return "%";
            var maximum = MaximumMessageBytes;
            if (match.Value == "%.*s")
            {
                var precision = unchecked((int)arguments[argument++]);
                if (precision >= 0) maximum = Math.Min(precision, MaximumMessageBytes);
            }
            return ReadString(arguments[argument++], maximum);
        });
    }

    private static string ReadString(nint pointer, int maximum)
    {
        if (pointer == nint.Zero) return "(null)";
        var bytes = new byte[maximum];
        var length = 0;
        while (length < maximum)
        {
            var value = Marshal.ReadByte(pointer, length);
            if (value == 0) break;
            bytes[length++] = value;
        }
        return Encoding.UTF8.GetString(bytes, 0, length);
    }
}
