using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
namespace GWGUI.Tests.Media.Conversion;
internal static class ConversionPlanScenarios
{
    private sealed class Catalog:IImageFormatCatalog
    {
        public IReadOnlyList<DiskFormat> Formats{get;}=[new("synthetic","test","test",[new(".scp","scp",true),new(".hfe","hfe")],Tag:"TEST-RAW")];
        public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string extension)=>extension==".img"?Formats:[];
    }
    public static void Plan()
    {
        var planner=new ConversionPlanner(new Catalog());
        var result=Assert.Single(planner.Plan("virtual.img","destination","disk",[new("synthetic",new HashSet<string>())],false));
        Assert.Equal(Path.Combine("destination","disk.scp"),result.OutputPath);
        Assert.True(result.UsesImplicitExtension);
        var explicitResult=Assert.Single(planner.Plan("virtual.img","destination","disk",[new("synthetic",new HashSet<string>{".HFE"})],true));
        Assert.Equal(Path.Combine("destination","[TEST-RAW] disk.hfe"),explicitResult.OutputPath);
        Assert.False(explicitResult.UsesImplicitExtension);
        Assert.Throws<InvalidOperationException>(()=>planner.Plan("virtual.img","destination","disk",[new("synthetic",new HashSet<string>{".bad"})],false));
        Assert.Throws<InvalidOperationException>(()=>planner.Plan("virtual.bad","destination","disk",[new("synthetic",new HashSet<string>())],false));
        Assert.Throws<InvalidOperationException>(()=>planner.Plan("virtual.img","destination","disk",[new("synthetic",new HashSet<string>()),new("synthetic",new HashSet<string>())],false));
    }
    public static void Multiple()
    {
        var planner = new ConversionPlanner(new Catalog());
        var outputs = planner.Plan("virtual.img", "destination", "disk", [new("synthetic", new HashSet<string>{".scp", ".hfe"})], true, "{NAME}-[{FAMILY}-{FORMAT}] ");
        Assert.Equal(2, outputs.Count);
        Assert.Equal(new[]{".hfe", ".scp"}, outputs.Select(x=>x.Extension).Order().ToArray());
        Assert.All(outputs, output =>
        {
            Assert.Equal(Path.Combine("destination", "disk-[TEST-RAW] " + output.Extension), output.OutputPath);
            Assert.False(output.UsesImplicitExtension);
            Assert.Equal(ConversionFidelityLevel.ReconstructedTracks, output.Fidelity);
            Assert.False(output.PreservesOriginalProtection);
        });
        Assert.Empty(planner.Plan("virtual.img", "destination", "disk", [], false));
        Assert.Throws<InvalidOperationException>(()=>planner.Plan("virtual.img", "destination", "disk", [new("missing", new HashSet<string>())], false));
    }
    public static void Fidelity(string source, string target, ConversionFidelityLevel expected)
    {
        var actual = ConversionFidelity.ForConversion(source, target);
        Assert.Equal(expected, actual);
        Assert.Equal(expected == ConversionFidelityLevel.PreservedFlux, ConversionFidelity.PreservesOriginalProtection(actual));
    }
}
