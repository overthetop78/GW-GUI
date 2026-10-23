namespace GWGUI.Emulation.Snk.Common.Constants;


// Unique to GWGUI.Emulation.Snk.
public static class CoreHostConstants
{
    /// <summary>Commande logicielle commune, distincte de la remise à zéro matérielle.</summary>
    internal const byte SoftResetCommand = 16;

    internal const string Windows = "windows";
    public const string CommandLineArgument = "--snk-core-host";
    internal const string HostName = "Snk";
    internal const string PipePrefix = "gwgui-snk-";
    internal const string VideoMapPrefix = "gwgui-snk-video-";
    internal const string LocalPipeServerName = ".";
    internal const string UniqueNameFormat = "N";
    internal const int ProtocolVersion = 4;
    internal const int MaximumPipeInstances = 1;
    internal const int PipeBufferSize = 8 * 1024 * 1024;
    internal const int ConnectionTimeoutMilliseconds = 15_000;
    internal const int ResponseTimeoutSeconds = 30;
    internal const int GracefulExitTimeoutMilliseconds = 5_000;
    internal const int NativeOperationSuccess = 0;
    internal const int MinimumVideoSlotCapacity = 64 * 1024;
    internal const string VideoMapGenerationSeparator = "-";
    internal const char ExtensionListSeparator = '|';
    internal const long InitialVideoSequence = 0L;
    internal const int InitialDiagnosticCount = 0;
    internal const byte InitializeCommand = 1;
    internal const byte RunFrameCommand = 2;
    internal const byte HardResetCommand = 3;
    internal const byte StopCommand = 4;
    internal const byte InsertMediaCommand = 5;
    internal const byte EjectMediaCommand = 6;
    internal const byte SaveStateCommand = 7;
    internal const byte LoadStateCommand = 8;
    internal const byte SetOptionCommand = 9;
    internal const byte SelectDiskCommand = 10;
    internal const byte DisposeCommand = 11;
    internal const byte GetDiskStatusCommand = 13;
    internal const byte SetControllerPortDeviceCommand = 15;
    internal const byte SuccessResponse = 1;
    internal const byte FailureResponse = 2;
}
