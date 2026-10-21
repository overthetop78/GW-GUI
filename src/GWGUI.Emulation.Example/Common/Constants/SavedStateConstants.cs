namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Limite choisie par le module pour les octets d’état produits ou consommés par ses cœurs.</summary>
/// <remarks>Le format et sa version appartiennent au contrat de fichiers d’état du SDK. Cette classe
/// définit uniquement la borne locale des données du cœur ; elle ne déclare aucun second format.</remarks>
internal static class SavedStateConstants
{
    /// <summary>Taille maximale choisie pour un état binaire du cœur, en octets.</summary>
    /// <remarks>La valeur de base est la borne positive de System.Int32, utilisée pour représenter les
    /// longueurs des états dans ces adaptations. Un module peut choisir une borne plus petite adaptée
    /// à ses cœurs. Les contrôles du format partagé et les capacités réelles d’allocation restent applicables.</remarks>
    internal const int MaximumStateSize = int.MaxValue;
}
