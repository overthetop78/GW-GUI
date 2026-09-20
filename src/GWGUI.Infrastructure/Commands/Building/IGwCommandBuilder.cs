using GWGUI.Infrastructure.Commands;
using EnabledOption = global::GWGUI.MediaEngine.Commands.Options.EnabledOption;
using ConversionOutput = global::GWGUI.MediaEngine.Conversion.ConversionOutput;
using GWGUI.Infrastructure.Maintenance;
using GWGUI.Infrastructure.Read;
using GWGUI.Infrastructure.Write;
namespace GWGUI.Infrastructure.Commands.Building;

/// <summary>
/// Central contract for every command assembled by the application.
/// Specialized builders retain the validation rules for their operation;
/// this service gives callers one substitutable dependency.
/// </summary>
public interface IGwCommandBuilder
{
    GwCommand BuildInfo(GwInfoRequest request);
    GwCommand BuildRead(ReadRequest request);
    GwCommand BuildWrite(WriteRequest request);
    GwCommand BuildConversion(string executable, string source, ConversionOutput output, IReadOnlyList<EnabledOption>? options = null, string? expertArguments = null);
    GwCommand BuildErase(EraseRequest request);
    GwCommand BuildClean(CleanRequest request);
    GwCommand BuildTool(ToolCommandRequest request);
}
