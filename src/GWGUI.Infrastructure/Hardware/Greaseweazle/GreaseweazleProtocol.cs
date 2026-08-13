namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public static class GreaseweazleProtocol
{
    public const int CommunicationClearBaudRate = 10000;
    public const int NormalBaudRate = 9600;

    public static readonly Version EarliestSupportedFirmware = new(0, 31);
}
