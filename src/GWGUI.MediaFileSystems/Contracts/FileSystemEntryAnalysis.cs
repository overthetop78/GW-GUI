namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Identifiants d'affichage d'une entrée extraits de la réponse de MediaAnalysis.</summary>
public sealed record FileSystemEntryAnalysis(
    string CategoryId,
    string IconId,
    string TypeResourceKey,
    string Extension,
    string ExecutionKindId,
    string ContentFormatId,
    string TextEncodingId,
    string PreviewKindId);
