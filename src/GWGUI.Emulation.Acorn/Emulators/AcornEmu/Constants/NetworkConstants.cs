namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Identifiants de configuration Econet ; les adresses du bus restent dans son câblage BBC.</summary>
internal static class NetworkConstants
{
    internal const string Prefix = "acornemu.econet.", Enabled = Prefix + "enabled", Station = Prefix + "station", ClockRate = Prefix + "clock";
    internal const string Mode = Prefix + "mode", BindAddress = Prefix + "bind", Port = Prefix + "port", Peers = Prefix + "peers";
    internal const string Firmware = "acornemu.nfs", Disconnected = "disconnected", RawUdp = "udp", Aun = "aun";
    internal const string Block = "Econet", ResourcePrefix = "Emulation.Acorn.Network.";
    internal const int DefaultStation = 2, DefaultPort = 32768, DefaultClockRate = 250000;
    internal const int MinimumStation = 1, MaximumStation = 254, MinimumPort = 1, MaximumPort = 65535;
    internal static IReadOnlyList<int> ClockRates { get; } = Array.AsReadOnly<int>([125000, 250000, 500000]);
}
