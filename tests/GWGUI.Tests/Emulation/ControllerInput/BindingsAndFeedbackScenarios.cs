using GWGUI.Emulation.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.App.Functions.Input.Bindings;
using System.Windows.Input;
namespace GWGUI.Tests.Emulation.ControllerInput;
internal static class BindingsAndFeedbackScenarios
{
    public static void Mapping()
    {
        var keys=new HashSet<GWGUI.Emulation.Enums.EmulationKey>{GWGUI.Emulation.Enums.EmulationKey.A,GWGUI.Emulation.Enums.EmulationKey.B};
        var mapped=GWGUI.Emulation.Functions.EmulationInputMappingFunctions.MapKeyboard(keys,new Dictionary<string,GWGUI.Emulation.Enums.EmulationKey>{{"F1",GWGUI.Emulation.Enums.EmulationKey.A}});
        Assert.Equal(new[]{GWGUI.Emulation.Enums.EmulationKey.B,GWGUI.Emulation.Enums.EmulationKey.F1}.Order(),mapped.Order()); Assert.Contains(GWGUI.Emulation.Enums.EmulationKey.A,keys);
        var first=EmulationControllerState.Empty with{DeviceId="one"}; var second=new EmulationControllerState(1u<<8,short.MinValue,short.MaxValue,0,0,0,0){DeviceId="two"};
        Assert.Same(second,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ResolveController("TWO",[first,second],0));
        Assert.Same(first,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ResolveController("missing",[first,second],0));
        Assert.Same(EmulationControllerState.Empty,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ResolveController(null,[],0));
        Assert.Equal("device:identifier",GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ParseControllerDeviceId("Controller:device:identifier:ButtonA"));
        Assert.Null(GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ParseControllerDeviceId("Keyboard:A"));
        Assert.Equal(1,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ControllerSourceValue("Controller:two:ButtonA",second));
        Assert.True(GWGUI.Emulation.Functions.EmulationInputMappingFunctions.IsControllerSourcePressed("ButtonA",second));
        Assert.False(GWGUI.Emulation.Functions.EmulationInputMappingFunctions.IsControllerSourcePressed("ButtonB",second));
        Assert.Equal(32768,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ControllerSourceValue("LeftStickLeft",second));
        Assert.Equal(-32767,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ControllerSourceValue("LeftStickUp",second));
        Assert.False(GWGUI.Emulation.Functions.EmulationInputMappingFunctions.IsControllerSourcePressed("LeftStickRight",second,14000));
        Assert.True(GWGUI.Emulation.Functions.EmulationInputMappingFunctions.IsControllerSourcePressed("LeftStickRight",second,14001));
        Assert.False(GWGUI.Emulation.Functions.EmulationInputMappingFunctions.IsControllerSourcePressed("AxisPositive",second,.75f));
        Assert.True(GWGUI.Emulation.Functions.EmulationInputMappingFunctions.IsControllerSourcePressed("AxisPositive",second,.76f));
    }
    private sealed class Pointer : GWGUI.App.Input.Mouse.IRelativeMouseCapture
    {
        public bool IsCaptured { get; private set; }
        public int Captures, Releases;
        public void Capture(System.Windows.FrameworkElement display,System.Windows.FrameworkElement screen,IntPtr handle) { Captures++; IsCaptured=true; }
        public void Release(System.Windows.FrameworkElement display,IntPtr handle) { if(IsCaptured) Releases++; IsCaptured=false; }
        public void ProcessMovement(System.Windows.FrameworkElement screen,Action<int,int> moved) => throw new InvalidOperationException("Unexpected native movement");
    }
    public static void Routing()
    {
        var snapshots=new[] {new List<EmulationInputSnapshot>(),new List<EmulationInputSnapshot>()}; var active=true; var selected=0; var reads=0; var focus=0;
        var physical=new GWGUI.App.Services.Input.GameInput.GameInputPhysicalState(new HashSet<GWGUI.Emulation.Enums.EmulationKey>{GWGUI.Emulation.Enums.EmulationKey.A},new(12,-3,2,true,false,false),[]);
        var machines=Enumerable.Range(0,2).Select(index=>
        {
            var input=GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.Emulation.Interfaces.IEmulationInput>((method,args)=>method.Name switch
            {
                "get_KeyboardBindings"=>new Dictionary<string,string>{{"F1","Control+B"}},
                "get_SupportsPointerCapture" or "get_CapturePointerOnClick"=>true,
                "SetInput"=>Record(index,(EmulationInputSnapshot)args[0]!),
                _=>throw new InvalidOperationException(method.Name)
            });
            return GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.Emulation.Interfaces.IEmulatedMachine>((method,_)=>method.Name=="get_Input"?input:throw new InvalidOperationException(method.Name));
        }).ToArray();
        object? Record(int index,EmulationInputSnapshot value) { snapshots[index].Add(value); return null; }
        var pointer=new Pointer(); var view=new GWGUI.App.Views.Controls.Emulation.Machine.MachineView();
        using var controller=new GWGUI.App.Controllers.Emulation.Machine.MachineInputController(view,new System.Windows.Controls.Border(),IntPtr.Zero,()=>machines[selected],[],_=>Task.CompletedTask,()=>active,
            pointer,()=>{reads++; return physical;},(_,_)=>focus++,automaticPolling:false);
        controller.Publish(); Assert.Equal(0,reads);
        controller.SetPowered(true); controller.Publish();
        Assert.Equal(physical.Keys,snapshots[0][^1].Keys); Assert.False(snapshots[0][^1].Pointer.Left); Assert.Equal(0,snapshots[0][^1].Pointer.DeltaX);
        controller.RequestPointerCapture(); Assert.Equal(1,pointer.Captures); Assert.Equal(physical.Pointer,snapshots[0][^1].Pointer); Assert.Equal(1,focus);
        controller.HandleKeyDown(Key.B,ModifierKeys.Control); Assert.Contains(GWGUI.Emulation.Enums.EmulationKey.F1,snapshots[0][^1].Keys);
        controller.HandleKeyUp(Key.B,ModifierKeys.None); Assert.DoesNotContain(GWGUI.Emulation.Enums.EmulationKey.F1,snapshots[0][^1].Keys);
        active=false; controller.Deactivate(); Assert.Empty(snapshots[0][^1].Keys); Assert.Equal(1,pointer.Releases);
        var previousReads=reads; controller.Publish(); controller.RequestPointerCapture(); Assert.Equal(previousReads,reads); Assert.Equal(1,pointer.Captures);
        selected=1; active=true; controller.Publish(); Assert.Equal(physical.Keys,Assert.Single(snapshots[1]).Keys);
        var firstCount=snapshots[0].Count; var secondCount=snapshots[1].Count; controller.SetPowered(false); controller.Publish(); Assert.Equal(secondCount,snapshots[1].Count); Assert.Equal(firstCount,snapshots[0].Count);
        controller.Dispose(); var count=snapshots[1].Count; controller.Publish(); Assert.Equal(count,snapshots[1].Count);
    }
    public static void Accumulate()
    {
        var accumulator=new EmulationInputAccumulator();
        var first=EmulationInputSnapshot.Empty with { Pointer=new(10,int.MaxValue,2,true,false,false) };
        var second=EmulationInputSnapshot.Empty with { Pointer=new(-3,100,1,false,true,false) };
        accumulator.Update(first);accumulator.Update(second);
        var result=accumulator.Consume();
        Assert.Equal(7,result.Pointer.DeltaX);Assert.Equal(int.MaxValue,result.Pointer.DeltaY);
        Assert.Equal(3,result.Pointer.Wheel);Assert.True(result.Pointer.Right);Assert.False(result.Pointer.Left);
        var next=accumulator.Consume();
        Assert.Equal(0,next.Pointer.DeltaX);Assert.Equal(0,next.Pointer.Wheel);Assert.True(next.Pointer.Right);
    }
    public static void Chords()
    {
        Assert.True(KeyboardChordFunctions.TryParse("Control+Shift+A+A",out var chord));
        Assert.True(KeyboardChordFunctions.Matches(chord,ModifierKeys.Control|ModifierKeys.Shift,new HashSet<Key>{Key.A}));
        Assert.False(KeyboardChordFunctions.Matches(chord,ModifierKeys.Control,new HashSet<Key>{Key.A}));
        Assert.False(KeyboardChordFunctions.Matches(chord,chord.Modifiers,new HashSet<Key>{Key.A,Key.B}));
        Assert.True(KeyboardChordFunctions.TryParse("Alt+F4",out var reserved));
        Assert.True(KeyboardChordFunctions.IsWindowsReserved(reserved));
        Assert.False(KeyboardChordFunctions.TryParse("Ctrl",out _));
        Assert.False(KeyboardChordFunctions.TryParse("not-a-key",out _));
    }
}
