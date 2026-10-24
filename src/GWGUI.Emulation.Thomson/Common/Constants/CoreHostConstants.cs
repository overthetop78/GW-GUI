namespace GWGUI.Emulation.Thomson.Common.Constants;


public static class CoreHostConstants
{
    /// <summary>Commande commune de consultation du statut des disques.</summary>
    internal const byte GetDiskStatusCommand = 13;

    /// <summary>Commande logicielle commune, distincte de la remise à zéro matérielle.</summary>
    internal const byte SoftResetCommand = 16;

    public const string CommandLineArgument = "--thomson-core-host";
    internal const string Windows = "windows";
    internal const string HostName = "Thomson";
    internal const string PipePrefix = "gwgui-thomson-";
    internal const string VideoMapPrefix = "gwgui-thomson-video-";
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
    /// <summary>Publication explicite des changements d’un média par son propriétaire.</summary>
    internal const byte SaveMediaChangesCommand = 12;
    /// <summary>Consultation des changements possibles conservés par le backend.</summary>
    internal const byte HasUnsavedMediaChangesCommand = 14;
}
