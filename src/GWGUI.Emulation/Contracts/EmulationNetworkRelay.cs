namespace GWGUI.Emulation.Contracts;

/// <summary>Relais effectivement déclaré ; sa présence n'accorde aucune conversion de protocole.</summary>
public sealed record EmulationNetworkRelay(EmulationNetworkRelayType Type, string Target);
