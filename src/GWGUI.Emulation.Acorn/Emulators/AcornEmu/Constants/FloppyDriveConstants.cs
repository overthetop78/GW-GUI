namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Caractéristiques des lecteurs raccordables aux interfaces du BBC Micro.</summary>
internal static class FloppyDriveConstants
{
    internal const decimal DiameterInches = 5.25m;
    internal const int FortyTracks = 40, EightyTracks = 80;
    internal const int SectorSize = 256, SingleDensitySectors = 10, DoubleDensitySectors = 16;
    internal const string DfsName = "BBC DFS", AdfsName = "BBC ADFS";
    internal const string DfsSingle40 = "acorn.dfs.ss", DfsSingle80 = "acorn.dfs.ss80";
    internal const string DfsDouble40 = "acorn.dfs.ds", DfsDouble80 = "acorn.dfs.ds80";
    internal const string Adfs160 = "acorn.adfs.160", Adfs320 = "acorn.adfs.320", Adfs640 = "acorn.adfs.640";
}
