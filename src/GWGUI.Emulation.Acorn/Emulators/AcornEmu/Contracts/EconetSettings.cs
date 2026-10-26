using System.Net;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;

/// <summary>Une station distante explicitement reliée à son point d'accès hôte.</summary>
internal sealed record EconetPeer(byte Network, byte Station, IPEndPoint EndPoint);

/// <summary>Paramètres propres à l'extension ; aucune socket n'est ouverte pendant leur lecture.</summary>
internal sealed record EconetSettings(bool Enabled, byte Station, int ClockRate, string Mode, IPEndPoint LocalEndPoint, IReadOnlyList<EconetPeer> Peers)
{
    internal int CyclesPerByte => checked((int)(BbcMicroModelConstants.CpuFrequencyHz * 8 / ClockRate));
}
