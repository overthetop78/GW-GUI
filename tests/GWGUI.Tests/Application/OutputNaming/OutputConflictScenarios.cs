using GWGUI.Domain.Naming;
namespace GWGUI.Tests.Application.OutputNaming;
internal static class OutputConflictScenarios
{
    public static void Next()
    {
        var calls=new List<string>();
        var result=OutputConflictResolver.FindNextAvailableWithValue("virtual","disk",".scp",SequenceKind.Numeric,2,7,
            path=>{calls.Add(path); return calls.Count<=2;});
        Assert.Equal(new[]{Path.Combine("virtual","disk 07.scp"),Path.Combine("virtual","disk 08.scp"),Path.Combine("virtual","disk 09.scp")},calls);
        Assert.Equal(9,result.Value);
        Assert.Equal(Path.Combine("virtual","disk 09.scp"),result.Path);
    }
    public static void Exhausted()
    {
        var count=0;
        Assert.Throws<IOException>(()=>OutputConflictResolver.FindNextAvailableWithValue("virtual","disk",".scp",SequenceKind.Numeric,1,long.MaxValue-1,
            _=>{count++;return true;}));
        Assert.Equal(1,count);
    }
}
