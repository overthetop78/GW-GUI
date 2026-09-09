using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updater;

internal static class UpdatePlanReader
{
    internal static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    internal static UpdateExecutionPlan Read(string planPath)
    {
        var path = Path.GetFullPath(planPath);
        var work = Path.GetDirectoryName(path) ?? throw new InvalidDataException("The update plan has no directory.");
        var plan = JsonSerializer.Deserialize<UpdateExecutionPlan>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("The update plan is empty.");
        if (plan.SchemaVersion != 2 || !Guid.TryParseExact(plan.TransactionId, "N", out _))
            throw new InvalidDataException("The update plan identity is invalid.");
        if (plan.ProcessIds.Count == 0 || plan.ProcessIds.Any(id => id <= 0)
            || plan.ShutdownTimeoutSeconds is < 1 or > 300 || plan.StartupTimeoutSeconds is < 1 or > 300)
            throw new InvalidDataException("The update plan process or timeout values are invalid.");

        var installation = Path.GetFullPath(plan.InstallationDirectory).TrimEnd(Path.DirectorySeparatorChar);
        var executable = Path.GetFullPath(plan.ApplicationExecutable);
        if (!string.Equals(executable, Path.Combine(installation, "gwgui.exe"), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The application executable is outside the installation root.");
        EnsureInside(work, plan.StartupSignalPath, "startup signal");
        EnsureInside(work, plan.ResultPath, "result");

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var component in plan.Components)
        {
            if (!Enum.IsDefined(component.Kind) || !Enum.IsDefined(component.Operation)
                || !ids.Add(component.ComponentId)
                || component.ComponentId.Length == 0
                || !component.ComponentId.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))
                throw new InvalidDataException("The update plan contains an invalid or duplicate component.");
            if (component.Kind == UpdateComponentKind.Application && component.ComponentId != "gwgui")
                throw new InvalidDataException("The application component id must be gwgui.");
            if (component.Kind == UpdateComponentKind.Application
                && component.Operation != UpdateComponentOperation.UpdateApplication
                || component.Kind == UpdateComponentKind.Module
                && component.Operation == UpdateComponentOperation.UpdateApplication)
                throw new InvalidDataException("The update operation does not match its component kind.");
            EnsureInside(work, component.PreparedDirectory, $"component {component.ComponentId}");
            if (!Directory.Exists(component.PreparedDirectory))
                throw new DirectoryNotFoundException($"Prepared component is missing: {component.ComponentId}");
        }
        if (plan.Components.Count == 0) throw new InvalidDataException("The update plan contains no component.");
        return plan with { InstallationDirectory = installation, ApplicationExecutable = executable };
    }

    internal static void WriteResult(UpdateExecutionPlan plan, UpdateTransactionStatus status, string? detail = null)
    {
        var temporary = plan.ResultPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(
            new UpdateTransactionResult(1, plan.TransactionId, status, detail), JsonOptions));
        File.Move(temporary, plan.ResultPath, overwrite: true);
    }

    private static void EnsureInside(string root, string value, string field)
    {
        var full = Path.GetFullPath(value);
        if (!full.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The {field} path is outside the update work directory.");
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
