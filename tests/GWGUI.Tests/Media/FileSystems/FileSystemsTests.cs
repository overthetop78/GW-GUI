using GWGUI.MediaEngine.FileSystems.Amiga;
namespace GWGUI.Tests.Media.FileSystems;
public class FileSystemsTests
{
    [Theory] [InlineData(true,0)] [InlineData(false,0)] [InlineData(true,1)] [InlineData(false,2)] [InlineData(true,3)] [InlineData(false,4)]
    public void AppleDos32And33ContentsEmptyAndBrokenLists(bool thirteen,int damage) => AppleFileSystemScenarios.Dos(thirteen,damage);
    [Theory] [InlineData(1,0)] [InlineData(2,0)] [InlineData(3,0)] [InlineData(1,1)] [InlineData(2,1)] [InlineData(2,2)] [InlineData(3,3)]
    public void ProDosSeedlingSaplingTreeSparseAndInvalidBlocks(int storage,int damage) => AppleFileSystemScenarios.ProDosStorage(storage,damage);
    [Theory] [InlineData(14,0)] [InlineData(15,0)] [InlineData(17,0)] [InlineData(14,1)] [InlineData(15,2)] [InlineData(17,3)]
    public void LisaCatalogVariantsFragmentedDuplicateAndMissingPages(ushort version,int damage) => AppleFileSystemScenarios.Lisa(version,damage);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] public void InformStoryHeaderChecksumAndContents(int damage) => AppleFileSystemScenarios.Inform(damage);
    [Theory] [InlineData(0,0)] [InlineData(1,0)] [InlineData(2,0)] [InlineData(3,0)] [InlineData(4,0)] [InlineData(5,0)] [InlineData(6,0)] [InlineData(7,0)]
    [InlineData(0,1)] [InlineData(1,1)] [InlineData(0,2)] [InlineData(1,2)] [InlineData(0,3)] [InlineData(1,3)] [InlineData(0,4)]
    public void AmigaDosVariantDataAndBrokenExtensions(byte variant,int damage) => AmigaFileSystemScenarios.FileBlocks(variant,damage);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] public void AmigaFlatArchiveContentsAndMissingBlocks(int damage) => AmigaFileSystemScenarios.Archive(damage);
    [Theory] [InlineData("commodore.1541",0)] [InlineData("commodore.1571",0)] [InlineData("commodore.1581",0)]
    [InlineData("commodore.1541",1)] [InlineData("commodore.1541",2)] [InlineData("commodore.1571",3)] [InlineData("commodore.1581",4)]
    public void CommodoreFragmentedAndBrokenSectorChains(string format,int damage) => CommodoreFileSystemScenarios.Chains(format,damage);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void Fat12FragmentedFileNestedDirectoryAndInvalidAllocation(int damage) => Fat12FileSystemScenarios.Fragmented(damage);
    [Theory] [InlineData(false,0)] [InlineData(true,0)] [InlineData(false,1)] [InlineData(true,1)]
    [InlineData(false,2)] [InlineData(true,2)] [InlineData(false,3)] [InlineData(true,3)] [InlineData(false,4)] [InlineData(true,4)]
    public void AdfsOldAndNewMapsFragmentationCyclesAndMissingBlocks(bool newMap,int damage) => AcornFileSystemScenarios.Adfs(newMap,damage);
    [Theory] [InlineData(6,0)] [InlineData(10,0)] [InlineData(6,1)] [InlineData(6,2)] [InlineData(6,3)] [InlineData(6,4)] [InlineData(6,5)]
    public void UcsdDirectorySizesSeparateExtentsAndInvalidRanges(int end,int damage) => UcsdFileSystemScenarios.Directory(end,damage);
    [Theory]
    [InlineData("atari.90",128,0)] [InlineData("atari.130",128,0)] [InlineData("atari.180",256,0)]
    [InlineData("atari.90",128,1)] [InlineData("atari.90",128,2)] [InlineData("atari.90",128,3)] [InlineData("atari.90",128,4)] [InlineData("atari.90",128,5)]
    [InlineData("atari.90",128,6)] [InlineData("atari.180",256,6)]
    public void AtariDosVariantsFragmentationEmptyAndMalformedChains(string format,int size,int damage) => AtariFileSystemScenarios.Variant(format,size,damage);
    [Theory]
    [InlineData(0xc1,false,0)] [InlineData(0x41,false,0)] [InlineData(1,false,0)] [InlineData(1,true,0)]
    [InlineData(0xc1,false,1)] [InlineData(0xc1,false,2)] [InlineData(0xc1,false,3)]
    [InlineData(0xc1,false,4)] [InlineData(1,true,4)]
    public void AmstradCpmLayoutsExtentsAndInvalidAllocations(int firstSector,bool pcw,int damage) => CpmFileSystemScenarios.Amstrad(firstSector,pcw,damage);
    [Theory]
    [InlineData("epson.qx10.320",32768)] [InlineData("epson.qx10.396",16384)] [InlineData("epson.qx10.399",4096)]
    [InlineData("epson.qx10.400",20480)] [InlineData("epson.qx10.logo",16384)]
    public void EpsonCpmLayoutsAndMissingAllocation(string format,int origin) => CpmFileSystemScenarios.Volume(format,origin,2048,64,false);
    [Theory] [InlineData(false,0)] [InlineData(true,0)] [InlineData(false,1)] [InlineData(false,2)]
    public void MacintoshHfsCatalogAndFragmentedForks(bool resourceFork,int damage) => MacintoshFileSystemScenarios.Hfs(resourceFork,damage);
    [Theory] [InlineData(false,0)] [InlineData(true,0)] [InlineData(false,1)] [InlineData(false,2)] [InlineData(false,3)]
    public void MacintoshMfsForksAndFragmentedChains(bool resourceFork,int damage) => MacintoshFileSystemScenarios.Mfs(resourceFork,damage);
    [Theory] [InlineData("commodore.1541",2560,1024,64,false)] [InlineData("commodore.1571",2560,2048,128,true)] [InlineData("commodore.1581",0,2048,128,true)]
    public void CpmAllocationWidthsAndUserAreas(string format,int origin,int allocationSize,int entryCount,bool wide) => CpmFileSystemScenarios.Volume(format,origin,allocationSize,entryCount,wide);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] public void CoherentInodesAndInvalidReferences(int damage) => CoherentFileSystemScenarios.Volume(damage);
    [Theory] [InlineData("commodore.1541", 174848)] [InlineData("commodore.1571", 349696)] [InlineData("commodore.1581", 819200)]
    public void CommodoreVolumeVariantsAndCapacity(string format, long capacity) => CommodoreFileSystemScenarios.Volume(format, capacity);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public void Rt11ContentsAndInvalidDirectorySegments(int damage) => DecFileSystemScenarios.Rt11(damage);
    [Theory] [InlineData(false)] [InlineData(true)] public void UcsdByteOrderAndFileContents(bool bigEndian) => UcsdFileSystemScenarios.Volume(bigEndian);
    [Theory] [InlineData(false)] [InlineData(true)] public void BbcDfsCatalogAndMissingContent(bool missing) => AcornFileSystemScenarios.Dfs(missing);
    [Theory] [InlineData(false)] [InlineData(true)] public void AtariDosCatalogAndBrokenChain(bool broken) => AtariFileSystemScenarios.Dos(broken);
    [Fact] public void Fat12WriterBuildsBootAndDirectoryWithExpectedContent()=>Fat12FileSystemScenarios.Volume();
    [Theory]
    [InlineData(AmigaDosVariant.Ofs)]
    [InlineData(AmigaDosVariant.Ffs)]
    public void AmigaDosWriterAndReaderPreserveFile(AmigaDosVariant variant)=>AmigaFileSystemScenarios.Volume(variant);
    [Fact] public void ProDosWriterAndReaderPreserveFile()=>AppleFileSystemScenarios.Volume();
    [Fact] public void SosWriterAddsItsBootMarkerAndPreservesFile()=>SosFileSystemScenarios.Volume();
}
