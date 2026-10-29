namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Mécanismes Winchester proposés par Acorn et limite d'adressage du contrôleur émulé.</summary>
internal static class WinchesterModelConstants
{
    internal const string Model110 = "acorn-winchester-110", Model130 = "acorn-winchester-130", Custom = "acorn-winchester-custom";
    internal const int Cylinders110 = 306, Heads110 = 4, Cylinders130 = 640, Heads130 = 6;
    internal const int AddressBits = 21;
    internal const long MaximumBytes = (1L << AddressBits) * AcornScsiConstants.SectorSize;
}
