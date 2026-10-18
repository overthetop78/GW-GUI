using System.IO;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace GWGUI.Emulation.Functions;

internal static partial class EmulationHostProtocolFunctions
{
    internal static TState GetProcessState<TState>(Process? process, bool connectionFailed, bool disposed,
        TState faulted, TState notStarted, TState exited, TState running)
        where TState : struct, Enum
    {
        if (connectionFailed) return faulted;
        if (process is null) return notStarted;
        if (disposed) return exited;
        try { return process.HasExited ? exited : running; }
        catch (InvalidOperationException) { return exited; }
    }

    internal static EmulationVideoStandard? Region(uint nativeRegion) => nativeRegion switch
    {
        0 => EmulationVideoStandard.Ntsc,
        1 => EmulationVideoStandard.Pal,
        _ => null
    };

    internal static int RegionValue(EmulationVideoStandard? region) => region is null ? -1 : (int)region.Value;

    internal static EmulationVideoStandard? ReadRegion(int value, bool allowAllDefinedStandards) =>
        allowAllDefinedStandards
            ? Enum.IsDefined(typeof(EmulationVideoStandard), value) ? (EmulationVideoStandard)value : null
            : value is (int)EmulationVideoStandard.Ntsc or (int)EmulationVideoStandard.Pal
                ? (EmulationVideoStandard)value : null;

    internal static void RunConnectedSession(string pipeName, string serverName, int connectionTimeoutMilliseconds,
        Action<BinaryReader, BinaryWriter> run)
    {
        using var pipe = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(connectionTimeoutMilliseconds);
        using var reader = new BinaryReader(pipe, Encoding.UTF8, leaveOpen: true);
        using var transportWriter = new BinaryWriter(pipe, Encoding.UTF8, leaveOpen: true);
        run(reader, transportWriter);
    }

    internal static void RunCommandLoop<TCommand>(BinaryReader reader, BinaryWriter transportWriter,
        Func<BinaryReader, TCommand> readCommand, TCommand disposeCommand,
        Action<TCommand, BinaryWriter> execute, Action<BinaryWriter, Exception> writeFailure)
        where TCommand : struct, Enum
    {
        while (true)
        {
            TCommand command;
            try { command = readCommand(reader); }
            catch (EndOfStreamException) { break; }
            var exit = EqualityComparer<TCommand>.Default.Equals(command, disposeCommand);
            WriteResponse(transportWriter, writer => execute(command, writer), writeFailure);
            if (exit) break;
        }
    }
}
