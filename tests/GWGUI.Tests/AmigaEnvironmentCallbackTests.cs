using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Tests;

public sealed class AmigaEnvironmentCallbackTests
{
    [Fact]
    public void SupportNoGame_IsReadFromTheNativeBoolean()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var value = Marshal.AllocHGlobal(1);
        try
        {
            Marshal.WriteByte(value, 1);
            Assert.True(callbacks.Environment(AmigaExternalApi.SetSupportNoGame, value));
            Assert.True(callbacks.SupportsNoGame);
            Marshal.WriteByte(value, 0);
            Assert.True(callbacks.Environment(AmigaExternalApi.SetSupportNoGame, value));
            Assert.False(callbacks.SupportsNoGame);
        }
        finally
        {
            Marshal.FreeHGlobal(value);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void CoreMessages_AreCopiedIntoDiagnostics()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var text = Marshal.StringToCoTaskMemUTF8("Kickstart loaded");
        var message = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.Message>());
        try
        {
            Marshal.StructureToPtr(new AmigaExternalApi.Message { Text = text, Frames = 120 }, message, false);
            Assert.True(callbacks.Environment(AmigaExternalApi.SetMessage, message));
            Assert.Contains(callbacks.Diagnostics, item => item.Contains("Kickstart loaded", StringComparison.Ordinal));
        }
        finally
        {
            Marshal.FreeHGlobal(message);
            Marshal.FreeCoTaskMem(text);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void MissingVariable_IsAHandledRequestWithNullValue()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var key = Marshal.StringToCoTaskMemUTF8("missing_option");
        var variable = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.Variable>());
        try
        {
            Marshal.StructureToPtr(new AmigaExternalApi.Variable { Key = key, Value = (nint)123 }, variable, false);
            Assert.True(callbacks.Environment(AmigaExternalApi.GetVariable, variable));
            Assert.Equal(0, Marshal.PtrToStructure<AmigaExternalApi.Variable>(variable).Value);
            Assert.True(callbacks.Environment(AmigaExternalApi.GetVariable, 0));
        }
        finally
        {
            Marshal.FreeHGlobal(variable);
            Marshal.FreeCoTaskMem(key);
            Directory.Delete(root, true);
        }
    }

    private static string TemporaryRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Environment", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static AmigaExternalHostCallbacks CreateCallbacks(string root) => new(
        Path.Combine(root, "System"), Path.Combine(root, "Content"), Path.Combine(root, "Saves"), null);
}
