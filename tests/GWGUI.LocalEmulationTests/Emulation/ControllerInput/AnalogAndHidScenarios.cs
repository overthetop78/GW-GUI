using GWGUI.App.Functions.Input.Controllers;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.Emulation.Contracts;
namespace GWGUI.Tests.Emulation.ControllerInput;
internal static class AnalogAndHidScenarios
{
    public static void Report(int variant)
    {
        var id=variant>=2?(byte)7:(byte)0; var calls=0;
        var bindings=new HidReportDecoder.Binding[]{new(new(GameInputControlType.Axis,0,default),id,1,0x30,0,8,-128,127,0),new(new(GameInputControlType.Switch,1,default),id,1,0x39,0,8,1,8,0),new(new(GameInputControlType.Button,2,default),id,9,2,0,1,0,1,3)};
        byte[] raw=variant==0?[128,2,1]:variant==1?[0,128,2,1,99]:[variant==3?(byte)8:id,128,2,1];
        void Observe(byte[] report) { calls++; Assert.Equal(new byte[]{id,128,2,1},report); }
        using var decoder=new HidReportDecoder(4,bindings,(binding,report)=>{Observe(report); return (variant!=4,binding.Usage==0x30?128u:2u);},(binding,report)=>{Observe(report); Assert.Equal((ushort)2,binding.Usage); return (variant!=4,new ushort[]{1,2});});
        Assert.Equal(bindings.Select(binding=>binding.Descriptor),decoder.Controls);
        var result=decoder.Decode(raw); Assert.Equal(3,result.Count);
        if(variant is 3 or 4) { Assert.Equal(.5f,result[0].Value); Assert.Equal(GameInputSwitchPosition.Center,result[1].SwitchPosition); Assert.False(result[2].IsPressed); }
        else { Assert.Equal(0f,result[0].Value); Assert.Equal(GameInputSwitchPosition.UpRight,result[1].SwitchPosition); Assert.True(result[2].IsPressed); }
        Assert.Equal(variant==3?0:3,calls); var previous=calls;
        Assert.Equal(decoder.NeutralControls(),decoder.Decode([])); decoder.Dispose(); Assert.Equal(decoder.NeutralControls(),decoder.Decode(raw)); Assert.Equal(previous,calls);
    }
    public static void Signed(uint raw, ushort bits, bool signed, int expected) => Assert.Equal(expected, HidReportDecoder.SignExtend(raw,bits,signed));
    public static void Normalize(int raw, int minimum, int maximum, float expected) => Assert.Equal(expected, HidReportDecoder.Normalize(raw,minimum,maximum));
    public static void Hat()
    {
        var expected = new[] { GameInputSwitchPosition.Up, GameInputSwitchPosition.UpRight, GameInputSwitchPosition.Right, GameInputSwitchPosition.DownRight, GameInputSwitchPosition.Down, GameInputSwitchPosition.DownLeft, GameInputSwitchPosition.Left, GameInputSwitchPosition.UpLeft };
        for (var index = 0; index < expected.Length; index++) { Assert.Equal(expected[index], HidReportDecoder.DecodeHat(index,0)); Assert.Equal(expected[index], HidReportDecoder.DecodeHat(index+1,1)); }
        Assert.Equal(GameInputSwitchPosition.Center, HidReportDecoder.DecodeHat(-1,0)); Assert.Equal(GameInputSwitchPosition.Center, HidReportDecoder.DecodeHat(8,0));
    }
    public static void DeadZones()
    {
        var state=new EmulationControllerState(1u|1u<<12|1u<<13,100,100,32767,0,100,32767){DeviceId="virtual"};
        var result=ControllerAnalogDeadZoneFunctions.Apply(state,new(20,20,10));
        Assert.Equal(0,result.LeftX);Assert.Equal(0,result.LeftY);
        Assert.Equal(short.MaxValue,result.RightX);
        Assert.Equal(0,result.LeftTrigger);Assert.Equal(short.MaxValue,result.RightTrigger);
        Assert.Equal(1u|1u<<13,result.Buttons);
        Assert.Equal("virtual",result.DeviceId);
        Assert.Equal(new ControllerAnalogDeadZoneProfile(0,50,30),new ControllerAnalogDeadZoneProfile(-1,99,99).Normalize());
    }
}
