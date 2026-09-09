using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;

namespace GWGUI.Tests.Updates;

public sealed class ModuleUpdateCatalogValidatorTests
{
    private readonly ModuleUpdateCatalogValidator _validator = new();

    [Fact]
    public void Valid_single_module_catalog_is_accepted()
    {
        var component = _validator.Validate(Catalog(Component("amiga", Release("1.1.0"))), "amiga");

        Assert.Equal("amiga", component.Id);
    }

    [Fact]
    public void Catalog_identity_must_match_installed_manifest()
    {
        Assert.Throws<InvalidDataException>(() =>
            _validator.Validate(Catalog(Component("atari", Release("1.1.0"))), "amiga"));
    }

    [Fact]
    public void Catalog_must_contain_exactly_one_module()
    {
        var catalog = Catalog(Component("amiga", Release("1.1.0")),
            Component("atari", Release("1.1.0")));

        Assert.Throws<InvalidDataException>(() => _validator.Validate(catalog));
    }

    [Fact]
    public void Catalog_rejects_non_https_package_url()
    {
        var release = Release("1.1.0") with { PackageUrl = "http://example.test/module.zip" };

        Assert.Throws<InvalidDataException>(() =>
            _validator.Validate(Catalog(Component("amiga", release)), "amiga"));
    }

    [Fact]
    public void Plan_builder_selects_latest_compatible_module_version()
    {
        var catalog = Catalog(Component("amiga", Release("1.1.0"), Release("1.3.0"),
            Release("1.2.0")));
        _validator.Validate(catalog, "amiga");

        var result = new UpdatePlanBuilder().Search(catalog,
            new InstalledUpdateState("1.0.0", "1.0",
                [new InstalledModuleVersion("amiga", "1.0.0", "1.0", "1.0")]),
            UpdateSearchScope.Modules);

        Assert.Equal("1.3.0", Assert.Single(result.Plan.Items).Release.Version);
    }

    private static UpdateCatalog Catalog(params UpdateCatalogComponent[] components) =>
        new(2, UpdateCatalogKind.Module, DateTimeOffset.UtcNow, components);

    private static UpdateCatalogComponent Component(string id, params UpdateCatalogRelease[] releases) =>
        new(id, UpdateComponentKind.Module, releases);

    private static UpdateCatalogRelease Release(string version) => new(
        version,
        $"https://example.test/{version}/module.zip",
        new string('b', 64),
        $"https://example.test/{version}/notes",
        HostApiMinimum: "1.0",
        HostApiMaximum: "1.0");
}
