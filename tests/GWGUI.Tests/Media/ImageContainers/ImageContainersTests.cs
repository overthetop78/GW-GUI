namespace GWGUI.Tests.Media.ImageContainers;
public class ImageContainersTests
{
    [Theory] [InlineData(false)] [InlineData(true)] public Task AppleRawMfsAndLisaRequireStructure(bool lisa) => AppleContainerScenarios.RawStructures(lisa);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] public Task TwoImgInvalidVersionFormatAndPayloadRange(int damage) => AppleContainerScenarios.TwoImgInvalid(damage);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] public Task AppleRawDosSosAndProDos800(int kind) => AppleContainerScenarios.RawVariant(kind);
    [Theory] [InlineData(3)] [InlineData(800)] [InlineData(1702)] public Task DiskCopyLisaAndGenericTaggedGeometry(int count) => AppleContainerScenarios.LisaDiskCopy(count);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] public Task Woz2TrackBlocksCrcAndInvalidReferences(int damage) => AppleContainerScenarios.Woz2(damage);
    [Theory] [InlineData(false,false)] [InlineData(true,false)] [InlineData(false,true)] [InlineData(true,true)] public Task AppleFacadeEncodesStandardAndRwts18(bool woz,bool rwts) => AppleContainerScenarios.WriterFacade(woz,rwts);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void ScpMultipleRevolutionsOverflowAndUnsupportedHeaders(int damage) => ScpContainerScenarios.Revolutions(damage);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public Task CpcInvalidSignatureGeometryAndOffsets(int damage) => AmstradContainerScenarios.Invalid(damage);
    [Fact] public Task HfeChunkBoundariesSingleSideAndCancellation() => HfeContainerScenarios.Chunks();
    [Theory] [InlineData(0)] [InlineData(2)] [InlineData(4)] [InlineData(16)] [InlineData(32)]
    public Task TeleDiskCommentsSectorFlagsAndCancellation(byte flags) => TeleDiskContainerScenarios.Flags(flags);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] public void TeleDiskSectorEncodingKnownBytes(int encoding) => TeleDiskContainerScenarios.Encodings(encoding);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public Task Cp2AngularOrderingAndInvalidDescriptors(int damage) => Cp2ContainerScenarios.Ordering(damage);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public Task I86fTwoSidedExplicitBitCountAndInvalidRanges(int damage) => I86fContainerScenarios.Extended(damage);
    [Theory]
    [InlineData(false,false,false)] [InlineData(true,false,false)] [InlineData(false,true,false)] [InlineData(true,true,false)]
    [InlineData(false,false,true)] [InlineData(true,false,true)] [InlineData(false,true,true)] [InlineData(true,true,true)]
    public Task RawInterpretationsPrioritizeFatThenCpcThenPcw(bool directory,bool specification,bool fat) => RawContainerScenarios.Interpretation(directory,specification,fat);
    [Theory]
    [InlineData(0,327680,1280,40,2,256,16)] [InlineData(1,409600,400,40,2,1024,5)]
    [InlineData(2,65024,254,15,1,256,17)] [InlineData(3,408576,806,40,2,512,10)]
    [InlineData(4,405504,824,40,2,512,10)] [InlineData(5,382976,796,40,2,512,10)]
    public Task EpsonVariableLayoutsAndMissingSector(int kind,int capacity,int blocks,int cylinders,int heads,int size,int sector) => EpsonContainerScenarios.Variant(kind,capacity,blocks,cylinders,heads,size,sector);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public Task ImdFmAndMfmDataRatesArePreserved(int mode) => ImageDiskContainerScenarios.Mode(mode);
    [Theory] [InlineData(false,13)] [InlineData(false,16)] [InlineData(true,13)] [InlineData(true,16)]
    public Task AppleBitContainersCarryDecodedSector(bool woz,int sectors) => AppleContainerScenarios.BitContainer(woz,sectors);
    [Theory] [InlineData(409600,false)] [InlineData(819200,false)] [InlineData(1474560,false)] [InlineData(409600,true)] [InlineData(819200,true)]
    public Task MacintoshRawDiskCopyHeadersChecksumsAndTags(int capacity,bool tagged) => AppleContainerScenarios.Macintosh(capacity,tagged);
    [Theory] [InlineData(128,720,"atari.90")] [InlineData(128,1040,"atari.130")] [InlineData(256,720,"atari.180")]
    public Task AtrBootSectorSizesAndHeader(int size,int count,string format) => AtariContainerScenarios.Atr(size,count,format);
    [Theory] [InlineData(false)] [InlineData(true)] public Task MsaRawAndRleTracks(bool compressed) => AtariContainerScenarios.Msa(compressed);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void MsaRleEscapesMarkersAndRejectsMalformedRuns(int damage) => AtariContainerScenarios.Rle(damage);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
    public Task ImdWriterPreservesRecordStatusAndEncoding(int type) => ImageDiskContainerScenarios.Write(type);
    [Fact] public Task ImdWriterPreservesCylinderHeadAndExplicitSizeMaps() => ImageDiskContainerScenarios.Maps();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] public Task LinearWritesRequireEveryExpectedBlock(int failure)=>RawContainerScenarios.Write(failure);
    [Theory] [InlineData(false)] [InlineData(true)] public Task AppleRawAndTwoImgPayload(bool container) => AppleContainerScenarios.Read(container);
    [Fact] public Task Cp2SectorDescriptorsAndTruncation() => Cp2ContainerScenarios.Read();
    [Theory] [InlineData(false)] [InlineData(true)] public Task I86fBitOrder(bool reverse) => I86fContainerScenarios.Read(reverse);
    [Fact] public Task CoherentCanonicalSizeAndPayload() => CoherentContainerScenarios.Read();
    [Theory] [InlineData(false)] [InlineData(true)] public Task CpcContainerMapsSectorDescriptors(bool extended) => AmstradContainerScenarios.Read(extended);
    [Fact] public Task DecInterleaveMapsPhysicalSectors() => DecContainerScenarios.Read();
    [Fact] public void TeleDiskDetailedRecordsAndCorruption() => TeleDiskContainerScenarios.ReadWrite();
    [Fact] public void HfeSidePackingAndWriter() => HfeContainerScenarios.ReadWrite();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void HfeInvalidHeaderOrTrack(int variant) => HfeContainerScenarios.Invalid(variant);
    [Theory]
    [InlineData(64,174848,683,1)]
    [InlineData(71,349696,1366,2)]
    [InlineData(81,819200,3200,1)]
    public Task CommodoreReadersPreserveLogicalBlocks(int kind,int length,int count,int heads)=>CommodoreContainerScenarios.Read(kind,length,count,heads);
    [Theory]
    [InlineData(1,35,false)] [InlineData(1,35,true)] [InlineData(1,40,false)] [InlineData(1,40,true)]
    [InlineData(2,35,false)] [InlineData(2,35,true)] [InlineData(2,40,false)] [InlineData(2,40,true)]
    public Task CommodoreExtendedTracksAndErrorMaps(int heads,int tracks,bool map) => CommodoreContainerScenarios.Diagnostics(heads,tracks,map);
    [Theory]
    [InlineData(184320,40,1,(byte)0xfc)]
    [InlineData(368640,80,1,(byte)0xf8)]
    [InlineData(368640,40,2,(byte)0xf9)]
    [InlineData(737280,80,2,(byte)0xf9)]
    public Task MsxReaderUsesBootGeometry(int size,int cylinders,int heads,byte descriptor)=>MsxContainerScenarios.Read(size,cylinders,heads,descriptor);
    [Fact] public Task EpsonReaderUsesSelectedGeometry()=>EpsonContainerScenarios.Read();
    [Theory]
    [InlineData(".ssd",102400,40,1,10,256)]
    [InlineData(".ssd",204800,80,1,10,256)]
    [InlineData(".dsd",204800,40,2,10,256)]
    [InlineData(".dsd",409600,80,2,10,256)]
    public Task AcornReaderBuildsExpectedSectors(string extension,int length,int cylinders,int heads,int sectors,int blockSize)=>AcornContainerScenarios.Read(extension,length,cylinders,heads,sectors,blockSize);
    [Theory]
    [InlineData(".adf",819200,80,2,5,1024)]
    [InlineData(".adf",901120,80,2,11,512)]
    [InlineData(".adf",1802240,80,2,22,512)]
    public Task AdfReaderBuildsExpectedSectors(string extension,int length,int cylinders,int heads,int sectors,int blockSize)=>AdfContainerScenarios.Read(extension,length,cylinders,heads,sectors,blockSize);
    [Theory]
    [InlineData(".img",163840,40,1,8,512)]
    [InlineData(".img",1474560,80,2,18,512)]
    [InlineData(".ima",184320,40,1,9,512)]
    [InlineData(".img",327680,40,2,8,512)]
    [InlineData(".ima",368640,40,2,9,512)]
    [InlineData(".img",737280,80,2,9,512)]
    [InlineData(".ima",819200,80,2,10,512)]
    [InlineData(".img",1228800,80,2,15,512)]
    [InlineData(".ima",1720320,80,2,21,512)]
    [InlineData(".img",2949120,80,2,36,512)]
    public Task IbmReaderBuildsExpectedSectors(string extension,int length,int cylinders,int heads,int sectors,int blockSize)=>IbmContainerScenarios.Read(extension,length,cylinders,heads,sectors,blockSize);
    [Theory] [InlineData(false,false)] [InlineData(false,true)] [InlineData(true,false)] [InlineData(true,true)]
    public Task IbmBpbOverridesCapacityOnlyWhenConsistent(bool largeCount,bool valid) => IbmContainerScenarios.Bpb(largeCount,valid);
    [Fact] public Task IbmExplicitDmfUsesCompatibleGeometry() => IbmContainerScenarios.Dmf();
    [Theory]
    [InlineData(".img",163840,40,1,8,512)]
    public Task RawReaderBuildsExpectedSectors(string extension,int length,int cylinders,int heads,int sectors,int blockSize)=>RawContainerScenarios.Read(extension,length,cylinders,heads,sectors,blockSize);
    [Theory]
    [InlineData(".img",163840,40,1,8,512)]
    public Task UcsdReaderBuildsExpectedSectors(string extension,int length,int cylinders,int heads,int sectors,int blockSize)=>UcsdContainerScenarios.Read(extension,length,cylinders,heads,sectors,blockSize);
    [Theory]
    [InlineData(".st",737280,80,2,9,512)]
    public Task AtariReaderBuildsExpectedSectors(string extension,int length,int cylinders,int heads,int sectors,int blockSize)=>AtariContainerScenarios.Read(extension,length,cylinders,heads,sectors,blockSize);
    [Theory]
    [InlineData(184320,40,1,9)] [InlineData(368640,40,2,9)] [InlineData(409600,80,1,10)]
    [InlineData(450560,80,1,11)] [InlineData(737280,80,2,9)] [InlineData(819200,80,2,10)]
    [InlineData(829440,90,2,9)] [InlineData(901120,80,2,11)] [InlineData(1474560,80,2,18)]
    public Task AtariBpbGeometryRoundTrips(int capacity,int cylinders,int heads,int sectors) => AtariContainerScenarios.Read(".st",capacity,cylinders,heads,sectors,512,true);
    [Fact] public Task ScpWriterProducesExpectedTrackTableFluxAndChecksum()=>ScpContainerScenarios.Write();
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ScpReaderRejectsInvalidHeader(int change)=>ScpContainerScenarios.Invalid(change);
    [Fact] public Task ScpWriterRejectsCancellationAndInvalidTracksBeforeWriting()=>ScpContainerScenarios.Reject();
    [Fact] public void ImdCompressedSectorExpandsToExpectedData()=>ImageDiskContainerScenarios.Compressed();
    [Theory]
    [InlineData(new byte[]{})]
    [InlineData(new byte[]{73,77,68,26,0,0})]
    [InlineData(new byte[]{73,77,68,26,9,0,0,1,0,1,2,42})]
    [InlineData(new byte[]{73,77,68,26,0,0,0,1,0,1,2})]
    public void ImdRejectsTruncatedOrInvalidData(byte[] bytes)=>ImageDiskContainerScenarios.Invalid(bytes);
}
