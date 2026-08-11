namespace GWGUI.MediaEngine.Decoding;

/// <summary>Représente l'état d'intégrité affiché dans une description de structure de flux.</summary>
internal enum FluxIntegrityDescriptionState
{
    /// <summary>Indique que l'intégrité a été validée.</summary>
    Valid,
    /// <summary>Indique que l'intégrité est incorrecte.</summary>
    Invalid,
    /// <summary>Indique que l'intégrité n'a pas pu être déterminée.</summary>
    Unavailable
}

/// <summary>Construit les descriptions techniques attachées aux structures issues du décodage d'un flux.</summary>
internal static class FluxStructureDescriptions
{
    /// <summary>Convertit un résultat d'intégrité nullable en état nommé.</summary>
    /// <param name="valid">Résultat d'intégrité ou valeur nulle lorsqu'il est indisponible.</param>
    /// <returns>État nommé correspondant.</returns>
    public static FluxIntegrityDescriptionState IntegrityState(bool? valid) => valid is null ? FluxIntegrityDescriptionState.Unavailable : valid.Value ? FluxIntegrityDescriptionState.Valid : FluxIntegrityDescriptionState.Invalid;

    /// <summary>Construit l'identité technique commune d'une structure.</summary>
    /// <param name="codec">Nom du codec.</param><param name="kind">Type de structure.</param><param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Secteur.</param><param name="size">Taille en octets.</param><param name="mark">Marque éventuelle.</param><param name="variant">Variante éventuelle.</param>
    /// <returns>Description de l'identité technique.</returns>
    public static string Identity(string codec, FluxStructureKind kind, int cylinder, int head, int sector, int size, byte? mark, string? variant)
    {
        var markText = mark is null ? string.Empty : $", mark {mark.Value:X2}";
        var variantText = string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}";
        return $"{codec} {kind}, C{cylinder} H{head} R{sector}, {size} bytes{markText}{variantText}";
    }

    /// <summary>Décrit un état d'intégrité nommé.</summary>
    /// <param name="label">Nom du contrôle.</param><param name="valid">Résultat du contrôle.</param><returns>Description du contrôle.</returns>
    public static string Integrity(string label, bool? valid) => $"{label} {IntegrityState(valid).ToString().ToLowerInvariant()}";

    /// <summary>Décrit ensemble l'intégrité de l'en-tête et des données.</summary>
    /// <param name="headerValid">Intégrité de l'en-tête.</param><param name="dataValid">Intégrité des données.</param><returns>Description des deux contrôles.</returns>
    public static string Integrity(bool? headerValid, bool? dataValid) => $"{Integrity("header CRC", headerValid)}, {Integrity("data CRC", dataValid)}";

    /// <summary>Construit l'identité technique d'une structure suivie de son contrôle d'intégrité.</summary>
    /// <param name="codec">Codec.</param><param name="kind">Type.</param><param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Secteur.</param><param name="size">Taille.</param><param name="mark">Marque.</param><param name="variant">Variante.</param><param name="integrityLabel">Nom du contrôle.</param><param name="integrityValid">Résultat du contrôle.</param><returns>Description technique complète.</returns>
    public static string WithIntegrity(string codec, FluxStructureKind kind, int cylinder, int head, int sector, int size, byte? mark, string? variant, string integrityLabel, bool? integrityValid) => $"{Identity(codec, kind, cylinder, head, sector, size, mark, variant)}, {Integrity(integrityLabel, integrityValid)}";

    /// <summary>Construit la description d'une structure complète.</summary>
    /// <param name="codec">Codec.</param><param name="kind">Type.</param><param name="cylinder">Cylindre.</param><param name="head">Face.</param><param name="sector">Secteur.</param><param name="size">Taille.</param><param name="mark">Marque.</param><param name="variant">Variante.</param><param name="headerValid">Intégrité d'en-tête.</param><param name="dataValid">Intégrité des données.</param><param name="headerIntegrityLabel">Nom du contrôle d'en-tête.</param><param name="dataIntegrityLabel">Nom du contrôle des données.</param><returns>Description complète.</returns>
    public static string Complete(string codec, FluxStructureKind kind, int cylinder, int head, int sector, int size, byte? mark, string? variant, bool? headerValid, bool? dataValid, string headerIntegrityLabel = "header CRC", string dataIntegrityLabel = "data CRC") => $"{Identity(codec, kind, cylinder, head, sector, size, mark, variant)}, {Integrity(headerIntegrityLabel, headerValid)}, {Integrity(dataIntegrityLabel, dataValid)}";

    /// <summary>Construit la description d'une structure contrôlée par les checksums Amiga de l'en-tête et des données.</summary>
    public static string CompleteWithChecksums(string codec, FluxStructureKind kind, int cylinder, int head, int sector, int size, bool? headerValid, bool? dataValid) => Complete(codec, kind, cylinder, head, sector, size, null, null, headerValid, dataValid, "header checksum", "data checksum");

    /// <summary>Construit la description d'une structure tronquée.</summary>
    /// <param name="codec">Codec.</param><param name="kind">Type.</param><param name="mark">Marque.</param><param name="variant">Variante.</param><returns>Description de la structure tronquée.</returns>
    public static string Truncated(string codec, FluxStructureKind kind, byte? mark, string? variant) => $"{codec} {kind}{(mark is null ? string.Empty : $", mark {mark.Value:X2}")}{(string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}")}, truncated";

    /// <summary>Construit la description de données non appariées.</summary>
    /// <param name="codec">Codec.</param><param name="mark">Marque.</param><param name="variant">Variante.</param><returns>Description des données non appariées.</returns>
    public static string UnpairedData(string codec, byte? mark, string? variant) => $"Unpaired {codec} data{(mark is null ? string.Empty : $", mark {mark.Value:X2}")}{(string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}")}";

    /// <summary>Construit la description d'une marque non classée.</summary>
    /// <param name="codec">Codec.</param><param name="kind">Type.</param><param name="mark">Marque.</param><param name="variant">Variante.</param><returns>Description de la marque non classée.</returns>
    public static string UnclassifiedMark(string codec, FluxStructureKind kind, byte? mark, string? variant) => $"Unclassified {codec} {kind}{(mark is null ? string.Empty : $", mark {mark.Value:X2}")}{(string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}")}";
}
