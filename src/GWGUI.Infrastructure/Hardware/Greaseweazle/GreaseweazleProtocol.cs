namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public static class GreaseweazleProtocol
{
    public const string MediaProviderId = "greaseweazle";
    public const int CommunicationClearBaudRate = 10000;
    public const int NormalBaudRate = 9600;
    public const double FakeIndexLeadSeconds = 0.0005;
    public const double HardSectorCaptureSeconds = 2;

    public static readonly Version EarliestSupportedFirmware = new(0, 31);
}
