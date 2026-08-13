namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public enum GreaseweazleCommand : byte
{
    GetInfo = 0,
    Seek = 2,
    Head = 3,
    Motor = 6,
    WriteFlux = 8,
    GetFluxStatus = 9,
    Select = 12,
    Deselect = 13,
    SetBusType = 14,
    Reset = 16
}
