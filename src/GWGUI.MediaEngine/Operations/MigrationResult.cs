using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Operations;

/// <summary>Image remplie par MediaFileSystems et rapport retournés par l'API MediaEngine.</summary>
public sealed record MigrationResult(SectorImage Image, MigrationValidationReport Report);
