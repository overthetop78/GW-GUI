namespace GWGUI.Emulation.Watara.Common.Constants;


public static class CoreHostConstants
{
    public const string CommandLineArgument = "--watara-core-host";
    internal const string Windows = "windows";
    internal const string HostName = "Watara";
    internal const string PipePrefix = "gwgui-watara-";
    internal const string VideoMapPrefix = "gwgui-watara-video-";
    internal const string LocalPipeServerName = ".";
    internal const string UniqueNameFormat = "N";
    internal const int ProtocolVersion = 5;
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
}
