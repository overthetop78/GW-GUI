using System.Net.Http;

namespace GWGUI.Emulation.Contracts;

/// <summary>Services fournis à un adaptateur pour rechercher et installer ses cœurs.</summary>
/// <param name="HttpClient">Client HTTP emprunté à l’hôte : l’adaptateur ne doit pas le détruire.</param>
/// <param name="CoreDirectory">Dossier de travail des installations gérées par cet adaptateur.</param>
public sealed record EmulatorManagementContext(
    HttpClient HttpClient,
    string CoreDirectory);
