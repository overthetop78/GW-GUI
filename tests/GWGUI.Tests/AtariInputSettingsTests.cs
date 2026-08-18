using GWGUI.App.Controls;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariInputSettingsTests
{
    [Fact]
    public void ControllerSectionCanBeRebuiltForAnotherConfiguration()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var section = new AtariInputSettingsSection();

            section.Load(new AtariMachineConfiguration(AtariMachineModel.Atari2600));
            section.Load(new AtariMachineConfiguration(AtariMachineModel.Jaguar));
        });
    }

    [Fact]
    public void ComputerKeyboardEditorContainsOnlyMachineSpecificAssignments()
    {
        var st = AtariInputSettingsFunctions.Create(new AtariMachineConfiguration(AtariMachineModel.St));
        var eightBit = AtariInputSettingsFunctions.Create(
            new AtariMachineConfiguration(AtariMachineModel.Atari800Xl));

        Assert.Equal(AtariKeyboardSettingsConstants.FunctionKeys.Concat(AtariKeyboardSettingsConstants.ComputerSpecialKeys)
                .Select(value => value.ToString()),
            st.KeyboardDefinitions.Select(value => value.Id));
        Assert.Equal(AtariKeyboardSettingsConstants.Atari800SpecialKeys.Select(value => value.ToString()),
            eightBit.KeyboardDefinitions.Select(value => value.Id));
        Assert.DoesNotContain(st.KeyboardDefinitions, value => value.Id == EmulationKey.A.ToString());
        var undo = st.KeyboardDefinitions.Single(value => value.Id == EmulationKey.AtariUndo.ToString());
        var interrupt = st.KeyboardDefinitions.Single(value => value.Id == EmulationKey.AtariBreak.ToString());
        var help = st.KeyboardDefinitions.Single(value => value.Id == EmulationKey.Help.ToString());
        Assert.Equal(EmulationKey.Home.ToString(), undo.DefaultBinding);
        Assert.Equal("Help", help.Label);
        Assert.NotEqual(undo.Id, undo.Label);
        Assert.NotEqual(interrupt.Id, interrupt.Label);
    }

    [Fact]
    public void ConsoleInputPagesDoNotExposeControllerButtonsAsKeyboardOrMouseActions()
    {
        foreach (var model in new[]
                 {
                     AtariMachineModel.Atari2600,
                     AtariMachineModel.Atari5200,
                     AtariMachineModel.Atari7800,
                     AtariMachineModel.Lynx,
                     AtariMachineModel.Jaguar,
                     AtariMachineModel.JaguarCd
                 })
        {
            var view = AtariInputSettingsFunctions.Create(new AtariMachineConfiguration(model));

            Assert.False(view.HasKeyboard);
            Assert.False(view.HasMouse);
            Assert.Empty(view.KeyboardDefinitions);
            Assert.Empty(view.MouseDefinitions);
        }
    }

    [Fact]
    public void AtariComputerMouseExposesOnlyItsTwoPhysicalButtons()
    {
        var view = AtariInputSettingsFunctions.Create(new AtariMachineConfiguration(AtariMachineModel.St));

        Assert.True(view.HasMouse);
        Assert.Equal(new[] { "Left", "Right" }, view.MouseDefinitions.Select(value => value.Id));
    }

    public static TheoryData<AtariMachineModel> EveryModel => new(Enum.GetValues<AtariMachineModel>());

    [Theory]
    [InlineData(AtariMachineModel.Atari400, 4)]
    [InlineData(AtariMachineModel.Atari800, 4)]
    [InlineData(AtariMachineModel.Atari800Xl, 2)]
    [InlineData(AtariMachineModel.Atari130Xe, 2)]
    [InlineData(AtariMachineModel.Xegs, 2)]
    [InlineData(AtariMachineModel.Atari5200, 4)]
    [InlineData(AtariMachineModel.Atari2600, 2)]
    [InlineData(AtariMachineModel.Atari7800, 2)]
    [InlineData(AtariMachineModel.Lynx, 1)]
    [InlineData(AtariMachineModel.Jaguar, 2)]
    [InlineData(AtariMachineModel.JaguarCd, 2)]
    public void ModelExposesItsPhysicalControllerPortCount(AtariMachineModel model, int expected)
    {
        Assert.Equal(expected, AtariInputSettingsFunctions.Create(
            new AtariMachineConfiguration(model)).Ports.Count);
    }

    [Theory]
    [MemberData(nameof(EveryModel))]
    public void EveryModelUsesItsInputCompatibilityAndPortCount(AtariMachineModel model)
    {
        var view = AtariInputSettingsFunctions.Create(new AtariMachineConfiguration(model));
        var compatibility = AtariCompatibilityCatalog.Get(model);

        Assert.Equal(compatibility.ControllerPortCount, view.Ports.Count);
        Assert.Equal(IsEditable(compatibility, AtariSettingOption.KeyboardMappings), view.HasKeyboard);
        Assert.Equal(IsEditable(compatibility, AtariSettingOption.MouseMappings), view.HasMouse);
        Assert.All(view.Ports, port =>
        {
            Assert.DoesNotContain(AtariPeripheralKind.Automatic, port.Peripherals);
            Assert.Contains(AtariPeripheralKind.None, port.Peripherals);
            Assert.NotEmpty(port.Definitions);
        });
    }

    [Fact]
    public void SelectedAtariPeripheralsExposeOnlyTheirSpecificMappings()
    {
        var atari5200 = AtariControllerSettingsFunctions.Definitions(
            AtariMachineModel.Atari5200, AtariPeripheralKind.AnalogJoystick,
            AtariInputSettingsTestConstants.FirstPort);
        var jaguar = AtariControllerSettingsFunctions.Definitions(
            AtariMachineModel.Jaguar, AtariPeripheralKind.EnhancedController,
            AtariInputSettingsTestConstants.FirstPort);

        Assert.Contains(atari5200,
            value => value.Id == AtariInputSettingsTestConstants.NumericKey);
        Assert.Contains(jaguar,
            value => value.Id == AtariInputSettingsTestConstants.JaguarButton);
        Assert.DoesNotContain(jaguar,
            value => value.Id == AtariInputSettingsTestConstants.TurboAction);
    }

    [Fact]
    public void LegacyAtari5200KeypadSelectionLoadsAsTheCompleteController()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari5200,
            input: new AtariInputConfiguration(Controllers:
            [new AtariControllerBinding(AtariInputSettingsTestConstants.FirstPort,
                AtariPeripheralKind.NumericKeypad)]));

        var port = AtariInputSettingsFunctions.Create(configuration).Ports[0];

        Assert.Equal(AtariPeripheralKind.AnalogJoystick, port.Selected);
        Assert.DoesNotContain(AtariPeripheralKind.NumericKeypad, port.Peripherals);
        Assert.Contains(port.Definitions, definition => definition.Id == AtariInputSettingsTestConstants.NumericKey);
    }

    [Fact]
    public void ApplyPersistsMappingsControllersMouseAndUnknownOptions()
    {
        var source = new AtariMachineConfiguration(AtariMachineModel.St,
            options: new Dictionary<string, string>
            {
                [AtariInputSettingsTestConstants.UnknownOption] = AtariInputSettingsTestConstants.UnknownValue
            });
        var row = new InputBindingRow(AtariInputSettingsTestConstants.Action,
            AtariInputSettingsTestConstants.Action, EmulationKey.A.ToString(), EmulationKey.A.ToString());
        var controller = new AtariControllerBinding(AtariInputSettingsTestConstants.FirstPort,
            AtariPeripheralKind.Joystick);

        var result = AtariInputSettingsFunctions.Apply(source, [row], [], [controller], false,
            EmulationKey.F10, AtariInputSettingsTestConstants.MouseSpeed);

        Assert.False(result.Input.CaptureMouse);
        Assert.Equal(EmulationKey.F10, result.Input.ReleaseMouseKey);
        Assert.Equal(EmulationKey.A, result.Input.KeyboardMappings![AtariInputSettingsTestConstants.Action]);
        Assert.Single(result.Input.Controllers!);
        Assert.Equal(AtariInputSettingsTestConstants.MouseSpeed.ToString(),
            result.Options[AtariMouseSettingsConstants.SpeedOptionKey]);
        Assert.Equal(AtariInputSettingsTestConstants.UnknownValue,
            result.Options[AtariInputSettingsTestConstants.UnknownOption]);
    }

    [Fact]
    public void SharedBindingTableDetectsClearsRestoresAndDeletesBindings()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var editor = new InputBindingEditor();
            var definitions = new[]
            {
                new InputBindingDefinition(AtariInputSettingsTestConstants.FirstAction,
                    AtariInputSettingsTestConstants.FirstAction, AtariInputSettingsTestConstants.FirstDefault),
                new InputBindingDefinition(AtariInputSettingsTestConstants.SecondAction,
                    AtariInputSettingsTestConstants.SecondAction, AtariInputSettingsTestConstants.SecondDefault)
            };
            editor.SetRows(definitions, new Dictionary<string, string>
            {
                [AtariInputSettingsTestConstants.FirstAction] = AtariInputSettingsTestConstants.ConflictBinding,
                [AtariInputSettingsTestConstants.SecondAction] = AtariInputSettingsTestConstants.ConflictBinding
            });
            Assert.True(editor.HasErrors);

            Invoke(editor, AtariInputSettingsTestConstants.ClearConflictsMethod, editor, new RoutedEventArgs());
            Assert.All(editor.Rows, row => Assert.Equal(string.Empty, row.Binding));
            Invoke(editor, AtariInputSettingsTestConstants.RestoreDefaultsMethod, editor, new RoutedEventArgs());
            Assert.Equal(AtariInputSettingsTestConstants.FirstDefault, editor.Rows[0].Binding);
            Assert.Equal(AtariInputSettingsTestConstants.SecondDefault, editor.Rows[1].Binding);
            Invoke(editor, AtariInputSettingsTestConstants.ClearMethod,
                new Button { Tag = editor.Rows[0] }, new RoutedEventArgs());
            Assert.Equal(string.Empty, editor.Rows[0].Binding);
        });
    }

    private static void Invoke(InputBindingEditor editor, string method, object sender, RoutedEventArgs args) =>
        typeof(InputBindingEditor).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(editor, [sender, args]);

    private static void RunOnSta(Action action)
        => WpfTestHost.Run(action);

    private static bool IsEditable(AtariCompatibilityDefinition definition, AtariSettingOption option) =>
        definition.Options.Single(value => value.Option == option).Availability == AtariOptionAvailability.Editable;
}

internal static class AtariInputSettingsTestConstants
{
    internal const int FirstPort = 0;
    internal const int MouseSpeed = 125;
    internal const string NumericKey = "Key0";
    internal const string JaguarButton = "A";
    internal const string TurboAction = "Turbo";
    internal const string Action = "Action";
    internal const string UnknownOption = "future_input_option";
    internal const string UnknownValue = "preserved";
    internal const string FirstAction = "First";
    internal const string SecondAction = "Second";
    internal const string FirstDefault = "A";
    internal const string SecondDefault = "B";
    internal const string ConflictBinding = "C";
    internal const string ClearConflictsMethod = "ClearConflictsClicked";
    internal const string RestoreDefaultsMethod = "RestoreDefaultsClicked";
    internal const string ClearMethod = "ClearClicked";
    internal const int StaTimeoutMilliseconds = 10000;
}
