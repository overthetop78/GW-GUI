namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Identifiants de configuration Econet ; les adresses du bus restent dans son câblage BBC.</summary>
internal static class NetworkConstants
{
    internal const string Prefix = "acornemu.econet.";
    internal const string Enabled = Prefix + "enabled";
    internal const string Station = Prefix + "station";
    internal const string ClockRate = Prefix + "clock";
    internal const string Mode = Prefix + "mode";
    internal const string BindAddress = Prefix + "bind";
    internal const string Port = Prefix + "port";
    internal const string Peers = Prefix + "peers";
    internal const string Firmware = "acornemu.nfs";
    internal const string Disconnected = "disconnected";
    internal const string RawUdp = "udp";
    internal const string Aun = "aun";
    internal const string Block = "Econet";
    internal const string ResourcePrefix = "Emulation.Acorn.Network.";
    internal const string StationAddressFormat = "{0}.{1}";
    internal const char StationAddressSeparator = '.';
    internal const byte LocalNetwork = 0;
    internal const int AddressPartCount = 2;
    internal const int NetworkPartIndex = 0;
    internal const int StationPartIndex = 1;
    internal const long UnobservedRegistryVersion = -1;
    internal const int DefaultStation = 2;
    internal const int DefaultPort = 32768;
    internal const int DefaultClockRate = 250000;
    internal const int MinimumStation = 1;
    internal const int MaximumStation = 254;
    internal const int MinimumPort = 1;
    internal const int MaximumPort = 65535;
    internal static IReadOnlyList<int> ClockRates { get; } = Array.AsReadOnly<int>([125000, 250000, 500000]);
}
