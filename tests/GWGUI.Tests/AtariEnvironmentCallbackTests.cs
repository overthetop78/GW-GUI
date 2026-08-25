using GWGUI.Emulation.Atari;
using GWGUI.MediaEngine.Containers.ImageDisk;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AtariEnvironmentCallbackTests
{
    [Fact]
    public void Directories_AreAbsoluteStableAndCreatedBeforeUse()
    {
        var root = TemporaryRoot();
        try
        {
            using var callbacks = CreateCallbacks(root);
            var destination = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                AssertDirectory(callbacks, ExternalCoreApiConstants.GetSystemDirectory, destination, "system");
                AssertDirectory(callbacks, ExternalCoreApiConstants.GetSaveDirectory, destination, "saves");
                AssertDirectory(callbacks, ExternalCoreApiConstants.GetContentDirectory, destination, "assets");
                Assert.True(Directory.Exists(Path.Combine(root, "content")));
            }
            finally { Marshal.FreeHGlobal(destination); }
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void NativeDescriptions_AreCopiedImmediately()
    {
        var root = TemporaryRoot();
        var description = Marshal.StringToCoTaskMemUTF8("Fire");
        var descriptorSize = Marshal.SizeOf<ExternalCoreApi.InputDescriptor>();
        var descriptors = Marshal.AllocHGlobal(descriptorSize * 2);
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.InputDescriptor
            {
                Port = 1, Device = 2, Index = 3, Id = 4, Description = description
            }, descriptors, false);
            Marshal.StructureToPtr(default(ExternalCoreApi.InputDescriptor), descriptors + descriptorSize, false);
            using var callbacks = CreateCallbacks(root);

            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetInputDescriptors, descriptors));
            Marshal.FreeCoTaskMem(description);
            description = nint.Zero;

            var copied = Assert.Single(callbacks.InputDescriptors);
            Assert.Equal("Fire", copied.Description);
            Assert.Equal((1u, 2u, 3u, 4u), (copied.Port, copied.Device, copied.Index, copied.Id));
        }
        finally
        {
            if (description != nint.Zero) Marshal.FreeCoTaskMem(description);
            Marshal.FreeHGlobal(descriptors);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ControllerAndMemoryStructures_AreCopiedImmediately()
    {
        var root = TemporaryRoot();
        var controllerName = Marshal.StringToCoTaskMemUTF8("Atari joystick");
        var addressSpace = Marshal.StringToCoTaskMemUTF8("RAM");
        var devices = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.ControllerDescription>());
        var ports = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.ControllerInfo>() * 2);
        var descriptors = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.MemoryDescriptor>());
        var map = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.MemoryMap>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.ControllerDescription { Description = controllerName, Id = 9 }, devices, false);
            Marshal.StructureToPtr(new ExternalCoreApi.ControllerInfo { Types = devices, Count = 1 }, ports, false);
            Marshal.StructureToPtr(default(ExternalCoreApi.ControllerInfo),
                ports + Marshal.SizeOf<ExternalCoreApi.ControllerInfo>(), false);
            Marshal.StructureToPtr(new ExternalCoreApi.MemoryDescriptor
            {
                Flags = 7, Pointer = (nint)1234, Offset = 5, Start = 6, Select = 7,
                Disconnect = 8, Length = 9, AddressSpace = addressSpace
            }, descriptors, false);
            Marshal.StructureToPtr(new ExternalCoreApi.MemoryMap { Descriptors = descriptors, Count = 1 }, map, false);
            using var callbacks = CreateCallbacks(root);

            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetControllerInfo, ports));
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetMemoryMaps, map));
            Marshal.FreeCoTaskMem(controllerName); controllerName = nint.Zero;
            Marshal.FreeCoTaskMem(addressSpace); addressSpace = nint.Zero;

            Assert.Equal("Atari joystick", Assert.Single(Assert.Single(callbacks.ControllerPorts).Devices).Description);
            var memory = Assert.Single(callbacks.MemoryDescriptors);
            Assert.Equal("RAM", memory.AddressSpace);
            Assert.Equal((nuint)9, memory.Length);
        }
        finally
        {
            if (controllerName != nint.Zero) Marshal.FreeCoTaskMem(controllerName);
            if (addressSpace != nint.Zero) Marshal.FreeCoTaskMem(addressSpace);
            Marshal.FreeHGlobal(map); Marshal.FreeHGlobal(descriptors); Marshal.FreeHGlobal(ports); Marshal.FreeHGlobal(devices);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CommonQueries_ReturnExplicitValuesAndRejectInvalidRotation()
    {
        var root = TemporaryRoot();
        var value = Marshal.AllocHGlobal(sizeof(long));
        try
        {
            using var callbacks = CreateCallbacks(root);
            Marshal.WriteInt32(value, 2);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetRotation, value));
            Assert.Equal(2u, callbacks.Rotation);
            Marshal.WriteInt32(value, 4);
            Assert.False(callbacks.Environment(ExternalCoreApiConstants.SetRotation, value));
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetLanguage, value));
            Assert.InRange(Marshal.ReadInt32(value), 0, 36);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetInputDeviceCapabilities, value));
            Assert.NotEqual(0L, Marshal.ReadInt64(value));
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetFastForwarding, value));
            Assert.Equal(AtariConstants.NativeBooleanFalse, Marshal.ReadByte(value));
            Assert.False(callbacks.Environment(ExternalCoreApiConstants.GetVfsInterface, value));
        }
        finally { Marshal.FreeHGlobal(value); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void UnknownCommands_AreReportedOnce_AndNativeFormatIsNotEvaluated()
    {
        var root = TemporaryRoot();
        var format = Marshal.StringToCoTaskMemUTF8("Loaded %08x: %s %% done");
        try
        {
            using var callbacks = CreateCallbacks(root);
            Assert.False(callbacks.Environment(9999, nint.Zero));
            Assert.False(callbacks.Environment(9999, nint.Zero));
            callbacks.Log(1, format);

            Assert.Single(callbacks.Diagnostics, item => item.Contains("9999", StringComparison.Ordinal));
            Assert.Contains("Loaded <native-argument>: <native-argument> % done", callbacks.Diagnostics);
        }
        finally { Marshal.FreeCoTaskMem(format); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void LedRumbleAndSensorInterfaces_ExposeStableDelegates()
    {
        var root = TemporaryRoot();
        var led = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.LedInterface>());
        var rumble = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.RumbleInterface>());
        var sensor = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.SensorInterface>());
        try
        {
            using var callbacks = CreateCallbacks(root);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetLedInterface, led));
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetRumbleInterface, rumble));
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetSensorInterface, sensor));
            var ledCallback = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetLedState>(
                Marshal.PtrToStructure<ExternalCoreApi.LedInterface>(led).SetLedState);
            ledCallback(3, 1);
            Assert.True(callbacks.LedStates[3]);
            var rumbleCallback = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetRumbleState>(
                Marshal.PtrToStructure<ExternalCoreApi.RumbleInterface>(rumble).SetState);
            Assert.False(rumbleCallback(0, 0, ushort.MaxValue));
            var sensorInterface = Marshal.PtrToStructure<ExternalCoreApi.SensorInterface>(sensor);
            var setSensor = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.SetSensorState>(sensorInterface.SetState);
            var getSensor = Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.GetSensorInput>(sensorInterface.GetInput);
            Assert.False(setSensor(0, 0, 60));
            Assert.Equal(AtariEnvironmentConstants.NoSensorInput, getSensor(0, 0));
        }
        finally
        {
            Marshal.FreeHGlobal(sensor); Marshal.FreeHGlobal(rumble); Marshal.FreeHGlobal(led);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VideoAudioMessagesKeyboardAndAchievements_CopyNativeValues()
    {
        var root = TemporaryRoot();
        var buffer = Marshal.AllocHGlobal(AtariEnvironmentCallbackTestConstants.NativeStructureBufferSize);
        var text = Marshal.StringToCoTaskMemUTF8("Native status");
        try
        {
            using var callbacks = CreateCallbacks(root);
            Marshal.WriteInt32(buffer, AtariConstants.PixelFormatXrgb8888);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetPixelFormat, buffer));
            Marshal.StructureToPtr(new ExternalCoreApi.SystemAvInfo
            {
                Geometry = new ExternalCoreApi.Geometry { BaseWidth = 320, BaseHeight = 240, AspectRatio = 1.25f },
                Timing = new ExternalCoreApi.Timing { FramesPerSecond = 60, SampleRate = 48000 }
            }, buffer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetSystemAvInfo, buffer));
            Assert.Equal(60, callbacks.FramesPerSecond);
            Assert.Equal(48000, callbacks.SampleRate);
            Assert.Equal(1.25f, callbacks.AspectRatio);
            Assert.Equal(320u, callbacks.SystemAvInfo.Geometry.BaseWidth);
            Marshal.StructureToPtr(new ExternalCoreApi.Geometry { BaseWidth = 640, AspectRatio = 1.5f }, buffer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetGeometry, buffer));
            Assert.Equal(1.5f, callbacks.AspectRatio);
            Assert.Equal(640u, callbacks.Geometry.BaseWidth);
            Marshal.StructureToPtr(new ExternalCoreApi.Message { Text = text, Frames = 120 }, buffer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetMessage, buffer));
            Marshal.StructureToPtr(new ExternalCoreApi.MessageExtended
            {
                Text = text, DurationMilliseconds = 2500, Priority = 2, Level = 3, Target = 4, Type = 5, Progress = 75
            }, buffer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetMessageExtended, buffer));
            Marshal.StructureToPtr(new ExternalCoreApi.KeyboardCallback { Callback = (nint)123 }, buffer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetKeyboardCallback, buffer));
            Assert.Equal((nint)123, callbacks.KeyboardCallbackPointer);
            Marshal.WriteByte(buffer, AtariConstants.NativeBooleanTrue);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetSupportAchievements, buffer));
            Assert.True(callbacks.SupportsAchievements);
            Marshal.WriteInt32(buffer, 7);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetPerformanceLevel, buffer));
            Assert.Equal(7u, callbacks.PerformanceLevel);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetCanDuplicateFrames, buffer));
            Assert.Equal(AtariConstants.NativeBooleanTrue, Marshal.ReadByte(buffer));
            Assert.Contains("Native status", callbacks.Diagnostics);
            Assert.Equal(120u, Assert.Single(callbacks.Messages).Frames);
            Assert.Equal(75, Assert.Single(callbacks.ExtendedMessages).Progress);
        }
        finally
        {
            Marshal.FreeCoTaskMem(text);
            Marshal.FreeHGlobal(buffer);
            Directory.Delete(root, recursive: true);
        }
    }

    private static AtariExternalHostCallbacks CreateCallbacks(string root) => new(
        Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "saves"),
        Path.Combine(root, "assets"), new Dictionary<string, string>());

    private static string TemporaryRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-environment-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static void AssertDirectory(AtariExternalHostCallbacks callbacks, uint command, nint destination,
        string expectedName)
    {
        Assert.True(callbacks.Environment(command, destination));
        var first = Marshal.ReadIntPtr(destination);
        Assert.True(callbacks.Environment(command, destination));
        Assert.Equal(first, Marshal.ReadIntPtr(destination));
        var path = Marshal.PtrToStringUTF8(first)!;
        Assert.True(Path.IsPathFullyQualified(path));
        Assert.Equal(expectedName, new DirectoryInfo(path).Name);
        Assert.True(Directory.Exists(path));
    }
}

internal static class AtariEnvironmentCallbackTestConstants
{
    internal const int NativeStructureBufferSize = 128;
}
