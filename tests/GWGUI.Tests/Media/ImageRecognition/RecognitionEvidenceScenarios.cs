using System.Buffers.Binary;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.ImageRecognition;
internal static class RecognitionEvidenceScenarios
{
    public static async Task Registry(int failure)
    {
        var bytes=new byte[143424]; "2IMG"u8.CopyTo(bytes); bytes[8]=64; bytes[10]=1; bytes[12]=1; bytes[24]=64; BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(28),143360); bytes[64]=42;
        if(failure==1) bytes[10]=2;
        int reads=0, fallback=0; var error=new IOException("synthetic read failure");
        var context=new DiskImageRecognitionContext("source.WRONG",null,bytes.Length,token=>{reads++; token.ThrowIfCancellationRequested(); if(failure==2) throw error; return Task.FromResult(bytes);});
        var fallbackImage=new SectorImage("fallback",128,1,1,1,[new(0,new(0,0,0),new byte[128])]);
        var registry=new DiskImageRecognitionRegistry([new AppleImageRecognitionPolicy(new()),new ExtensionHintRecognitionPolicy((path,token)=>{fallback++;Assert.Equal(context.Path,path);return Task.FromResult(fallbackImage);},".wrong")]);
        if(failure==2)
        {
            Assert.Same(error,await Assert.ThrowsAsync<IOException>(()=>registry.ReadAsync(context,CancellationToken.None)));
            Assert.Same(error,await Assert.ThrowsAsync<IOException>(()=>context.ReadBytesAsync())); Assert.Equal(1,reads); Assert.Equal(0,fallback); return;
        }
        var image=await registry.ReadAsync(context,CancellationToken.None); Assert.Equal(1,reads); Assert.Equal(failure,fallback);
        if(failure==1) Assert.Same(fallbackImage,image); else { Assert.Equal("apple2.prodos",image.FormatId); Assert.Equal(42,image.AvailableBlocks.Single(b=>b.LogicalBlock==0).Data[0]); }
        Assert.Equal(bytes,(await context.ReadBytesAsync()).ToArray()); Assert.Equal(1,reads);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>registry.ReadAsync(context,new CancellationToken(true)));
    }

    public static async Task Rejected(bool candidate)
    {
        var context=new DiskImageRecognitionContext("source.po",null,1,_=>Task.FromResult(new byte[]{0}));
        var registry=new DiskImageRecognitionRegistry(candidate?[new AppleImageRecognitionPolicy(new())]:[]);
        if(candidate) await Assert.ThrowsAsync<DiskImageCandidatesRejectedException>(()=>registry.ReadAsync(context,CancellationToken.None));
        else await Assert.ThrowsAnyAsync<NotSupportedException>(()=>registry.ReadAsync(context,CancellationToken.None));
    }

    internal static readonly BuiltInImageFormatCatalog Catalog = new(key => key);
    public static void Size(string extension, long size, string expected)
    {
        var result = new ImageFormatDetector(Catalog, _ => throw new InvalidOperationException("No header should be needed.")).Detect("virtual" + extension, size);
        Assert.Equal(expected, result.Format?.Id); Assert.False(result.RequiresUserChoice);
        Assert.Equal(FormatConfidence.Certain, result.Confidence); Assert.Equal(extension.ToLowerInvariant(), result.Extension);
        Assert.All(result.Candidates, format => Assert.Contains(format.Extensions, e => e.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)));
    }
    public static void Header(string kind, bool valid)
    {
        var data = new byte[kind == "msa" ? 10 : 1026];
        if (kind == "msa")
        {
            BinaryPrimitives.WriteUInt16BigEndian(data, valid ? (ushort)0x0e0f : (ushort)0);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(2), 9); BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(4), 1);
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(8), 79);
        }
        else BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(1024), valid ? (ushort)0x4244 : (ushort)0);
        var opens = 0; MemoryStream? stream = null;
        var result = new ImageFormatDetector(Catalog, path => { Assert.Equal("virtual." + (kind == "msa" ? "msa" : "img"), path); opens++; return stream = new MemoryStream(data, false); })
            .Detect("virtual." + (kind == "msa" ? "msa" : "img"), kind == "msa" ? 10 : 819200);
        Assert.Equal(1, opens); Assert.False(stream!.CanRead);
        Assert.Equal(kind == "msa" ? valid ? "atarist.720" : null : valid ? "mac.800" : "ibm.800", result.Format?.Id);
        Assert.Equal(kind == "msa" && !valid, result.RequiresUserChoice);
        if (kind != "msa") Assert.Contains(result.Candidates, x => x.Id == "ibm.800");
    }
    public static void MissingHeader(bool denied)
    {
        var detector = new ImageFormatDetector(Catalog, _ => throw (denied ? new UnauthorizedAccessException() : (Exception)new IOException()));
        var msa = detector.Detect("virtual.msa", 10); Assert.Null(msa.Format); Assert.True(msa.RequiresUserChoice);
        var img = detector.Detect("virtual.img", 819200); Assert.Equal("ibm.800", img.Format?.Id);
        var unknown = detector.Detect("virtual.unsupported", 123); Assert.Null(unknown.Format); Assert.Empty(unknown.Candidates);
    }
}
