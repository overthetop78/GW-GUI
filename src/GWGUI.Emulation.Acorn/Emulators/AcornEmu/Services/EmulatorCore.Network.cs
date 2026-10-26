using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.NetworkConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Possession du transport réseau de la session ; la carte BBC ne conserve que ses callbacks empruntés.</summary>
internal sealed partial class EmulatorCore
{
    private EconetTransport? _network;
    private void ConfigureNetwork(MachineConfiguration configuration)
    {
        var settings = NetworkFunctions.Read(configuration);
        if (!settings.Enabled || settings.Mode == C.Disconnected) { _machine!.SetNetworkLine(false); return; }
        _network = new(settings, _machine!);
        _machine!.NetworkFrameTransmitted = _network.Transmit; _machine.NetworkPoll = _network.Advance; _machine.SetNetworkLine(true);
    }
    private void DisposeNetwork()
    {
        try { if (_machine is not null) { _machine.NetworkPoll = null; _machine.NetworkFrameTransmitted = null; _machine.SetNetworkLine(false); } }
        finally { var network = _network; _network = null; network?.Dispose(); }
    }
}
