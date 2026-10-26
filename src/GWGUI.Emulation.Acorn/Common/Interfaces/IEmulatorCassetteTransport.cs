namespace GWGUI.Emulation.Acorn.Common.Interfaces;

/// <summary>Facette cassette synchrone d'un cœur, empruntée par la session Common.</summary>
/// <remarks>Common sérialise Execute avec les autres commandes du cœur. Les états sont des instantanés ;
/// le cœur possède le lecteur et son signal, tandis que la session possède le thread d'exécution.</remarks>
internal interface IEmulatorCassetteTransport
{
    EmulationCassetteState CassetteState { get; }
    bool CassetteActive { get; }
    EmulationCassetteCommand? CassetteOperation { get; }
    IReadOnlySet<EmulationCassetteCommand> CassetteCommands { get; }
    void ExecuteCassette(EmulationCassetteCommand command);
}
