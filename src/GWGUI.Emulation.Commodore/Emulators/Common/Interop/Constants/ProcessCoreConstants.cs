
namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Constants;

internal static class ProcessCoreConstants
{
    internal const string TransportName = "Commodore";
    internal const string CoreHost = "--commodore-core-host";
    internal const string PipeNamePrefix = "gwgui-commodore-";
    internal const string VideoMapNamePrefix = "gwgui-commodore-video-";
    internal const string SessionIdentifierFormat = "N";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
    internal const int PipeInstanceCount = 1;
    internal const int InitializationTimeoutSeconds = 15;
    internal const int CommandTimeoutSeconds = 30;
    internal const int ProcessExitTimeoutMilliseconds = 5_000;
    internal const long ViewStartOffset = 0;
    internal const char ExtensionSeparator = '|';
    internal const string ProcessExitDetailFormat = " It exited with code {0}.";
}
