namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public static class GreaseweazleFluxProtocol
{
    public const byte EndOfStream = 0;
    public const byte Escape = 255;
    public const byte LongIntervalStart = 250;
    public const int ExtendedValueLength = 4;
    public const int ReadBufferSize = 4096;
}
