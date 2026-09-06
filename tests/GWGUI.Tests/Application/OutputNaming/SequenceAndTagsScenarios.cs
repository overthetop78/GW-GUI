using GWGUI.Domain.Naming;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
namespace GWGUI.Tests.Application.OutputNaming;
internal static class SequenceAndTagsScenarios
{
    public static void Sequence(long value,SequenceKind kind,int width,string expected)
    {
        Assert.Equal(expected,SequenceFormatter.Format(value,kind,width));
        Assert.True(SequenceFormatter.TryParse(expected,kind,out var parsed));
        Assert.Equal(value,parsed);
    }
    public static void Invalid(string text,SequenceKind kind)=>Assert.False(SequenceFormatter.TryParse(text,kind,out _));
    public static void Bounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(()=>SequenceFormatter.Format(-1,SequenceKind.Numeric,1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SequenceFormatter.Format(1,SequenceKind.Numeric,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SequenceFormatter.Format(1,SequenceKind.Numeric,17));
    }
    public static void Tags()
    {
        var format=new DiskFormat("test.raw","test","synthetic",[],Tag:"TEST-RAW");
        var stamp=new DateTime(2026,9,6,12,34,56);
        Assert.Equal("disk-TEST-RAW-SCP-20260906-123456",
            ConversionTagFormatter.Format("{name}-{family}-{format}-{extension}-{DATE:YYYYMMDD}-{TIME:HHMMSS}",format,".scp","disk",stamp));
        Assert.Throws<ArgumentException>(()=>ConversionTagFormatter.Format("literal",format,".scp","disk",stamp));
    }
}
