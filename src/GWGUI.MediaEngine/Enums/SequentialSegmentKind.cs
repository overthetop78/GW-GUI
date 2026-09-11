namespace GWGUI.MediaEngine.Enums;

/// <summary>Identifies the meaning of one sequential media segment without assigning a machine protocol.</summary>
public enum SequentialSegmentKind
{
    Unknown,
    Samples,
    Pulse,
    Carrier,
    Silence,
    DataBlock,
    Record,
    TapeMark
}
