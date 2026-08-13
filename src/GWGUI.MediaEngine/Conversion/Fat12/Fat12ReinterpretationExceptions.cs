namespace GWGUI.MediaEngine.Conversion.Fat12;

public static class Fat12ReinterpretationExceptions
{
    public static InvalidDataException HybridSource() => new("A hybrid disk image cannot be safely reinterpreted as a single FAT12 target.");

    public static InvalidDataException UnsupportedTarget(string formatId) => new($"The target format '{formatId}' is not a supported Atari ST, IBM PC, or MSX FAT12 format.");

    public static InvalidDataException IncompatibleGeometry(string sourceFormatId, string targetFormatId) => new($"The FAT12 geometry of '{sourceFormatId}' is not exactly compatible with '{targetFormatId}'.");

    public static InvalidDataException InvalidBpb(string formatId) => new($"The image '{formatId}' does not contain a complete, consistent FAT12 BPB and layout.");

    public static InvalidDataException MissingSectors(string formatId) => new($"The image '{formatId}' is missing sectors and cannot be safely reinterpreted.");
}
