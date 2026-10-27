namespace GWGUI.MediaEngine.Contracts;

/// <summary>Format vierge compatible avec un lecteur, avec sa capacité exacte et son extension.</summary>
public sealed record MediaFloppyCreationFormat(string FormatId, string Extension, string DisplayName, long Capacity);
