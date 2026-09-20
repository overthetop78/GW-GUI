using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Functions;
using System.IO;

namespace GWGUI.App.Services.Parity;

internal static class MediaEngineConversionSupport
{
    private static readonly MediaEngineComposition Engine = MediaEngineComposition.CreateDefault();

    public static bool CanConvert(string sourcePathOrExtension, string formatId, string targetExtension)
    {
        var sourceExtension = MediaFileExtensionFunctions.Normalize(Path.GetExtension(SourcePath(sourcePathOrExtension)));
        var sourceKinds = Engine.Recognition.Readers
            .Where(reader => reader.Extensions.Contains(sourceExtension, StringComparer.OrdinalIgnoreCase))
            .SelectMany(reader => reader.RepresentationKinds)
            .Distinct()
            .ToArray();
        if (sourceKinds.Length == 0) return false;

        var writers = Engine.Writing.Registry.FindTargetCandidates(formatId, targetExtension);
        if (writers.Count == 0) return false;
        if (writers.Any(writer => writer.RepresentationKinds.Any(sourceKinds.Contains))) return true;

        return Engine.Conversion.Registry.Converters.Any(converter =>
            converter.SourceRepresentationKinds.Any(sourceKinds.Contains) &&
            writers.Any(writer => writer.RepresentationKinds.Any(converter.TargetRepresentationKinds.Contains)));
    }

    public static bool CanCreate(string formatId, string targetExtension) =>
        Engine.Writing.Registry.FindTargetCandidates(formatId, targetExtension).Count > 0;

    private static string SourcePath(string pathOrExtension) => pathOrExtension.StartsWith('.')
        ? "source" + pathOrExtension
        : pathOrExtension;
}
