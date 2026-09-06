using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
namespace GWGUI.Tests.Media.ImageRecognition;
internal static class MultipleFormatsScenarios
{
    public static void Candidates()
    {
        var detector = new ImageFormatDetector(RecognitionEvidenceScenarios.Catalog, _ => throw new InvalidOperationException());
        var ambiguous = detector.Detect("virtual.adf", 123);
        Assert.True(ambiguous.RequiresUserChoice); Assert.Null(ambiguous.Format);
        Assert.Equal(new[] { "acorn.adfs.800", "amiga.amigados", "amiga.amigados_hd" }, ambiguous.Candidates.Select(x => x.Id).Order());
        var selected = ambiguous.Candidates.Single(x => x.Id == "amiga.amigados");
        var automatic = detector.Detect("virtual.adf", 901120);
        Assert.Equal(selected.Id, automatic.Format?.Id); Assert.Equal(ambiguous.Candidates, automatic.Candidates);
        Assert.Null(ambiguous.Format); Assert.Equal(FormatConfidence.Ambiguous, ambiguous.Confidence);
    }
}
