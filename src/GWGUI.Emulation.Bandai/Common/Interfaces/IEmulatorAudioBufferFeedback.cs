namespace GWGUI.Emulation.Bandai.Common.Interfaces;

/// <summary>Contrat audio obligatoire entre Common et chacun de ses cœurs.</summary>
/// <remarks>Le même contrat public transporte le remplissage avant le calcul de la frame et la demande
/// de latence après ce calcul. Le cœur transmet ces valeurs à ses callbacks ou à son processus d’hôte.
/// La sortie physique reste possédée par la machine ; aucun pointeur ni handle audio ne change de propriétaire.</remarks>
internal interface IEmulatorAudioBufferFeedback : GWGUI.Emulation.Interfaces.IEmulationAudioBufferFeedback
{
}