using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Amiga.Cores;
using GWGUI.Emulation.Common;

namespace GWGUI.Tests;

public sealed class AmigaEnvironmentCallbackTests
{
    [Fact]
    public void DirectoryRequests_ReturnStableAbsoluteSessionPaths()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var output = Marshal.AllocHGlobal(IntPtr.Size);
        try
        {
            AssertDirectory(ExternalCoreApiConstants.GetSystemDirectory, callbacks.SystemDirectory);
            AssertDirectory(ExternalCoreApiConstants.GetContentDirectory, callbacks.ContentDirectory);
            AssertDirectory(ExternalCoreApiConstants.GetSaveDirectory, callbacks.SaveDirectory);
            Assert.True(Directory.Exists(callbacks.SystemDirectory));
            Assert.True(Directory.Exists(callbacks.ContentDirectory));
            Assert.True(Directory.Exists(callbacks.SaveDirectory));
        }
        finally
        {
            Marshal.FreeHGlobal(output);
            Directory.Delete(root, true);
        }

        void AssertDirectory(uint command, string expected)
        {
            Marshal.WriteIntPtr(output, 0);
            Assert.True(callbacks.Environment(command, output));
            var first = Marshal.ReadIntPtr(output);
            Assert.NotEqual(0, first);
            Assert.Equal(Path.GetFullPath(expected), Marshal.PtrToStringUTF8(first));
            Assert.True(callbacks.Environment(command, output));
            Assert.Equal(first, Marshal.ReadIntPtr(output));
        }
    }

    [Fact]
    public void AudioCallbacks_PreserveStereoOrderAndBoundQueuedAudioToTwoHundredMilliseconds()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        callbacks.SampleRate = 1000;
        var samples = new short[40];
        for (var index = 0; index < samples.Length; index += 2)
        {
            samples[index] = (short)(100 + index);
            samples[index + 1] = (short)(-100 - index);
        }
        var pointer = Marshal.AllocHGlobal(samples.Length * sizeof(short));
        try
        {
            Marshal.Copy(samples, 0, pointer, samples.Length);
            for (var batch = 0; batch < 15; batch++)
                Assert.Equal((nuint)20, callbacks.AudioBatch(pointer, 20));

            Assert.InRange(callbacks.BufferedAudioFrames, 1, 200);
            Assert.True(callbacks.AudioOverrunCount > 0);
            Assert.True(callbacks.TryDequeueAudio(out var chunk));
            Assert.NotNull(chunk);
            Assert.Equal(samples, chunk!.InterleavedStereo.ToArray());

            while (callbacks.TryDequeueAudio(out _)) { }
            callbacks.AudioSample(123, -456);
            Assert.True(callbacks.TryDequeueAudio(out chunk));
            Assert.Equal(new short[] { 123, -456 }, chunk!.InterleavedStereo.ToArray());
            Assert.Equal(0, callbacks.BufferedAudioFrames);

            var oversized = new short[600];
            for (var index = 0; index < oversized.Length; index++) oversized[index] = (short)index;
            Marshal.FreeHGlobal(pointer);
            pointer = Marshal.AllocHGlobal(oversized.Length * sizeof(short));
            Marshal.Copy(oversized, 0, pointer, oversized.Length);
            Assert.Equal((nuint)300, callbacks.AudioBatch(pointer, 300));
            Assert.Equal(200, callbacks.BufferedAudioFrames);
            Assert.True(callbacks.TryDequeueAudio(out chunk));
            Assert.Equal(200, chunk!.FrameCount);
            Assert.Equal(oversized[^400..], chunk.InterleavedStereo.ToArray());
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SupportNoGame_IsReadFromTheNativeBoolean()
    {
        var root = TemporaryRoot();
        using var callbacks = CreateCallbacks(root);
        var value = Marshal.AllocHGlobal(1);
        try
        {
            Marshal.WriteByte(value, 1);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetSupportNoGame, value));
            Assert.True(callbacks.SupportsNoGame);
            Marshal.WriteByte(value, 0);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetSupportNoGame, value));
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
        var message = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.Message>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.Message { Text = text, Frames = 120 }, message, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetMessage, message));
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
        var variable = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.Variable>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.Variable { Key = key, Value = (nint)123 }, variable, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetVariable, variable));
            Assert.Equal(0, Marshal.PtrToStructure<ExternalCoreApi.Variable>(variable).Value);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetVariable, 0));
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
        var descriptionSize = Marshal.SizeOf<ExternalCoreApi.ControllerDescription>();
        var descriptions = Marshal.AllocHGlobal(descriptionSize * 2);
        var infoSize = Marshal.SizeOf<ExternalCoreApi.ControllerInfo>();
        var infos = Marshal.AllocHGlobal(infoSize * 2);
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.ControllerDescription { Description = automaticText, Id = 111 }, descriptions, false);
            Marshal.StructureToPtr(new ExternalCoreApi.ControllerDescription { Description = cd32Text, Id = 777 }, descriptions + descriptionSize, false);
            Marshal.StructureToPtr(new ExternalCoreApi.ControllerInfo { Types = descriptions, Count = 2 }, infos, false);
            Marshal.StructureToPtr(new ExternalCoreApi.ControllerInfo(), infos + infoSize, false);

            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetControllerInfo, infos));
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
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.LedInterface>());
        try
        {
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetLedInterface, pointer));
            var ledInterface = Marshal.PtrToStructure<ExternalCoreApi.LedInterface>(pointer);
            var setLed = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetLedState>(ledInterface.SetLedState);
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
        var variableSize = Marshal.SizeOf<ExternalCoreApi.Variable>();
        var variables = Marshal.AllocHGlobal(variableSize * 2);
        var display = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.CoreOptionDisplay>());
        var callbackData = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.CoreOptionsUpdateDisplayCallback>());
        var updateCount = 0;
        ExternalCoreApi.UpdateCoreOptionsDisplay update = () => { updateCount++; return true; };
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.Variable { Key = key, Value = definition }, variables, false);
            Marshal.StructureToPtr(new ExternalCoreApi.Variable(), variables + variableSize, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetVariables, variables));
            var option = Assert.Single(callbacks.OptionCatalog);
            Assert.Equal("legacy_option", option.Key);
            Assert.Equal("disabled", option.DefaultValue);
            Assert.True(option.IsVisible);

            Marshal.StructureToPtr(new ExternalCoreApi.CoreOptionDisplay { Key = key, Visible = false }, display, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptionsDisplay, display));
            Assert.False(Assert.Single(callbacks.OptionCatalog).IsVisible);

            Marshal.StructureToPtr(new ExternalCoreApi.CoreOptionsUpdateDisplayCallback
            {
                Callback = Marshal.GetFunctionPointerForDelegate(update)
            }, callbackData, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback, callbackData));
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
