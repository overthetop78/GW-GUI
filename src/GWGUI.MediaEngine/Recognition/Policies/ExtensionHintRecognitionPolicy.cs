using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne un Reader à partir d'extensions utilisées uniquement comme indices avant validation du contenu.</summary>
internal sealed class ExtensionHintRecognitionPolicy : ReaderBackedRecognitionPolicy
{
    /// <summary>Extensions copiées lors de la construction et comparées sans tenir compte de la casse.</summary>
    private readonly HashSet<string> extensions;

    /// <summary>Crée une politique qui délègue la validation complète au Reader fourni.</summary>
    /// <param name="read">Fonction de lecture et de validation du Reader.</param>
    /// <param name="extensions">Extensions servant uniquement à présélectionner ce Reader.</param>
    public ExtensionHintRecognitionPolicy(Func<string, CancellationToken, Task<SectorImage>> read, params string[] extensions) : base(read) => this.extensions = new(extensions, StringComparer.OrdinalIgnoreCase);

    /// <summary>Indique si l'extension présélectionne le Reader, sans valider le contenu du fichier.</summary>
    /// <param name="context">Contexte dont l'extension normalisée doit être comparée.</param>
    /// <param name="cancellationToken">Jeton d'annulation transmis par le registre.</param>
    /// <returns><see langword="true"/> lorsque l'extension fait partie des indices configurés.</returns>
    public override ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => ValueTask.FromResult(extensions.Contains(context.Extension));
}
