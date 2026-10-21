namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Choix de sortie physique et de latence partagé par les trois émulateurs.</summary>
/// <param name="OutputDeviceId">Identité de sortie fournie par l’hôte, ou null pour sa sortie par défaut.</param>
/// <param name="LatencyMilliseconds">Latence du tampon en millisecondes ; la valeur de base est définie par le module.</param>
/// <remarks>Cette description ne possède pas la sortie. Common transmet ces choix à sa fabrique et au contrôleur audio.</remarks>
public sealed record AudioConfiguration(string? OutputDeviceId = null, int LatencyMilliseconds = AudioConstants.DefaultLatencyMilliseconds);
