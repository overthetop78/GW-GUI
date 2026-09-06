namespace GWGUI.Tests.Hardware.GreaseweazleProtocol;
public class GreaseweazleProtocolTests
{
    [Theory] [InlineData(false)] [InlineData(true)] public void HardSectorIndexPairsDefineRevolutionsAndRejectDifferentCounts(bool inconsistent) => RotationNormalizationScenarios.Hard(inconsistent);
    [Fact] public void FluxEncoderBoundariesSpacesAndAstableHaveKnownWireBytes() => FluxEncodingScenarios.Encode();
    [Fact] public void FluxIndexOffsetsAndPendingSpaceHaveKnownIntervals() => FluxEncodingScenarios.IndexAndSpace();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public Task FirmwareHandshakeAndUnsupportedModesCloseTransport(int failure) => ProtocolFramesScenarios.Firmware(failure);
    [Theory] [InlineData(false,0)] [InlineData(false,1)] [InlineData(false,2)] [InlineData(false,3)] [InlineData(true,0)] [InlineData(true,1)] [InlineData(true,2)]
    public Task FluxTransfersFrameArgumentsChunksRetriesAndCancellation(bool write,int failure) => ProtocolFramesScenarios.Flux(write,failure);
    [Fact] public Task CommandsUseExpectedWireBytesAndCloseStopsMotor()=>ProtocolFramesScenarios.Commands();
    [Fact] public Task ProtocolErrorsDoNotSelectDrive()=>ProtocolFramesScenarios.Errors();
    [Fact] public void FluxBytesDecodeToIndependentExpectedIntervals()=>FluxEncodingScenarios.Decode();
    [Theory]
    [InlineData(new byte[]{})]
    [InlineData(new byte[]{1})]
    [InlineData(new byte[]{0,0})]
    [InlineData(new byte[]{250,0})]
    [InlineData(new byte[]{250,0,0})]
    [InlineData(new byte[]{255,1,0})]
    [InlineData(new byte[]{255,3,1,1,1,1,0})]
    public void InvalidFluxIsRejected(byte[] bytes)=>FluxEncodingScenarios.Invalid(bytes);
    [Fact] public void IndexSelectionUsesRequestedRevolutions()=>RotationNormalizationScenarios.Physical();
    [Fact] public void SyntheticIndexUsesProvidedFrequency()=>RotationNormalizationScenarios.Fake();
    [Fact] public void InvalidIndexRequestsAreRejected()=>RotationNormalizationScenarios.Invalid();
}
