using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne par l'indice ambigu IMG le Reader qui départage les interprétations brutes prises en charge.</summary>
internal sealed class RawImgRecognitionPolicy : ReaderBackedRecognitionPolicy
{
    /// <summary>Crée la politique déléguant la validation complète au Reader IMG brut.</summary>
    /// <param name="reader">Reader responsable d'interpréter le contenu IMG.</param>
    public RawImgRecognitionPolicy(RawImgReader reader) : base(reader.ReadAsync) { }

    /// <summary>Indique si l'extension IMG doit présélectionner ce Reader sans valider le contenu.</summary>
    /// <param name="context">Contexte dont l'extension doit être examinée.</param>
    /// <param name="cancellationToken">Jeton d'annulation transmis par le registre.</param>
    /// <returns><see langword="true"/> uniquement pour l'extension IMG.</returns>
    public override ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => ValueTask.FromResult(context.Extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase));
}
