using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using Xunit.Abstractions;

namespace GWGUI.Tests.Emulation;

public sealed class HardwareSettingsSnapshotTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("Bandai", "AD5872E59466B051E32965E301E1B187CFB8289AAEDA4B450FE29B61677B45DC")]
    [InlineData("Coleco", "0260B88CF4A277186CB770D418D86216ADC8BBA7CA1D15633DC921277094EAED")]
    [InlineData("Sharp", "0AA2331CD87EE35BE98C069854518F7C9446E6FC7705EAB8CAE25F93495D03B1")]
    [InlineData("Sinclair", "ACCC2A9E86BA80C1812B96CC13FD9D057100BDBA8B9C433508E39B7DC3AEE4CA")]
    [InlineData("Snk", "E454262203C0DD57C8B0641671EFDC46703B539ED41209ACD5C03C2831BCE1C2")]
    [InlineData("Mattel", "66098D986C1031461DAC25D2F83565C1C3D1CA47C2C9768058C234A4CFE8144F")]
    [InlineData("Philips", "384EDE9BF1CF061E49E44541A0DC504D8A123F7D15E88C0497E56293DD88F4DD")]
    [InlineData("Fairchild", "73EFFC9CAC5BB7825B6D89DF933CA6B2708A08B56884817EBADEE40AE814FD4E")]
    [InlineData("Epoch", "F51E93946F74C74BD4EDFFE50BB6CADE3265F62E79917DC2B8C66A13FDC051AD")]
    public void HardwareFieldsKeepTheirPreRefactorGraphForEveryModel(string module, string expected)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
            var assembly = Assembly.Load("gwgui.emulation." + module.ToLowerInvariant());
            var root = "GWGUI.Emulation." + module + ".Common.Machines.Common.";
            var configurationType = assembly.GetType(root + "Contracts.MachineConfiguration", throwOnError: true)!;
            var modelType = configurationType.GetProperty("Model")!.PropertyType;
            var constructor = Assert.Single(configurationType.GetConstructors(), candidate =>
                candidate.GetParameters() is { Length: > 0 } parameters && parameters[0].ParameterType == modelType
                && parameters.Skip(1).All(parameter => parameter.IsOptional));
            var addHardware = assembly.GetType(root + "Functions.SettingsDescriptionFunctions", throwOnError: true)!
                .GetMethod("AddHardware", BindingFlags.NonPublic | BindingFlags.Static)!;
            var graphs = new List<object>();
            foreach (var model in Enum.GetValues(modelType))
            {
                var arguments = Enumerable.Repeat<object?>(Type.Missing, constructor.GetParameters().Length).ToArray();
                arguments[0] = model;
                var configuration = constructor.Invoke(arguments);
                var blocks = new List<EmulationSettingsBlock>();
                addHardware.Invoke(null, [blocks, configuration]);
                graphs.Add(new { Model = model.ToString(), Blocks = blocks });
            }
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(graphs))));
            output.WriteLine($"{module}: {hash} ({graphs.Count} modèles)");
            Assert.Equal(expected, hash);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }
}
