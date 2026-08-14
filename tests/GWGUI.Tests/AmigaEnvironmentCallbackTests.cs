using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Amiga;
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

    [Fact]
    public void ControllerInfo_IsCopiedAndUsedForExactPerPortDeviceIds()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var automaticText = Marshal.StringToCoTaskMemUTF8("Automatic");
        var cd32Text = Marshal.StringToCoTaskMemUTF8("CD32 Pad");
        var descriptionSize = Marshal.SizeOf<AmigaExternalApi.ControllerDescription>();
        var descriptions = Marshal.AllocHGlobal(descriptionSize * 2);
        var infoSize = Marshal.SizeOf<AmigaExternalApi.ControllerInfo>();
        var infos = Marshal.AllocHGlobal(infoSize * 2);
        try
        {
            Marshal.StructureToPtr(new AmigaExternalApi.ControllerDescription { Description = automaticText, Id = 111 }, descriptions, false);
            Marshal.StructureToPtr(new AmigaExternalApi.ControllerDescription { Description = cd32Text, Id = 777 }, descriptions + descriptionSize, false);
            Marshal.StructureToPtr(new AmigaExternalApi.ControllerInfo { Types = descriptions, Count = 2 }, infos, false);
            Marshal.StructureToPtr(new AmigaExternalApi.ControllerInfo(), infos + infoSize, false);

            Assert.True(callbacks.Environment(AmigaExternalApi.SetControllerInfo, infos));
            var port = Assert.Single(callbacks.ControllerPorts);
            Assert.Collection(port,
                device => { Assert.Equal("Automatic", device.Name); Assert.Equal(111u, device.Id); },
                device => { Assert.Equal("CD32 Pad", device.Name); Assert.Equal(777u, device.Id); });
            Assert.Equal(777u, AmigaExternalCore.ControllerDevice(callbacks.ControllerPorts, 0, AmigaControllerType.Cd32Pad));
        }
        finally
        {
            Marshal.FreeHGlobal(infos);
            Marshal.FreeHGlobal(descriptions);
            Marshal.FreeCoTaskMem(cd32Text);
            Marshal.FreeCoTaskMem(automaticText);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LedInterface_RecordsCoreDriveActivity()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.LedInterface>());
        try
        {
            Assert.True(callbacks.Environment(AmigaExternalApi.GetLedInterface, pointer));
            var ledInterface = Marshal.PtrToStructure<AmigaExternalApi.LedInterface>(pointer);
            var setLed = Marshal.GetDelegateForFunctionPointer<AmigaExternalApi.SetLedState>(ledInterface.SetLedState);
            setLed(2, 1);
            setLed(3, 0);

            Assert.True(callbacks.LedStates[2]);
            Assert.False(callbacks.LedStates[3]);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LegacyOptions_PreserveVisibilityAndInvokeTheUpdateCallback()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var key = Marshal.StringToCoTaskMemUTF8("legacy_option");
        var definition = Marshal.StringToCoTaskMemUTF8("Legacy option; disabled|enabled");
        var variableSize = Marshal.SizeOf<AmigaExternalApi.Variable>();
        var variables = Marshal.AllocHGlobal(variableSize * 2);
        var display = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.CoreOptionDisplay>());
        var callbackData = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.CoreOptionsUpdateDisplayCallback>());
        var updateCount = 0;
        AmigaExternalApi.UpdateCoreOptionsDisplay update = () => { updateCount++; return true; };
        try
        {
            Marshal.StructureToPtr(new AmigaExternalApi.Variable { Key = key, Value = definition }, variables, false);
            Marshal.StructureToPtr(new AmigaExternalApi.Variable(), variables + variableSize, false);
            Assert.True(callbacks.Environment(AmigaExternalApi.SetVariables, variables));
            var option = Assert.Single(callbacks.OptionCatalog);
            Assert.Equal("legacy_option", option.Key);
            Assert.Equal("disabled", option.DefaultValue);
            Assert.True(option.IsVisible);

            Marshal.StructureToPtr(new AmigaExternalApi.CoreOptionDisplay { Key = key, Visible = false }, display, false);
            Assert.True(callbacks.Environment(AmigaExternalApi.SetCoreOptionsDisplay, display));
            Assert.False(Assert.Single(callbacks.OptionCatalog).IsVisible);

            Marshal.StructureToPtr(new AmigaExternalApi.CoreOptionsUpdateDisplayCallback
            {
                Callback = Marshal.GetFunctionPointerForDelegate(update)
            }, callbackData, false);
            Assert.True(callbacks.Environment(AmigaExternalApi.SetCoreOptionsUpdateDisplayCallback, callbackData));
            callbacks.SetOption("legacy_option", "enabled");
            Assert.Equal(1, updateCount);
        }
        finally
        {
            GC.KeepAlive(update);
            Marshal.FreeHGlobal(callbackData);
            Marshal.FreeHGlobal(display);
            Marshal.FreeHGlobal(variables);
            Marshal.FreeCoTaskMem(definition);
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
