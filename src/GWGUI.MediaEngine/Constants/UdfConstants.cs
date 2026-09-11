namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant ECMA-167 and UDF descriptor identifiers, offsets, and limits.</summary>
internal static class UdfConstants
{
    public const int LogicalBlockSize = 2_048;
    public const int FirstVolumeRecognitionSector = 16;
    public const int MaximumVolumeRecognitionSectors = 16;
    public const int VolumeStructureIdentifierOffset = 1;
    public const int VolumeStructureIdentifierLength = 5;
    public const string BeginningExtendedAreaIdentifier = "BEA01";
    public const string Nsr02Identifier = "NSR02";
    public const string Nsr03Identifier = "NSR03";
    public const string TerminatingExtendedAreaIdentifier = "TEA01";

    public const int AnchorSector = 256;
    public const ushort AnchorDescriptorTagId = 2;
    public const ushort PartitionDescriptorTagId = 5;
    public const ushort LogicalVolumeDescriptorTagId = 6;
    public const ushort TerminatingDescriptorTagId = 8;
    public const ushort FileSetDescriptorTagId = 256;
    public const ushort FileIdentifierDescriptorTagId = 257;
    public const ushort FileEntryDescriptorTagId = 261;
    public const ushort ExtendedFileEntryDescriptorTagId = 266;

    public const int DescriptorTagSize = 16;
    public const int DescriptorTagIdOffset = 0;
    public const int DescriptorTagChecksumOffset = 4;
    public const int DescriptorTagCrcOffset = 8;
    public const int DescriptorTagCrcLengthOffset = 10;
    public const int DescriptorTagLocationOffset = 12;

    public const int AnchorMainVolumeDescriptorSequenceExtentOffset = 16;
    public const int ExtentLengthOffset = 0;
    public const int ExtentLocationOffset = 4;

    public const int PartitionNumberOffset = 22;
    public const int PartitionStartingLocationOffset = 188;
    public const int PartitionLengthOffset = 192;

    public const int LogicalVolumeIdentifierOffset = 84;
    public const int LogicalVolumeIdentifierLength = 128;
    public const int LogicalVolumeBlockSizeOffset = 212;
    public const int LogicalVolumeDomainIdentifierOffset = 216;
    public const int EntityIdentifierSuffixOffset = 24;
    public const int LogicalVolumeContentsUseOffset = 248;
    public const int LogicalVolumeMapTableLengthOffset = 264;
    public const int LogicalVolumePartitionMapCountOffset = 268;
    public const int LogicalVolumePartitionMapsOffset = 440;
    public const byte TypeOnePartitionMap = 1;
    public const int TypeOnePartitionMapLength = 6;
    public const int TypeOnePartitionMapPartitionNumberOffset = 4;

    public const ushort MinimumSupportedRevision = 0x0102;
    public const ushort MaximumSupportedRevision = 0x0260;
    public static readonly ushort[] SupportedRevisions = [0x0102, 0x0150, 0x0200, 0x0201, 0x0250, 0x0260];

    public const int LongAllocationDescriptorSize = 16;
    public const int LongAllocationDescriptorLengthOffset = 0;
    public const int LongAllocationDescriptorBlockOffset = 4;
    public const int LongAllocationDescriptorPartitionOffset = 8;
    public const int ShortAllocationDescriptorSize = 8;
    public const int ShortAllocationDescriptorLengthOffset = 0;
    public const int ShortAllocationDescriptorBlockOffset = 4;
    public const uint AllocationDescriptorLengthMask = 0x3FFF_FFFF;
    public const int AllocationDescriptorTypeShift = 30;
    public const ushort ShortAllocationDescriptors = 0;
    public const ushort LongAllocationDescriptors = 1;
    public const ushort ExtendedAllocationDescriptors = 2;
    public const ushort EmbeddedAllocationDescriptors = 3;

    public const int FileSetRootDirectoryIcbOffset = 400;
    public const int IcbTagOffset = 16;
    public const int IcbFileTypeOffset = 11;
    public const int IcbFlagsOffset = 18;
    public const ushort IcbAllocationDescriptorTypeMask = 0x0007;
    public const byte DirectoryFileType = 4;
    public const byte RegularFileType = 5;
    public const byte SymbolicLinkFileType = 12;

    public const int FileEntryInformationLengthOffset = 56;
    public const int FileEntryAccessTimeOffset = 72;
    public const int FileEntryModificationTimeOffset = 84;
    public const int FileEntryAttributeTimeOffset = 96;
    public const int FileEntryExtendedAttributesLengthOffset = 168;
    public const int FileEntryAllocationDescriptorsLengthOffset = 172;
    public const int FileEntryVariableDataOffset = 176;

    public const int ExtendedFileEntryInformationLengthOffset = 56;
    public const int ExtendedFileEntryAccessTimeOffset = 80;
    public const int ExtendedFileEntryModificationTimeOffset = 92;
    public const int ExtendedFileEntryCreationTimeOffset = 104;
    public const int ExtendedFileEntryAttributeTimeOffset = 116;
    public const int ExtendedFileEntryExtendedAttributesLengthOffset = 208;
    public const int ExtendedFileEntryAllocationDescriptorsLengthOffset = 212;
    public const int ExtendedFileEntryVariableDataOffset = 216;

    public const int FileIdentifierCharacteristicsOffset = 18;
    public const int FileIdentifierLengthOffset = 19;
    public const int FileIdentifierIcbOffset = 20;
    public const int FileIdentifierImplementationUseLengthOffset = 36;
    public const int FileIdentifierImplementationUseOffset = 38;
    public const byte FileIdentifierDirectoryFlag = 0x02;
    public const byte FileIdentifierDeletedFlag = 0x04;
    public const byte FileIdentifierParentFlag = 0x08;

    public const int TimestampSize = 12;
    public const int TimestampYearOffset = 2;
    public const int TimestampMonthOffset = 4;
    public const int TimestampDayOffset = 5;
    public const int TimestampHourOffset = 6;
    public const int TimestampMinuteOffset = 7;
    public const int TimestampSecondOffset = 8;
    public const int TimestampCentisecondsOffset = 9;
    public const int TimestampHundredsOfMicrosecondsOffset = 10;
    public const int TimestampMicrosecondsOffset = 11;
    public const ushort TimestampTimezoneMask = 0x0FFF;
    public const ushort TimestampUnspecifiedTimezone = 0x0800;

    public const byte CompressedUnicode8 = 8;
    public const byte CompressedUnicode16 = 16;
}
