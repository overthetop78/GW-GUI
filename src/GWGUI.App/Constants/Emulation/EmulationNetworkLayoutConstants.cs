using System.Windows;

namespace GWGUI.App.Constants.Emulation;

internal static class EmulationNetworkLayoutConstants
{
    internal const string ResourcePrefix = "Emulation.Network.";
    internal const string TitleResource = ResourcePrefix + "Title";
    internal const string DescriptionResource = ResourcePrefix + "Description";
    internal const string RunningResource = ResourcePrefix + "Running";
    internal const string MachineResource = ResourcePrefix + "Machine";
    internal const string InterfaceResource = ResourcePrefix + "Interface";
    internal const string NetworkResource = ResourcePrefix + "Network";
    internal const string ProtocolResource = ResourcePrefix + "Protocol";
    internal const string FormatResource = ResourcePrefix + "Format";
    internal const string TransportResource = ResourcePrefix + "Transport";
    internal const string IpResource = ResourcePrefix + "Ip";
    internal const string PortResource = ResourcePrefix + "Port";
    internal const string AddressResource = ResourcePrefix + "Address";
    internal const string RelayResource = ResourcePrefix + "Relay";
    internal const string EndpointResource = ResourcePrefix + "Endpoint";
    internal const string ValueSeparator = " · ";
    internal const string TypedValueFormat = "{0}: {1}";
    internal const string StylesUri = "/gwgui.app;component/Theme/Base/Views/Styles/EmulationNetworkStyles.xaml";
    internal const string TableStyleResource = "EmulationNetworkTable";
    internal const string TextStyleResource = "EmulationNetworkText";
    internal const string StateStyleResource = "EmulationNetworkState";
    internal const double FontSize = 13;
    internal const double RowHeight = 36;
    internal const double OuterMargin = 12;
    internal const double NoMargin = 0;
    internal const double HeaderAvailableSpace = 24;
    internal const double RunningWidth = 90;
    internal const double MachineWidth = 160;
    internal const double InterfaceWidth = 100;
    internal const double NetworkWidth = 100;
    internal const double ProtocolWidth = 140;
    internal const double FormatWidth = 130;
    internal const double TransportWidth = 100;
    internal const double IpWidth = 140;
    internal const double PortWidth = 80;
    internal const double NativeAddressWidth = 170;
    internal const double RelayWidth = 130;
    internal const double EndpointWidth = 160;
    internal const int NoRefreshPending = 0;
    internal const int RefreshPending = 1;
    public static Thickness CellTextMargin { get; } = new(12, 0, 12, 0);
    public static Thickness HeaderPadding { get; } = new(12, 10, 12, 10);
    public static Thickness TableBorderThickness { get; } = new(1);
    public static Thickness HeaderBorderThickness { get; } = new(0, 0, 0, 1);
    public static Thickness EmptyThickness { get; } = new(0);
}
