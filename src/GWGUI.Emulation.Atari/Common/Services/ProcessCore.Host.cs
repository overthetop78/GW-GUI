using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Runtime.Versioning;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Atari.Common.Services;

[SupportedOSPlatform(CoreHostConstants.Windows)]
internal sealed partial class ProcessCore : IEmulatorCore
{
    private Process StartHostProcess(string pipeName, string videoMapName)
    {
        var startInfo = CommonCoreHostFunctions.Protocol.CreateHostStartInfo(_hostExecutablePath,
            Path.GetDirectoryName(_hostExecutablePath)!, _hostCommand, pipeName, videoMapName);
        return CommonCoreHostFunctions.Protocol.StartHostProcess(startInfo, static () => new InvalidOperationException(CoreHostErrors.ProcessStartFailed));
    }

    private void ReadInitialization(BinaryReader reader)
    {
        CoreSha256 = reader.ReadString();
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        MinimumAudioLatencyMilliseconds = reader.ReadInt32();
        SupportsSaveStates = reader.ReadBoolean();
        ReadRuntimeStatus(reader);
        Options = JsonSerializer.Deserialize<IReadOnlyList<CoreOption>>(reader.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? [];
        Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? [];
        CoreName = reader.ReadString();
        CoreVersion = reader.ReadString();
        SupportedContentExtensions = reader.ReadString().Split(CoreHostConstants.ExtensionListSeparator, StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        LedStates = CommonCoreHostFunctions.ReadLedStates(reader);
        ShutdownRequested = reader.ReadBoolean();
        ControllerFeedback = CommonCoreHostFunctions.Protocol.ReadControllerFeedback(reader);
    }

    private void ReadFrame(BinaryReader reader)
    {
        LatestVideoFrame = CommonCoreHostFunctions.ReadResizableSharedFrame(reader, ref _videoMemory, ref _videoMap, ref _activeVideoMapName) ?? LatestVideoFrame;
        foreach (var chunk in CommonCoreHostFunctions.ReadAudio(reader))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        MinimumAudioLatencyMilliseconds = reader.ReadInt32();
        ReadRuntimeStatus(reader);
        if (reader.ReadBoolean())
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? [];
        LedStates = CommonCoreHostFunctions.ReadLedStates(reader);
        ShutdownRequested = reader.ReadBoolean();
        ControllerFeedback = CommonCoreHostFunctions.Protocol.ReadControllerFeedback(reader);
    }

    private void ReadRuntimeStatus(BinaryReader reader)
    => CommonCoreHostFunctions.Protocol.ReadRuntimeStatus(reader,
        value => Region = RuntimeFunctions.ReadRegion(value),
        value => BufferedAudioFrames = value,
        value => AudioOverrunCount = value,
        value => AudioUnderrunCount = value);
}
