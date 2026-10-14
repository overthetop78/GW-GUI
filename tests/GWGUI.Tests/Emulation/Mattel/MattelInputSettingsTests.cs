using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Mattel.Common.Machines.Common.Enums;
using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Mattel.Emulators.FreeIntv.Factories;
using GWGUI.Emulation.Mattel.Modules;
using NativeInput = GWGUI.Emulation.Mattel.Emulators.Common.Interop.Constants.InputConstants;

namespace GWGUI.Tests.Emulation.Mattel;

public sealed class MattelInputSettingsTests
{
    [Fact]
    public void BothPortsExposeOppositeDirectionsWithoutPhysicalDefaults()
    {
        using var http = new HttpClient();
        var module = new MattelEmulationModule("unused", "unused", http, "unused");
        var settings = module.DescribeInputSettings(new MachineConfiguration(MachineModel.Intellivision));
        Assert.Equal(2, settings.ControllerPorts.Count);
        var opposites = new Dictionary<string, string>
        {
            ["Up"] = "Down", ["Down"] = "Up", ["Left"] = "Right", ["Right"] = "Left"
        };
        foreach (var port in settings.ControllerPorts)
        foreach (var choice in port.ControllerChoices)
        foreach (var action in choice.BindingDefinitions!)
        {
            Assert.Empty(action.DefaultBinding);
            Assert.Equal(opposites.GetValueOrDefault(action.Id), action.OppositeDirectionId);
        }
    }

    [Fact]
    public void MixedAxesPersistAndRetainPressureUntilAnySourceIsReleased()
    {
        using var http = new HttpClient();
        var module = new MattelEmulationModule("unused", "unused", http, "unused");
        var original = new MachineConfiguration(MachineModel.Intellivision);
        var settings = module.DescribeInputSettings(original);
        var ports = settings.ControllerPorts.Select(port => port with
        {
            Bindings = port.Bindings with
            {
                Values = new Dictionary<string, string>
                {
                    ["Right"] = "Keyboard:LeftControl+Mouse:Left+Controller:LeftStickRight"
                }
            }
        }).ToArray();
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(original, settings with { ControllerPorts = ports }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        var physical = EmulationControllerState.Empty with { LeftX = 8192 };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true },
            Controllers = [physical, physical]
        };
        var mapper = new FreeIntvMachineFactory();
        var mapped = mapper.MapInput(snapshot, restored.Input, restored.Model);
        for (uint port = 0; port < 2; port++)
            Assert.InRange(NativeInputFunctions.State(mapped, port, NativeInput.AnalogDevice, NativeInput.LeftAnalogIndex, NativeInput.AnalogXId, Emulator.FreeIntv), (short)8100, (short)8200);
        foreach (var released in new[]
        {
            snapshot with { Keys = new HashSet<EmulationKey>() },
            snapshot with { Pointer = snapshot.Pointer with { Left = false } },
            snapshot with { Controllers = [EmulationControllerState.Empty, EmulationControllerState.Empty] }
        })
        {
            mapped = mapper.MapInput(released, restored.Input, restored.Model);
            Assert.All(mapped.Controllers, controller => Assert.Equal((short)0, controller.LeftX));
        }
        mapped = mapper.MapInput(snapshot, original.Input, original.Model);
        Assert.All(mapped.Controllers, controller =>
        {
            Assert.Equal(0u, controller.Buttons);
            Assert.Equal((short)0, controller.LeftX);
        });
    }

    [Fact]
    public void PlayerOneControllerChoiceRequiresRestart()
    {
        using var http = new HttpClient();
        var module = new MattelEmulationModule("unused", "unused", http, "unused");
        var field = Assert.Single(module.Describe("Intellivision").Blocks.SelectMany(block => block.Fields), field => field.Id == "default_p1_controller");
        Assert.Equal(EmulationMachineTab.Controllers, field.Tab);
        Assert.True(field.RequiresRestart);
    }
}
