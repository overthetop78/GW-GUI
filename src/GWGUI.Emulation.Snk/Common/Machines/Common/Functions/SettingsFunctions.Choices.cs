namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class SettingsChoiceFunctions
{
    internal static string Format(FrameSkipRatio ratio) => ratio switch
    {
        FrameSkipRatio.OneOfTwo => "1/2",
        FrameSkipRatio.TwoOfThree => "2/3",
        FrameSkipRatio.ThreeOfFour => "3/4",
        FrameSkipRatio.FourOfFive => "4/5",
        FrameSkipRatio.FiveOfSix => "5/6",
        _ => throw new ArgumentOutOfRangeException(nameof(ratio))
    };
}