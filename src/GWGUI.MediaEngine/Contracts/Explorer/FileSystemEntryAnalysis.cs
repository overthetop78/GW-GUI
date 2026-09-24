namespace GWGUI.MediaEngine.Contracts.Explorer;

/// <summary>Identifiants d'affichage relayés depuis MediaFileSystems vers App.</summary>
public sealed record FileSystemEntryAnalysis(
    string CategoryId,
    string IconId,
    string TypeResourceKey,
    string Extension,
    string ExecutionKindId,
    string ContentFormatId,
    string TextEncodingId,
    string PreviewKindId);
