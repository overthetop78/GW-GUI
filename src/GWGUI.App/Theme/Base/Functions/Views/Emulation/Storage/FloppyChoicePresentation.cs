using System.Globalization;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Functions.Views.Emulation.Storage;

/// <summary>Libellés localisés issus de la géométrie, sans interpréter les noms techniques du catalogue.</summary>
internal static class FloppyChoicePresentation
{
    internal static string Model(FloppyDriveModelChoice model, string? moduleId)
    {
        if (model.Sides > 0 && model.Tracks > 0)
        {
            var diameter = model.DiameterInches.ToString("0.##", CultureInfo.CurrentCulture);
            return $"{diameter}″ — {Geometry(model.Sides, model.Tracks, model.DoubleDensity)} — {StorageSizeFormatter.FormatBytes(model.BlankImageSize)}";
        }
        return string.IsNullOrWhiteSpace(model.DisplayResourceKey) ? model.InvariantDisplayValue ?? model.Value
            : moduleId is null ? LocExtension.Get(model.DisplayResourceKey) : LocExtension.GetForModule(moduleId, model.DisplayResourceKey);
    }

    internal static string DiskFormat(FloppyCreationFormatChoice format) => format.Sides > 0 && format.Tracks > 0
        ? $"{format.DisplayName} — {Geometry(format.Sides, format.Tracks, format.DoubleDensity)} — {StorageSizeFormatter.FormatBytes(format.Capacity)}"
        : format.DisplayName;

    internal static FloppyCreationFormatChoice ImageFormat(FloppyCreationFormatChoice format) => format with
    {
        DisplayName = $"{format.Extension.TrimStart('.').ToUpperInvariant()} (*{format.Extension})"
    };

    private static string Geometry(int sides, int tracks, bool doubleDensity) => string.Join(", ",
        LocExtension.Get(sides == 1 ? "Emulation.Storage.Floppy.OneSide" : "Emulation.Storage.Floppy.TwoSides"),
        LocExtension.Get("Emulation.Storage.Floppy.Tracks", tracks),
        LocExtension.Get(doubleDensity ? "Emulation.Storage.Floppy.DoubleDensity" : "Emulation.Storage.Floppy.SingleDensity"));
}
