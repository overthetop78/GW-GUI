using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using Xunit.Abstractions;

namespace GWGUI.Tests.Emulation;

public sealed class AdditionalNativeFieldSnapshotTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("Bandai", "61E0AB3FE57B20A536C821BF6EF438ED25BCEEBB635ABC1A33AA828FA164E8AE")]
    [InlineData("Coleco", "09B9446D269E293126182897C0A9190638C677AA682E86F0C857BA256BB567C8")]
    [InlineData("Mattel", "A59F5563A245EDA148412EA137298E9F7783253C5B947D6015CA1691A0E63E29")]
    [InlineData("Philips", "A59F5563A245EDA148412EA137298E9F7783253C5B947D6015CA1691A0E63E29")]
    [InlineData("Epoch", "A59F5563A245EDA148412EA137298E9F7783253C5B947D6015CA1691A0E63E29")]
    [InlineData("Fairchild", "5BD2568589E8A75D0F88231F03ACB0351F940DE45734F252C2A7D15D703C6E7B")]
    [InlineData("Apple", "4F981FD0EDF59980DEB0DD4AECC7CF19926686D14608407AE3A51FBF23F56DDF")]
    public void NativeFieldGraphsAndErrorTypesKeepTheirPreRefactorProfiles(string module, string expected)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
            var assembly = Assembly.Load("gwgui.emulation." + module.ToLowerInvariant());
            var root = "GWGUI.Emulation." + module + ".Common.";
            var configurationType = assembly.GetType(root + "Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            var modelType = configurationType.GetProperty("Model")!.PropertyType;
            var constructor = Assert.Single(configurationType.GetConstructors(), candidate =>
                candidate.GetParameters() is { Length: > 0 } parameters && parameters[0].ParameterType == modelType
                && parameters.Skip(1).All(parameter => parameter.IsOptional));
            var arguments = Enumerable.Repeat<object?>(Type.Missing, constructor.GetParameters().Length).ToArray();
            arguments[0] = module == "Apple" ? Enum.Parse(modelType, "AppleII") : Enum.GetValues(modelType).GetValue(0);
            var configuration = constructor.Invoke(arguments);
            var optionType = assembly.GetType(root + "Contracts.CoreOption", throwOnError: true)!;
            var native = assembly.GetType(root + "Machines.Common.Functions.SettingsDescriptionFunctions", throwOnError: true)!
                .GetMethod("Native", BindingFlags.Static | BindingFlags.NonPublic)!;
            Scenario[] scenarios =
            [
                new("normal", ["enabled", "disabled"]),
                new("duplicate", ["enabled", "enabled"]),
                new("upper", ["ON", "OFF"]),
                new("capitalized", ["Enabled", "Disabled"]),
                new("selection", ["automatic", "enabled", "disabled"]),
                new("invariant", ["enabled", "disabled"], MissingChoiceResource: true, Invariant: "KEEP"),
                new("missing-choice", ["enabled", "disabled"], MissingChoiceResource: true),
                new("missing-resource", ["enabled", "disabled"], MissingResource: true),
                new("empty", [])
            ];
            var results = new List<object>();
            foreach (var scenario in scenarios)
            {
                var json = JsonSerializer.Serialize(new
                {
                    Key = "audit_mode", Name = "Mode", Category = "Video",
                    DefaultValue = scenario.Values.LastOrDefault() ?? string.Empty, CurrentValue = "IGNORED",
                    ResourceKey = scenario.MissingResource ? null : "Native.Audit.Mode",
                    ExplanationResourceKey = "Native.Audit.Help",
                    Values = scenario.Values.Select(value => new
                    {
                        Value = value, Label = value,
                        ResourceKey = scenario.MissingChoiceResource ? null : "Native.Choice." + value,
                        InvariantDisplayValue = scenario.Invariant
                    }).ToArray()
                });
                var option = JsonSerializer.Deserialize(json, optionType)!;
                EmulationSettingsField? field = null;
                string? error = null;
                try
                {
                    object?[] call = native.GetParameters().Length == 3
                        ? [configuration, option, EmulationMachineTab.Video]
                        : [configuration, option, EmulationMachineTab.Video, "video"];
                    field = Assert.IsType<EmulationSettingsField>(native.Invoke(null, call));
                }
                catch (TargetInvocationException caught) when (caught.InnerException is not null)
                {
                    error = caught.InnerException.GetType().FullName;
                }
                if (scenario.Name == "normal") Assert.NotNull(field);
                results.Add(new { scenario.Name, Field = field, Error = error });
            }
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(results))));
            output.WriteLine($"{module}: {hash} ({scenarios.Length} profils)");
            Assert.Equal(expected, hash);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    private sealed record Scenario(string Name, string[] Values, bool MissingChoiceResource = false,
        bool MissingResource = false, string? Invariant = null);
}
