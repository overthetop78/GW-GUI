namespace GWGUI.App.Constants.Services.PhysicalDiskReading;

public static class PhysicalDiskReadDefaults
{
    public const int Revolutions = 3;
    public const int FluxOverflowRetries = 5;
    public const int SeekRetries = 3;
    public const double HardSectorCaptureSeconds = 2;
    public const byte ScpVersion = 0x19;
    public const byte ScpResolution = 0;
    public const long InMemoryFileSize = 0;
}
