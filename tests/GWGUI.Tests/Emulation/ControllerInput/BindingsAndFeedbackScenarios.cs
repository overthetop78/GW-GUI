using GWGUI.Emulation.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.App.Functions.Input.Bindings;
using System.Windows.Input;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
namespace GWGUI.Tests.Emulation.ControllerInput;
internal static class BindingsAndFeedbackScenarios
{
    public static void Mapping()
    {
        var detailed = GWGUI.App.Services.Input.GameInput.GameInputLiveState.Empty("rawgamecontroller:test") with
        {
            Controls =
            [
                new(GWGUI.App.Services.Input.GameInput.GameInputControlType.Button, 0, default, 1f),
                new(GWGUI.App.Services.Input.GameInput.GameInputControlType.Button, 1, default, 1f)
            ]
        };
        Assert.Equal(new[] { "Button1", "Button2" }, GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor.NewlyActivatedDetailedControls(detailed, GWGUI.App.Services.Input.GameInput.GameInputLiveState.Empty(detailed.DeviceId)));
        var dualView = EmulationControllerState.Empty with { Controls = new EmulationControllerControls(new Dictionary<string, float> { ["Button0"] = 1f, ["Axis0"] = .75f, ["Switch0"] = 1f }) };
        Assert.False(GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor.ShouldCaptureStandardControl(dualView, "ButtonA"));
        Assert.False(GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor.ShouldCaptureStandardControl(dualView, "LeftStickRight"));
        Assert.False(GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor.ShouldCaptureStandardControl(dualView, "DPadUp"));
        Assert.True(GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor.ShouldCaptureStandardControl(dualView, "XboxButton"));
        Assert.True(GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor.ShouldCaptureStandardControl(EmulationControllerState.Empty, "ButtonA"));
        var analogState = EmulationControllerState.Empty with { DeviceId = "axis", Controls = new EmulationControllerControls(new Dictionary<string, float> { ["Axis0"] = .55f }) };
        var analogSnapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl }, Controllers = [analogState] };
        const string analogBinding = "Keyboard:LeftControl+Controller:axis:Axis0Positive";
        foreach (var family in new[] { "Mattel", "Philips", "Sharp", "Sinclair", "Snk", "Thomson", "Watara", "Panasonic", "Epoch" })
        {
            var assembly = System.Reflection.Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
            var method = assembly.GetTypes().SelectMany(type => type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
                .Single(candidate => candidate.Name is "Magnitude" or "Value" or "AxisValue" && candidate.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(string), typeof(EmulationControllerState), typeof(EmulationInputSnapshot), typeof(int) }));
            Assert.Equal(0f, (float)method.Invoke(null, [analogBinding, analogState, analogSnapshot, 20])!);
            var halfway = analogState with { Controls = new EmulationControllerControls(new Dictionary<string, float> { ["Axis0"] = .75f }) };
            var output = (float)method.Invoke(null, [analogBinding, halfway, analogSnapshot with { Controllers = new[] { halfway } }, 20])!;
            Assert.Equal(family == "Epoch" ? .375f : .5f, output, 4);
            Assert.Equal(0f, (float)method.Invoke(null, [analogBinding, halfway, analogSnapshot with { Keys = new HashSet<EmulationKey>(), Controllers = new[] { halfway } }, 20])!);
        }
        var keys=new HashSet<GWGUI.Emulation.Enums.EmulationKey>{GWGUI.Emulation.Enums.EmulationKey.A,GWGUI.Emulation.Enums.EmulationKey.B};
        var mapped=GWGUI.Emulation.Functions.EmulationInputMappingFunctions.MapKeyboard(keys,new Dictionary<string,GWGUI.Emulation.Enums.EmulationKey>{{"F1",GWGUI.Emulation.Enums.EmulationKey.A}});
        Assert.Equal(new[]{GWGUI.Emulation.Enums.EmulationKey.B,GWGUI.Emulation.Enums.EmulationKey.F1}.Order(),mapped.Order()); Assert.Contains(GWGUI.Emulation.Enums.EmulationKey.A,keys);
        var first=EmulationControllerState.Empty with{DeviceId="one"}; var second=new EmulationControllerState(1u<<8,short.MinValue,short.MaxValue,0,0,0,0){DeviceId="two"};
        Assert.Same(second,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ResolveController("TWO",[first,second],0));
        Assert.Same(EmulationControllerState.Empty,GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ResolveController("missing",[first,second],0));
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
        var mixed = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.A },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true, Middle = true },
            Controllers = [second with { Buttons = (1u << 8) | (1u << 9) }]
        };
        const string combined = "Keyboard:LeftControl+Keyboard:A+Mouse:Left+Mouse:Middle+Controller:two:ButtonA+Controller:two:ButtonX";
        Assert.True(EmulationKeyboardBindingFunctions.IsValid(combined));
        Assert.Equal(1f, EmulationInputMappingFunctions.InputSourcePressure(combined, EmulationControllerState.Empty, mixed));
        Assert.Equal(0f, EmulationInputMappingFunctions.InputSourcePressure(combined, EmulationControllerState.Empty, mixed with { Pointer = mixed.Pointer with { Middle = false } }));
        Assert.Equal(0f, EmulationInputMappingFunctions.InputSourcePressure(combined, EmulationControllerState.Empty, mixed with { Keys = new HashSet<EmulationKey> { EmulationKey.A } }));
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
        var snapshots=new[] {new List<EmulationInputSnapshot>(),new List<EmulationInputSnapshot>()};
        var states=new[] {GWGUI.Emulation.Enums.EmulationMachineState.Running,GWGUI.Emulation.Enums.EmulationMachineState.Running};
        var active=true; var selected=0; var reads=0; var focus=0;
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
            return GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.Emulation.Interfaces.IEmulatedMachine>((method,_)=>method.Name switch
            {
                "get_Input"=>input,
                "get_State"=>states[index],
                _=>throw new InvalidOperationException(method.Name)
            });
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
        states[1]=GWGUI.Emulation.Enums.EmulationMachineState.Stopping;
        var stoppingCount=snapshots[1].Count; controller.Publish(); Assert.Equal(stoppingCount,snapshots[1].Count);
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
        var modifierState = new GWGUI.App.Functions.Input.Keyboard.KeyboardModifierState();
        var held = new HashSet<Key>();
        modifierState.Press(Key.LeftCtrl, 1000, held);
        held.Add(Key.LeftCtrl);
        modifierState.Press(Key.RightAlt, 1000, held);
        held.Add(Key.RightAlt);
        Assert.DoesNotContain(Key.LeftCtrl, held);
        Assert.Equal(ModifierKeys.Alt, modifierState.Normalize(ModifierKeys.Control | ModifierKeys.Alt, held));
        held.Add(Key.RightCtrl);
        Assert.Equal(ModifierKeys.Control | ModifierKeys.Alt, modifierState.Normalize(ModifierKeys.Control | ModifierKeys.Alt, held));
        modifierState.Reset();
        held.Clear();
        modifierState.Press(Key.LeftCtrl, 1000, held);
        held.Add(Key.LeftCtrl);
        modifierState.Press(Key.RightAlt, 1100, held);
        held.Add(Key.RightAlt);
        Assert.Contains(Key.LeftCtrl, held);
        Assert.False(modifierState.HasSyntheticLeftControl);
        Assert.True(GWGUI.App.Functions.Input.Keyboard.EmulationKeyMapper.TryMap(Key.D1, out var topRow));
        Assert.True(GWGUI.App.Functions.Input.Keyboard.EmulationKeyMapper.TryMap(Key.NumPad1, out var keypad));
        Assert.NotEqual(topRow, keypad);
        Assert.True(KeyboardChordFunctions.TryParse("Control+Shift+A+A",out var chord));
        Assert.True(KeyboardChordFunctions.Matches(chord,ModifierKeys.Control|ModifierKeys.Shift,new HashSet<Key>{Key.A}));
        Assert.False(KeyboardChordFunctions.Matches(chord,ModifierKeys.Control,new HashSet<Key>{Key.A}));
        Assert.False(KeyboardChordFunctions.Matches(chord,chord.Modifiers,new HashSet<Key>{Key.A,Key.B}));
        Assert.True(KeyboardChordFunctions.TryParse("Alt+Tab",out var reserved));
        Assert.True(KeyboardChordFunctions.IsWindowsReserved(reserved));
        Assert.True(KeyboardChordFunctions.TryParse("Ctrl",out var control));
        Assert.True(KeyboardChordFunctions.Matches(control, ModifierKeys.Control, new HashSet<Key> { Key.LeftCtrl }));
        Assert.True(KeyboardChordFunctions.TryParse("RightAlt", out var alt));
        Assert.True(KeyboardChordFunctions.Matches(alt, ModifierKeys.Alt, new HashSet<Key> { Key.RightAlt }));
        Assert.False(KeyboardChordFunctions.Matches(alt, ModifierKeys.Alt, new HashSet<Key> { Key.LeftAlt }));
        Assert.True(KeyboardChordFunctions.TryParse("Ctrl+Shift+NumpadPeriod", out var persisted));
        Assert.True(KeyboardChordFunctions.Matches(persisted, ModifierKeys.Control | ModifierKeys.Shift, new HashSet<Key> { Key.LeftCtrl, Key.LeftShift, Key.Decimal }));
        Assert.False(GWGUI.App.Services.Input.Keyboard.ForegroundKeyboardShortcutGuard.ShouldIntercept(Key.Tab, ModifierKeys.Alt));
        Assert.False(GWGUI.App.Services.Input.Keyboard.ForegroundKeyboardShortcutGuard.ShouldIntercept(Key.Delete, ModifierKeys.Control | ModifierKeys.Alt));
        Assert.False(GWGUI.App.Services.Input.Keyboard.ForegroundKeyboardShortcutGuard.ShouldIntercept(Key.Escape, ModifierKeys.Control | ModifierKeys.Shift));
        Assert.False(GWGUI.App.Services.Input.Keyboard.ForegroundKeyboardShortcutGuard.ShouldIntercept(Key.A, ModifierKeys.Windows));
        Assert.False(GWGUI.App.Services.Input.Keyboard.ForegroundKeyboardShortcutGuard.ShouldIntercept(Key.F4, ModifierKeys.Alt));
        Assert.True(GWGUI.App.Services.Input.Keyboard.ForegroundKeyboardShortcutGuard.ShouldIntercept(Key.A, ModifierKeys.Control));
        Assert.False(KeyboardChordFunctions.TryParse("not-a-key",out _));
        Assert.Equal("Ctrl+Alt+Shift+F6+F8", KeyboardChordFunctions.Format(ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt, [Key.F8, Key.F6]));
        Assert.Equal("RightCtrl+RightAlt+LeftShift+F6+F8", KeyboardChordFunctions.Format(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, [Key.LeftShift, Key.F8, Key.RightAlt, Key.RightCtrl, Key.F6]));
        for (var number = 1; number <= 24; number++)
        {
            Assert.True(GWGUI.App.Functions.Input.Keyboard.EmulationKeyMapper.TryMap(Enum.Parse<Key>("F" + number), out var function));
            Assert.Equal("F" + number, function.ToString());
            Assert.True(GWGUI.App.Functions.Input.Keyboard.EmulationKeyMapper.TryMap(function, out var physical));
            Assert.Equal(Enum.Parse<Key>("F" + number), physical);
        }
        Assert.True(GWGUI.App.Functions.Input.Keyboard.EmulationKeyMapper.TryMap(Key.LWin, out var leftWindows));
        Assert.Equal(EmulationKey.LeftWindows, leftWindows);
        Assert.True(GWGUI.App.Functions.Input.Keyboard.EmulationKeyMapper.TryMap(Key.RWin, out var rightWindows));
        Assert.Equal(EmulationKey.RightWindows, rightWindows);
    }
}
