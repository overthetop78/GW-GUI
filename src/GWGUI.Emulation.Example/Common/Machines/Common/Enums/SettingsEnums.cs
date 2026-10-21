namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Rapports de saut d’images proposés par les options existantes.</summary>
public enum FrameSkipRatio
{
    /// <summary>Une image non rendue sur deux.</summary>
    OneOfTwo,
    /// <summary>Deux images non rendues sur trois.</summary>
    TwoOfThree,
    /// <summary>Trois images non rendues sur quatre.</summary>
    ThreeOfFour,
    /// <summary>Quatre images non rendues sur cinq.</summary>
    FourOfFive,
    /// <summary>Cinq images non rendues sur six.</summary>
    FiveOfSix,
}
