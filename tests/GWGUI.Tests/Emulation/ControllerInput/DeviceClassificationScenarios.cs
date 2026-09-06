using GWGUI.App.Services.Input.GameInput;
namespace GWGUI.Tests.Emulation.ControllerInput;
internal static class DeviceClassificationScenarios
{
    public static void Duplicates()
    {
        GameInputDeviceDescriptor Describe(string id, string name, ushort vendor, ushort product) => new(id,name,name,"virtual-pnp",vendor,product,0,default,default,"root",Guid.Empty,default,new(){Page=1,Id=5},GameInputKind.Gamepad,default,default,"synthetic",[],[],null!,[],[],[],false,"",[],ControllerVisualModel.GenericGamepad,false);
        var primary = Describe("primary","Same Pad",42,93);
        var duplicate = Describe("fallback-one","Same Pad",42,93);
        var secondPad = duplicate with { Id="fallback-two" };
        var unknown = Describe("unknown","different",0,0);
        var distinct = RawGameControllerFallback.DistinctFallback([primary],new[]{duplicate,secondPad,unknown},item=>item);
        Assert.Equal(new[]{secondPad,unknown},distinct);
        Assert.Equal(new[]{duplicate,secondPad,unknown},RawGameControllerFallback.DistinctFallback([],new[]{duplicate,secondPad,unknown},item=>item));
        Assert.Empty(RawGameControllerFallback.DistinctFallback([primary],new[]{duplicate},item=>item));
        Assert.Empty(RawGameControllerFallback.DistinctFallback([primary],new[]{Describe("fallback"," same  PAD ",0,0)},item=>item));
        Assert.Empty(RawGameControllerFallback.DistinctFallback([primary],Array.Empty<GameInputDeviceDescriptor>(),item=>item));
    }
    public static void Known(ushort vendor, ushort product, string model, bool exact)
    {
        var result = GameInputDeviceModelCatalog.ResolveVisualModel(vendor,product,"unrelated synthetic name",GameInputKind.Unknown);
        Assert.Equal(model,result.Model.ToString()); Assert.Equal(exact,result.Exact);
        var name = GameInputDeviceModelCatalog.ResolveProductName(vendor,product,"wrong windows name","wrong database name","wrong runtime name");
        Assert.False(string.IsNullOrWhiteSpace(name)); Assert.DoesNotContain("wrong",name);
        Assert.Contains(result.Model,GameInputDeviceModelCatalog.AllVisualModels);
    }
    public static void Fallbacks()
    {
        Assert.Equal("Window Device",GameInputDeviceModelCatalog.ResolveProductName(0xfffe,1," Window Device ","Database Device","Runtime Device"));
        Assert.Equal("Database Device",GameInputDeviceModelCatalog.ResolveProductName(0xfffe,1,"","Database Device","Runtime Device"));
        Assert.Equal("Runtime Device",GameInputDeviceModelCatalog.ResolveProductName(0xfffe,1,null,null,"Runtime Device"));
        Assert.Equal("Controller FFFE:0001",GameInputDeviceModelCatalog.ResolveProductName(0xfffe,1,null,null,null));
        Assert.Equal((ControllerVisualModel.GenericGamepad,false),GameInputDeviceModelCatalog.ResolveVisualModel(0x081f,0xe401,"DualSense",GameInputKind.Gamepad));
        Assert.Equal((ControllerVisualModel.RacingWheel,false),GameInputDeviceModelCatalog.ResolveVisualModel(0xfffe,1,"synthetic",GameInputKind.RacingWheel));
        Assert.Equal((ControllerVisualModel.FlightStick,false),GameInputDeviceModelCatalog.ResolveVisualModel(0xfffe,1,"synthetic",GameInputKind.FlightStick));
        Assert.Equal((ControllerVisualModel.ArcadeStick,false),GameInputDeviceModelCatalog.ResolveVisualModel(0xfffe,1,"synthetic",GameInputKind.ArcadeStick));
    }
    public static void Classification(uint kind, ushort page, ushort usage, bool expected)
    {
        var descriptor = new GameInputDeviceDescriptor("virtual", "synthetic", "synthetic", "virtual-pnp", 0xfffe,1,0,default,default,"root",Guid.Empty,default,new() { Page = page, Id = usage },(GameInputKind)kind,default,default,"synthetic",[],[],null!,[],[],[],false,"",[],ControllerVisualModel.GenericGamepad,false);
        Assert.Equal(expected,GameInputDeviceClassifier.IsGamingController(descriptor)); Assert.Equal("FFFE:0001",descriptor.VidPid);
        Assert.Equal(expected,GameInputDeviceClassifier.IsGamingController(descriptor with { SupportedInput = descriptor.SupportedInput | GameInputKind.Keyboard | GameInputKind.Mouse }));
    }
}
