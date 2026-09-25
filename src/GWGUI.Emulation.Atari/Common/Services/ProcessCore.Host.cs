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

[SupportedOSPlatform(ProcessCoreConstants.Windows)]
internal sealed partial class ProcessCore : IEmulatorCore
{
private Process StartHostProcess(string pipeName, string videoMapName)
    {
        var startInfo = new ProcessStartInfo(_hostExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_hostExecutablePath)!
        };
        startInfo.ArgumentList.Add(CoreHostConstants.CommandLineArgument);
        startInfo.ArgumentList.Add(pipeName);
        startInfo.ArgumentList.Add(videoMapName);
        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException(CoreHostErrors.ProcessStartFailed);
        try
        {
            EmulationChildProcessLifetime.Attach(process);
            return process;
        }
        catch
        {
            if (!process.HasExited) process.Kill(true);
            process.Dispose();
            throw;
        }
    }

    private void ReadInitialization(BinaryReader reader)
    {
        CoreSha256 = reader.ReadString();
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        SupportsSaveStates = reader.ReadBoolean();
        ReadRuntimeStatus(reader);
        Options = JsonSerializer.Deserialize<IReadOnlyList<CoreOption>>(reader.ReadString(),
            CoreHostFunctions.JsonOptions) ?? [];
        Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(),
            CoreHostFunctions.JsonOptions) ?? [];
        CoreName = reader.ReadString();
        CoreVersion = reader.ReadString();
        SupportedContentExtensions = reader.ReadString()
            .Split(CoreHostConstants.ExtensionListSeparator, StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        LedStates = CoreHostFunctions.ReadLedStates(reader);
    }

    private void ReadFrame(BinaryReader reader)
    {
        LatestVideoFrame = CoreHostFunctions.ReadResizableSharedFrame(reader,
            ref _videoMemory, ref _videoMap, ref _activeVideoMapName) ?? LatestVideoFrame;
        foreach (var chunk in CoreHostFunctions.ReadAudio(reader))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        ReadRuntimeStatus(reader);
        if (reader.ReadBoolean())
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(),
                CoreHostFunctions.JsonOptions) ?? [];
        LedStates = CoreHostFunctions.ReadLedStates(reader);
    }

    private void ReadRuntimeStatus(BinaryReader reader)
    {
        Region = RuntimeFunctions.ReadRegion(reader.ReadInt32());
        BufferedAudioFrames = reader.ReadInt32();
        AudioOverrunCount = reader.ReadInt64();
        AudioUnderrunCount = reader.ReadInt64();
    }
}
