namespace GWGUI.Tests.Emulation.ControllerInput;
[Collection("WPF")]
public class ControllerInputTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public void FallbackDeduplicationPreservesTwoIdenticalPhysicalPadsAndHandlesRemoval()=>DeviceClassificationScenarios.Duplicates();
    [Fact] public void InputMappingsResolveSelectedDeviceKeysAndAxisThresholds() => BindingsAndFeedbackScenarios.Mapping();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void HidReportsNormalizeLengthIdsValuesAndBackendErrors(int variant) => AnalogAndHidScenarios.Report(variant);
    [Fact] public Task SessionInputRoutesRemappedKeysAndReleasesInactiveSession() => sta.Run(BindingsAndFeedbackScenarios.Routing);
    [Theory] [InlineData(0x045e,0x028e,"Xbox360",false)] [InlineData(0x045e,0x0b12,"XboxSeries",false)] [InlineData(0x10f5,0x7122,"XboxRematchCore",true)] [InlineData(0x0810,0xe501,"MegaDrive6",true)] [InlineData(0x0079,0x0006,"Nintendo64",true)] [InlineData(0x054c,0x05c4,"PlayStation4",true)] [InlineData(0x054c,0x09cc,"PlayStation4",true)] [InlineData(0x054c,0x0ce6,"PlayStation5",true)]
    public void KnownControllerIdentity(ushort vendor, ushort product, string model, bool exact) => DeviceClassificationScenarios.Known(vendor,product,model,exact);
    [Fact] public void UnknownAndAmbiguousControllerNames() => DeviceClassificationScenarios.Fallbacks();
    [Theory] [InlineData(0x40000u,0,0,true)] [InlineData(0xeu,0,0,true)] [InlineData(1u,1,4,true)] [InlineData(1u,1,5,true)] [InlineData(1u,1,8,true)] [InlineData(1u,1,6,false)] [InlineData(1u,2,4,false)] [InlineData(0u,1,4,false)]
    public void ControllerClassificationUsesKindAndUsage(uint kind, ushort page, ushort usage, bool expected) => DeviceClassificationScenarios.Classification(kind,page,usage,expected);
    [Theory] [InlineData(255u, 8, true, -1)] [InlineData(128u, 8, true, -128)] [InlineData(127u, 8, true, 127)] [InlineData(255u, 8, false, 255)] [InlineData(65535u, 16, true, -1)] [InlineData(4294967295u, 32, true, -1)]
    public void HidSignedFields(uint raw, ushort bits, bool signed, int expected) => AnalogAndHidScenarios.Signed(raw,bits,signed,expected);
    [Theory] [InlineData(-100, -100, 100, 0f)] [InlineData(0, -100, 100, .5f)] [InlineData(100, -100, 100, 1f)] [InlineData(-200, -100, 100, 0f)] [InlineData(200, -100, 100, 1f)] [InlineData(3, 5, 5, 0f)] [InlineData(0, int.MinValue, int.MaxValue, .5f)]
    public void HidAxisNormalization(int raw, int minimum, int maximum, float expected) => AnalogAndHidScenarios.Normalize(raw,minimum,maximum,expected);
    [Fact] public void HatDirectionsAndNeutralValues() => AnalogAndHidScenarios.Hat();
    [Fact] public void AnalogDeadZonesClampAndRemoveTriggerButtons()=>AnalogAndHidScenarios.DeadZones();
    [Fact] public void PointerDeltasAccumulateOnceWithoutOverflow()=>BindingsAndFeedbackScenarios.Accumulate();
    [Fact] public void KeyboardChordMatchesExactlyAndIdentifiesReservedKeys()=>BindingsAndFeedbackScenarios.Chords();
}
