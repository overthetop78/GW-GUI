namespace GWGUI.Emulation.Contracts;

/// <summary>Adresse native validée par son module, distincte du point de connexion hôte.</summary>
public sealed record EmulationNetworkAddress(EmulationNetworkAddressType Type, string Value);
