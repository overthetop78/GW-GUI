using GWGUI.Infrastructure.Commands;
using EnabledOption = global::GWGUI.MediaEngine.Commands.Options.EnabledOption;
using ConversionCommandBuilder = global::GWGUI.Infrastructure.Conversion.ConversionCommandBuilder;
using ConversionOutput = global::GWGUI.MediaEngine.Conversion.ConversionOutput;
using GWGUI.Infrastructure.Maintenance;
using GWGUI.Infrastructure.Read;
using GWGUI.Infrastructure.Write;
namespace GWGUI.Infrastructure.Commands.Building;

public sealed class GwCommandBuilder : IGwCommandBuilder
{
    public GwCommand BuildInfo(GwInfoRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Executable);
        var arguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Device)) arguments.AddRange(["--device", request.Device]);
        if (request.Bootloader) arguments.Add("--bootloader");
        return new GwCommand(request.Executable, "info", arguments);
    }

    public GwCommand BuildRead(ReadRequest request) => ReadCommandBuilder.Build(request);
    public GwCommand BuildWrite(WriteRequest request) => WriteCommandBuilder.Build(request);
    public GwCommand BuildConversion(string executable, string source, ConversionOutput output,
        IReadOnlyList<EnabledOption>? options = null, string? expertArguments = null) =>
        ConversionCommandBuilder.Build(executable, source, output, options, expertArguments);
    public GwCommand BuildErase(EraseRequest request) => MaintenanceCommandBuilder.Erase(request);
    public GwCommand BuildClean(CleanRequest request) => MaintenanceCommandBuilder.Clean(request);
    public GwCommand BuildTool(ToolCommandRequest request) => ToolCommandBuilder.Build(request);
}
