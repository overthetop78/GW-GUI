using System.Globalization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;
using C = GWGUI.App.Constants.Emulation.EmulationNetworkLayoutConstants;

namespace GWGUI.App.Contracts.Emulation.Network;

/// <summary>Projection du registre vers les cellules ; les coordonnées structurées ne sont pas modifiées.</summary>
internal sealed record EmulationNetworkRow(EmulationNetworkEndpoint Endpoint)
{
    public bool IsRunning => Endpoint.IsRunning;
    public string MachineId => Endpoint.MachineId;
    public string Interface => string.Equals(Display(Endpoint.InterfaceType), Endpoint.InterfaceId, StringComparison.Ordinal)
        ? Display(Endpoint.InterfaceType) : string.Join(C.ValueSeparator, Display(Endpoint.InterfaceType), Endpoint.InterfaceId);
    public string InterfaceId => Endpoint.InterfaceId;
    public string Network => Display(Endpoint.Network);
    public string Protocol => string.Join(C.ValueSeparator, Endpoint.Protocols.Select(value => Display(value)));
    public string TransferFormat => Display(Endpoint.TransferFormat);
    public string Transport => Display(Endpoint.Transport);
    public string? Ip => Endpoint.IpAddress?.ToString();
    public int? Port => Endpoint.Port;
    public string? HostEndpoint => Endpoint.HostEndpoint;
    public string Address => string.Join(C.ValueSeparator, (Endpoint.Addresses ?? []).Select(address =>
        string.Format(CultureInfo.CurrentCulture, C.TypedValueFormat, Display(address.Type), address.Value)));
    public string Relay => string.Join(C.ValueSeparator, (Endpoint.Relays ?? []).Select(relay =>
        string.Format(CultureInfo.CurrentCulture, C.TypedValueFormat, Display(relay.Type), relay.Target)));

    private static string Display(Enum value) => LocExtension.Get(EmulationNetworkValueFunctions.ResourceKey(value));
}
