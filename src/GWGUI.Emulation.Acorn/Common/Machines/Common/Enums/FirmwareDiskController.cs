namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;

/// <summary>Interfaces disque reconnues pour une identité de ROM, indépendamment du cœur.</summary>
[Flags]
internal enum FirmwareDiskController
{
    /// <summary>Interface non déterminée.</summary>
    Unknown = 0,
    /// <summary>Interface BBC à contrôleur Intel 8271.</summary>
    Intel8271 = 1,
    /// <summary>Interface compatible avec le câblage Acorn WD1770.</summary>
    AcornWd1770 = 2
}
