namespace GWGUI.Emulation.Contracts;

/// <summary>Choix de création décrit par le service média, sans exposer sa géométrie interne.</summary>
public sealed record FloppyCreationFormatChoice(string FormatId, string Extension, string DisplayName, long Capacity);
