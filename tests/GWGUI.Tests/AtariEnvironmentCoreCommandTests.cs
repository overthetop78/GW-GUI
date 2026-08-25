using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AtariEnvironmentCoreCommandTests
{
    public static IEnumerable<object[]> ObservedCommands => new uint[]
    {
        ExternalCoreApiConstants.SetPerformanceLevel, ExternalCoreApiConstants.GetSystemDirectory,
        ExternalCoreApiConstants.SetPixelFormat, ExternalCoreApiConstants.SetInputDescriptors,
        ExternalCoreApiConstants.GetVariable, ExternalCoreApiConstants.SetVariables,
        ExternalCoreApiConstants.SetSupportNoGame, ExternalCoreApiConstants.GetLogInterface,
        ExternalCoreApiConstants.GetPerformanceInterface, ExternalCoreApiConstants.GetContentDirectory,
        ExternalCoreApiConstants.GetSaveDirectory, ExternalCoreApiConstants.SetControllerInfo,
        ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.GetDiskControlVersion,
        ExternalCoreApiConstants.SetDiskControlExtended, ExternalCoreApiConstants.SetSupportAchievements,
        ExternalCoreApiConstants.GetVfsInterface, ExternalCoreApiConstants.GetMidiInterface,
        ExternalCoreApiConstants.GetInputBitmasks, ExternalCoreApiConstants.SetContentInfoOverride,
        ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback,
        ExternalCoreApiConstants.SetNetworkPacketInterface, ExternalCoreApiConstants.SetSerializationQuirks
    }.Select(command => new object[] { command });

    public static TheoryData<AtariEmulator, uint[]> CommandsByCore => new()
    {
        { AtariEmulator.Hatari, [
            ExternalCoreApiConstants.GetSystemDirectory, ExternalCoreApiConstants.SetPixelFormat,
            ExternalCoreApiConstants.SetInputDescriptors, ExternalCoreApiConstants.GetVariable,
            ExternalCoreApiConstants.SetVariables, ExternalCoreApiConstants.GetLogInterface,
            ExternalCoreApiConstants.GetContentDirectory, ExternalCoreApiConstants.GetSaveDirectory,
            ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.GetDiskControlVersion,
            ExternalCoreApiConstants.SetDiskControlExtended, ExternalCoreApiConstants.SetSerializationQuirks,
            ExternalCoreApiConstants.GetVfsInterface, ExternalCoreApiConstants.GetMidiInterface ] },
        { AtariEmulator.Atari800, [
            ExternalCoreApiConstants.GetSystemDirectory, ExternalCoreApiConstants.SetPixelFormat,
            ExternalCoreApiConstants.SetInputDescriptors, ExternalCoreApiConstants.GetVariable,
            ExternalCoreApiConstants.SetVariables, ExternalCoreApiConstants.SetSupportNoGame,
            ExternalCoreApiConstants.GetLogInterface, ExternalCoreApiConstants.GetContentDirectory,
            ExternalCoreApiConstants.GetSaveDirectory, ExternalCoreApiConstants.SetControllerInfo,
            ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.GetDiskControlVersion,
            ExternalCoreApiConstants.SetDiskControlExtended, ExternalCoreApiConstants.GetVfsInterface,
            ExternalCoreApiConstants.GetInputBitmasks ] },
        { AtariEmulator.Stella, [ ExternalCoreApiConstants.SetPerformanceLevel,
            ExternalCoreApiConstants.SetVariables, ExternalCoreApiConstants.GetLogInterface,
            ExternalCoreApiConstants.GetInputBitmasks ] },
        { AtariEmulator.ProSystem, [ ExternalCoreApiConstants.SetPerformanceLevel,
            ExternalCoreApiConstants.SetVariables, ExternalCoreApiConstants.GetLogInterface,
            ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.SetContentInfoOverride,
            ExternalCoreApiConstants.GetVfsInterface, ExternalCoreApiConstants.GetInputBitmasks ] },
        { AtariEmulator.BeetleLynx, [ ExternalCoreApiConstants.SetPerformanceLevel,
            ExternalCoreApiConstants.GetSystemDirectory, ExternalCoreApiConstants.SetVariables,
            ExternalCoreApiConstants.GetLogInterface, ExternalCoreApiConstants.GetPerformanceInterface,
            ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.GetVfsInterface,
            ExternalCoreApiConstants.GetInputBitmasks ] },
        { AtariEmulator.VirtualJaguar, [ ExternalCoreApiConstants.SetPerformanceLevel,
            ExternalCoreApiConstants.SetVariables, ExternalCoreApiConstants.GetLogInterface,
            ExternalCoreApiConstants.GetCoreOptionsVersion, ExternalCoreApiConstants.SetContentInfoOverride,
            ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback,
            ExternalCoreApiConstants.SetNetworkPacketInterface, ExternalCoreApiConstants.SetSupportAchievements,
            ExternalCoreApiConstants.GetVfsInterface, ExternalCoreApiConstants.GetInputBitmasks ] }
    };

    [Theory]
    [MemberData(nameof(CommandsByCore))]
    public void CommandsObservedFromOfficialCore_AreAllExplicitlyRecognized(AtariEmulator kind, uint[] commands)
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-command-contract-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            Assert.True(Enum.IsDefined(kind));
            using var callbacks = new AtariExternalHostCallbacks(
                Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "saves"),
                Path.Combine(root, "assets"), new Dictionary<string, string>());
            foreach (var command in commands) callbacks.Environment(command, nint.Zero);
            Assert.DoesNotContain(callbacks.Diagnostics,
                diagnostic => diagnostic.StartsWith("Unknown Atari environment command", StringComparison.Ordinal));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [MemberData(nameof(ObservedCommands))]
    public void CommandObservedFromOfficialCores_AcceptsANativeBufferWithoutBecomingUnknown(uint command)
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-command-buffer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var buffer = Marshal.AllocHGlobal(AtariEnvironmentCommandTestConstants.NativeBufferSize);
        ExternalCoreApi.SetEjectState setEject = _ => true;
        ExternalCoreApi.GetEjectState getEject = () => false;
        ExternalCoreApi.GetImageIndex getIndex = () => 0;
        ExternalCoreApi.SetImageIndex setIndex = _ => true;
        ExternalCoreApi.GetImageCount getCount = () => 0;
        ExternalCoreApi.ReplaceImage replace = (_, _) => true;
        ExternalCoreApi.AddImage add = () => true;
        try
        {
            for (var offset = 0; offset < AtariEnvironmentCommandTestConstants.NativeBufferSize; offset++)
                Marshal.WriteByte(buffer, offset, 0);
            if (command == ExternalCoreApiConstants.SetDiskControlExtended)
                Marshal.StructureToPtr(new ExternalCoreApi.DiskControlExtended
                {
                    Basic = new ExternalCoreApi.DiskControl
                    {
                        SetEjectState = Marshal.GetFunctionPointerForDelegate(setEject),
                        GetEjectState = Marshal.GetFunctionPointerForDelegate(getEject),
                        GetImageIndex = Marshal.GetFunctionPointerForDelegate(getIndex),
                        SetImageIndex = Marshal.GetFunctionPointerForDelegate(setIndex),
                        GetImageCount = Marshal.GetFunctionPointerForDelegate(getCount),
                        ReplaceImage = Marshal.GetFunctionPointerForDelegate(replace),
                        AddImage = Marshal.GetFunctionPointerForDelegate(add)
                    }
                }, buffer, false);
            using var callbacks = new AtariExternalHostCallbacks(
                Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "saves"),
                Path.Combine(root, "assets"), new Dictionary<string, string>());

            callbacks.Environment(command, buffer);

            Assert.DoesNotContain(callbacks.Diagnostics,
                diagnostic => diagnostic.StartsWith("Unknown Atari environment command", StringComparison.Ordinal));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
            Directory.Delete(root, recursive: true);
            GC.KeepAlive((setEject, getEject, getIndex, setIndex, getCount, replace, add));
        }
    }
}

internal static class AtariEnvironmentCommandTestConstants
{
    internal const int NativeBufferSize = 512;
}
