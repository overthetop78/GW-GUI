namespace GWGUI.Tests.Media.ImageRecognition;
public class ImageRecognitionTests
{
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] public Task RecognitionRegistryValidatesSignaturesReusesBytesAndFallsBack(int failure) => RecognitionEvidenceScenarios.Registry(failure);
    [Theory] [InlineData(false)] [InlineData(true)] public Task RecognitionRegistryDistinguishesAbsentAndRejectedCandidates(bool candidate) => RecognitionEvidenceScenarios.Rejected(candidate);
    [Theory]
    [InlineData(".SCP", 0, "raw.scp")]
    [InlineData(".adf", 901120, "amiga.amigados")]
    [InlineData(".adf", 1802240, "amiga.amigados_hd")]
    [InlineData(".adf", 819200, "acorn.adfs.800")]
    [InlineData(".adf", 820224, "acorn.adfs.800")]
    [InlineData(".st", 368640, "atarist.360")]
    [InlineData(".st", 409600, "atarist.400")]
    [InlineData(".st", 450560, "atarist.440")]
    [InlineData(".st", 737280, "atarist.720")]
    [InlineData(".st", 819200, "atarist.800")]
    [InlineData(".st", 829440, "atarist.810")]
    [InlineData(".st", 901120, "atarist.880")]
    [InlineData(".st", 1474560, "atarist.1440")]
    [InlineData(".atr", 92176, "atari.90")]
    [InlineData(".atr", 133136, "atari.130")]
    [InlineData(".atr", 183952, "atari.180")]
    [InlineData(".ima", 163840, "ibm.160")]
    [InlineData(".ima", 184320, "ibm.180")]
    [InlineData(".ima", 327680, "ibm.320")]
    [InlineData(".ima", 368640, "ibm.360")]
    [InlineData(".ima", 737280, "ibm.720")]
    [InlineData(".ima", 819200, "ibm.800")]
    [InlineData(".ima", 1228800, "ibm.1200")]
    [InlineData(".ima", 1474560, "ibm.1440")]
    [InlineData(".ima", 1720320, "ibm.1680")]
    [InlineData(".ima", 2949120, "ibm.2880")]
    public void SizeAndExtensionEvidence(string extension, long size, string expected) => RecognitionEvidenceScenarios.Size(extension, size, expected);
    [Theory] [InlineData("msa", true)] [InlineData("msa", false)] [InlineData("mac", true)] [InlineData("mac", false)]
    public void HeaderEvidence(string kind, bool valid) => RecognitionEvidenceScenarios.Header(kind, valid);
    [Theory] [InlineData(true)] [InlineData(false)] public void MissingHeaderIsNotEvidence(bool denied) => RecognitionEvidenceScenarios.MissingHeader(denied);
    [Fact] public void AutomaticSelectionPreservesCandidates() => MultipleFormatsScenarios.Candidates();
    [Fact] public void CapabilitiesFilterAndPreserveDefaultOutput() => CapabilitiesScenarios.Capabilities();
    [Fact] public void IncompatibleSourceClearsPresentedSelection() => CapabilitiesScenarios.Presentation();
}
