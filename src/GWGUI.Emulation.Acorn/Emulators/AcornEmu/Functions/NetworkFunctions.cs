using System.Globalization;
using System.Net;
using System.Net.Sockets;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.NetworkConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Valide et présente les options réseau par les champs de configuration existants.</summary>
internal static class NetworkFunctions
{
    internal static EconetSettings Read(MachineConfiguration configuration)
    {
        string Value(string key, string fallback) => configuration.Options.GetValueOrDefault(key, fallback);
        int Number(string key, int fallback) => int.TryParse(Value(key, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        var enabled = bool.TryParse(Value(C.Enabled, bool.FalseString), out var parsedEnabled) ? parsedEnabled : throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        var station = Number(C.Station, C.DefaultStation); var clock = Number(C.ClockRate, C.DefaultClockRate); var port = Number(C.Port, C.DefaultPort);
        var mode = Value(C.Mode, C.Disconnected);
        if (station is < 1 or > 254 || port is < 1 or > 65535 || !C.ClockRates.Contains(clock) || mode is not (C.Disconnected or C.RawUdp or C.Aun) || mode == C.Aun && station == 1) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        if (!IPAddress.TryParse(Value(C.BindAddress, "127.0.0.1"), out var address) || address.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        var peers = new List<EconetPeer>();
        foreach (var item in Value(C.Peers, "").Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pair = item.Split('=', 2, StringSplitOptions.TrimEntries); var id = pair[0].Split('.');
            if (pair.Length != 2 || id.Length != 2 || !byte.TryParse(id[0], NumberStyles.None, CultureInfo.InvariantCulture, out var network) || !byte.TryParse(id[1], NumberStyles.None, CultureInfo.InvariantCulture, out var target) || target is 0 or 255 || !IPEndPoint.TryParse(pair[1], out var endpoint) || endpoint.AddressFamily != AddressFamily.InterNetwork || endpoint.Port == 0 || peers.Any(peer => peer.Network == network && peer.Station == target)) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
            peers.Add(new(network, target, endpoint));
        }
        if (mode == C.Aun && peers.Select(peer => peer.EndPoint).Distinct().Count() != peers.Count) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        return new(enabled, (byte)station, clock, mode, new(address, port), peers.AsReadOnly());
    }

    internal static EmulationSettingsBlock Describe(MachineConfiguration configuration)
    {
        var settings = Read(configuration);
        EmulationSettingsField Field(string key, string label, EmulationSettingsEditor editor, string value, IReadOnlyList<EmulationSettingsChoice>? choices = null, EmulationSettingsNumericRange? range = null, string? explanation = null) =>
            new(key, EmulationMachineTab.Ethernet, C.Block, C.ResourcePrefix + label, editor, value, choices, IsEnabled: key == C.Enabled || settings.Enabled,
                ExplanationResourceKey: explanation, EnabledValue: bool.TrueString, DisabledValue: bool.FalseString, RefreshSettingsOnChange: key == C.Enabled, RequiresRestart: true, NumericRange: range);
        return new(C.Block, EmulationMachineTab.Ethernet, C.ResourcePrefix + "Title",
        [
            Field(C.Enabled, "Enabled", EmulationSettingsEditor.Toggle, settings.Enabled.ToString()),
            Field(C.Station, "Station", EmulationSettingsEditor.Number, settings.Station.ToString(CultureInfo.InvariantCulture), range: new(1, 254, 1)),
            Field(C.ClockRate, "Clock", EmulationSettingsEditor.Selection, settings.ClockRate.ToString(CultureInfo.InvariantCulture), C.ClockRates.Select(rate => new EmulationSettingsChoice(rate.ToString(CultureInfo.InvariantCulture), "", (rate / 1000).ToString(CultureInfo.InvariantCulture) + " kHz")).ToArray()),
            Field(C.Mode, "Mode", EmulationSettingsEditor.Selection, settings.Mode, [new(C.Disconnected, C.ResourcePrefix + "Disconnected"), new(C.RawUdp, C.ResourcePrefix + "Udp"), new(C.Aun, C.ResourcePrefix + "Aun")]),
            Field(C.BindAddress, "BindAddress", EmulationSettingsEditor.Text, settings.LocalEndPoint.Address.ToString()),
            Field(C.Port, "Port", EmulationSettingsEditor.Number, settings.LocalEndPoint.Port.ToString(CultureInfo.InvariantCulture), range: new(1, 65535, 1)),
            Field(C.Peers, "Peers", EmulationSettingsEditor.Text, configuration.Options.GetValueOrDefault(C.Peers, ""), explanation: C.ResourcePrefix + "Peers.Help")
        ]);
    }
}
