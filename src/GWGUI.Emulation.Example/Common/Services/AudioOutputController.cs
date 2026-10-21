using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Adaptation locale du contrôleur audio commun : activation, volume, pause et récupération de la sortie.</summary>
/// <param name="output">Sortie audio empruntée à l’appelant, éventuellement absente.</param>
/// <param name="factory">Fabrique facultative créant une sortie dont le contrôleur devient propriétaire.</param>
/// <remarks>La machine démarre ce contrôleur avec la fréquence du cœur, en Hz, puis lui transmet les blocs PCM.
/// Les traitements restent ceux du contrôleur commun. Dispose libère les sorties créées par la fabrique et détache les sorties empruntées.</remarks>
internal sealed class AudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null) : EmulationAudioOutputController(output, factory)
{


}
