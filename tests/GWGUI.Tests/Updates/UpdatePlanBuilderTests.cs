using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;

namespace GWGUI.Tests.Updates;

public sealed class UpdatePlanBuilderTests
{
    private readonly UpdatePlanBuilder _builder = new();

    [Fact]
    public void Module_catalog_reports_installed_module_as_up_to_date()
    {
        var result = _builder.Search(ModuleCatalog(Module("amiga", ModuleRelease("1.0.0", "1.0", "1.0"))),
            Installed(), UpdateSearchScope.Modules);

        Assert.Equal(UpdateAvailability.UpToDate, Assert.Single(result.Components).Availability);
        Assert.Empty(result.Plan.Items);
    }

    [Fact]
    public void Module_catalog_selects_requested_compatible_version()
    {
        var component = Module("amiga", ModuleRelease("1.2.0", "1.0", "1.0"),
            ModuleRelease("1.1.0", "1.0", "1.0"));

        var result = _builder.Search(ModuleCatalog(component), Installed(), UpdateSearchScope.Modules,
            new Dictionary<string, string> { ["amiga"] = "1.1.0" });

        var update = Assert.Single(result.Components);
        Assert.Equal(UpdateAvailability.Available, update.Availability);
        Assert.Equal("1.1.0", update.SelectedVersion);
        Assert.Equal("1.1.0", Assert.Single(result.Plan.Items).Release.Version);
    }

    [Fact]
    public void Module_catalog_respects_host_api_bounds()
    {
        var result = _builder.Search(ModuleCatalog(Module("amiga", ModuleRelease("2.0.0", "2.0", "2.0"))),
            Installed(), UpdateSearchScope.Modules);

        var update = Assert.Single(result.Components);
        Assert.Equal(UpdateAvailability.ApplicationUpdateRequired, update.Availability);
        Assert.Equal("2.0", update.RequiredHostApiVersion);
        Assert.Empty(result.Plan.Items);
    }

    [Fact]
    public void Application_catalog_cannot_contain_a_module()
    {
        var catalog = new UpdateCatalog(2, UpdateCatalogKind.Application, DateTimeOffset.UtcNow,
            [Application("1.1.0", "1.0"), Module("amiga", ModuleRelease("1.1.0", "1.0", "1.0"))]);

        Assert.Throws<InvalidDataException>(() =>
            _builder.Search(catalog, Installed(), UpdateSearchScope.Application));
    }

    [Fact]
    public void Module_catalog_cannot_be_used_for_application_search()
    {
        Assert.Throws<InvalidDataException>(() => _builder.Search(
            ModuleCatalog(Module("amiga", ModuleRelease("1.1.0", "1.0", "1.0"))),
            Installed(), UpdateSearchScope.Application));
    }

    [Fact]
    public void Application_catalog_builds_application_only_plan()
    {
        var result = _builder.Search(ApplicationCatalog(Application("1.1.0", "1.0")),
            Installed(), UpdateSearchScope.Application);

        var item = Assert.Single(result.Plan.Items);
        Assert.Equal(UpdateComponentKind.Application, item.Kind);
        Assert.Equal("1.1.0", item.Release.Version);
    }

    [Fact]
    public void Application_update_is_blocked_when_installed_module_rejects_target_api()
    {
        var result = _builder.Search(ApplicationCatalog(Application("2.0.0", "2.0")),
            Installed(), UpdateSearchScope.Application);

        Assert.True(result.Plan.IsCompatible);
        Assert.Empty(result.Plan.Items);
        Assert.Equal(UpdateAvailability.Incompatible, Assert.Single(result.Components).Availability);
    }

    private static InstalledUpdateState Installed() => new("1.0.0", "1.0",
        [new InstalledModuleVersion("amiga", "1.0.0", "1.0", "1.0")]);

    private static UpdateCatalog ApplicationCatalog(params UpdateCatalogComponent[] components) =>
        new(2, UpdateCatalogKind.Application, DateTimeOffset.UtcNow, components);

    private static UpdateCatalog ModuleCatalog(params UpdateCatalogComponent[] components) =>
        new(2, UpdateCatalogKind.Module, DateTimeOffset.UtcNow, components);

    private static UpdateCatalogComponent Application(string version, string api) =>
        new("gwgui", UpdateComponentKind.Application, [Release(version, hostApiVersion: api)]);

    private static UpdateCatalogComponent Module(string id, params UpdateCatalogRelease[] releases) =>
        new(id, UpdateComponentKind.Module, releases);

    private static UpdateCatalogRelease ModuleRelease(string version, string minimum, string maximum) =>
        Release(version, minimum: minimum, maximum: maximum);

    private static UpdateCatalogRelease Release(string version, string? hostApiVersion = null,
        string? minimum = null, string? maximum = null) => new(version,
        $"https://example.test/{version}.zip", new string('a', 64),
        $"https://example.test/releases/{version}", hostApiVersion, minimum, maximum);
}
