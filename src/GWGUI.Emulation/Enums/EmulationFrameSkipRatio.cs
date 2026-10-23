namespace GWGUI.Emulation.Enums;

/// <summary>Rapports de saut d'images annoncés par les choix d'options natifs.</summary>
public enum EmulationFrameSkipRatio
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
    FiveOfSix
}
