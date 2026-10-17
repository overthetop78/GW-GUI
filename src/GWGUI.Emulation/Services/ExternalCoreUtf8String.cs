using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreUtf8String : IDisposable
{
    internal nint Pointer { get; private set; }

    internal ExternalCoreUtf8String(string value, int minimumByteCapacity = 0)
        : this(value, minimumByteCapacity, useWindowsAnsi: false)
    {
    }

    internal ExternalCoreUtf8String(string value, bool useWindowsAnsi)
        : this(value, 0, useWindowsAnsi)
    {
    }

    private ExternalCoreUtf8String(string value, int minimumByteCapacity, bool useWindowsAnsi)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumByteCapacity);
        if (useWindowsAnsi && OperatingSystem.IsWindows())
        {
            Pointer = Marshal.StringToCoTaskMemAnsi(value);
            return;
        }
        var encoded = Encoding.UTF8.GetBytes(value);
        var buffer = new byte[Math.Max(checked(encoded.Length + 1), minimumByteCapacity)];
        encoded.CopyTo(buffer, 0);
        nint allocation = 0;
        try
        {
            allocation = Marshal.AllocCoTaskMem(buffer.Length);
            Marshal.Copy(buffer, 0, allocation, buffer.Length);
            Pointer = allocation;
            allocation = 0;
        }
        finally
        {
            if (allocation != 0) Marshal.FreeCoTaskMem(allocation);
        }
    }

    internal static ExternalCoreUtf8String AddTo(IDictionary<string, ExternalCoreUtf8String> owner, string key, string value)
    {
        var text = new ExternalCoreUtf8String(value);
        var registered = false;
        try
        {
            owner.Add(key, text);
            registered = true;
            return text;
        }
        finally
        {
            if (!registered) text.Dispose();
        }
    }

    public void Dispose()
    {
        if (Pointer == 0) return;
        var allocation = Pointer;
        Pointer = 0;
        Marshal.FreeCoTaskMem(allocation);
    }
}
