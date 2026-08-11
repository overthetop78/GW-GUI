namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Construit les descriptions techniques du format E-mu FM.</summary>
internal static class EmuFmDescriptions
{
    /// <summary>Décrit un en-tête et ses contrôles d'intégrité.</summary>
    public static string Header(EmuTrackIdentity identity, bool? dataValid) => FluxStructureDescriptions.Complete(EmuFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, identity.Cylinder, identity.Head, EmuFmFormat.SectorNumber, EmuFmFormat.SectorSize, null, null, true, dataValid, EmuFmFormat.CrcDescription, EmuFmFormat.CrcDescription);
    /// <summary>Décrit un bloc de données et son CRC.</summary>
    public static string Data(EmuTrackIdentity identity, bool valid) => FluxStructureDescriptions.WithIntegrity(EmuFmFormat.StructureDescriptionName, FluxStructureKind.FormatData, identity.Cylinder, identity.Head, EmuFmFormat.SectorNumber, EmuFmFormat.SectorSize, null, null, EmuFmFormat.CrcDescription, valid);
    /// <summary>Décrit une marque non classée.</summary>
    public static string UnclassifiedMark() => FluxStructureDescriptions.UnclassifiedMark(EmuFmFormat.UnclassifiedStructureName, FluxStructureKind.FormatHeader, null, EmuFmFormat.MarkDescription);
}

/// <summary>Représente le cylindre et la face extraits de la piste E-mu.</summary>
internal sealed record EmuTrackIdentity(byte Cylinder, byte Head);
