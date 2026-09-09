using GWGUI.Updates.Contracts;
using GWGUI.Updates.Services;

namespace GWGUI.Tests.Updates;

public sealed class ModuleDirectoryCatalogValidatorTests
{
    private readonly ModuleDirectoryCatalogValidator _validator = new();

    [Fact]
    public void Valid_directory_is_sorted_and_accepted()
    {
        var entries = _validator.Validate(Directory(
            Entry("atari", "Atari"), Entry("amiga", "Amiga")));

        Assert.Equal(["amiga", "atari"], entries.Select(entry => entry.Id));
    }

    [Fact]
    public void Unsupported_schema_is_rejected()
    {
        Assert.Throws<InvalidDataException>(() =>
            _validator.Validate(Directory(Entry("amiga", "Amiga")) with { SchemaVersion = 2 }));
    }

    [Fact]
    public void Duplicate_id_is_rejected()
    {
        Assert.Throws<InvalidDataException>(() =>
            _validator.Validate(Directory(Entry("amiga", "Amiga"), Entry("AMIGA", "Another"))));
    }

    [Theory]
    [InlineData("http://example.test/catalog.json")]
    [InlineData("https://example.test/catalog")]
    public void Catalog_url_must_be_direct_https_json(string url)
    {
        Assert.Throws<InvalidDataException>(() =>
            _validator.Validate(Directory(Entry("amiga", "Amiga", url))));
    }

    private static ModuleDirectoryCatalog Directory(params ModuleDirectoryEntry[] modules) =>
        new(1, DateTimeOffset.UtcNow, modules);

    private static ModuleDirectoryEntry Entry(string id, string displayName,
        string? url = null) => new(id, displayName,
            url ?? $"https://example.test/{id}/update-catalog.json");
}
