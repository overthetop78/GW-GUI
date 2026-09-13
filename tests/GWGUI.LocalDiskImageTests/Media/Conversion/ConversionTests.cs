namespace GWGUI.Tests.Media.Conversion;
public class ConversionTests
{
    [Fact] public void MultipleDestinationsExposeReconstructionLoss()=>ConversionPlanScenarios.Multiple();
    [Theory]
    [InlineData(".scp", ".scp", GWGUI.Domain.Conversion.ConversionFidelityLevel.PreservedFlux)]
    [InlineData("HFE", ".SCP", GWGUI.Domain.Conversion.ConversionFidelityLevel.PreservedFlux)]
    [InlineData(".hfe", ".hfe", GWGUI.Domain.Conversion.ConversionFidelityLevel.PreservedFlux)]
    [InlineData(".scp", ".hfe", GWGUI.Domain.Conversion.ConversionFidelityLevel.ReconstructedTracks)]
    [InlineData(".img", ".scp", GWGUI.Domain.Conversion.ConversionFidelityLevel.ReconstructedTracks)]
    [InlineData(".scp", ".img", GWGUI.Domain.Conversion.ConversionFidelityLevel.SectorData)]
    [InlineData(".adf", ".img", GWGUI.Domain.Conversion.ConversionFidelityLevel.SectorData)]
    public void LossPolicyDistinguishesFluxTracksAndSectors(string source,string target,GWGUI.Domain.Conversion.ConversionFidelityLevel expected)=>ConversionPlanScenarios.Fidelity(source,target,expected);
    [Fact] public void AppleSectorOrderUsesExpectedPermutation()=>ConversionContentScenarios.AppleOrder();
    [Fact] public Task RawConversionUsesReaderAndWriterInMemory()=>ConversionContentScenarios.RawConversion();
    [Fact] public void SectorImageBecomesDecodableScpFlux()=>ConversionContentScenarios.SectorToFlux();
    [Fact] public void PlanUsesDefaultOrExplicitExtensionAndRejectsCollisions()=>ConversionPlanScenarios.Plan();
}
