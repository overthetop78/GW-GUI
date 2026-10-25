using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Updates;
using GWGUI.Emulation.Contracts;
using GWGUI.Updates.Contracts;

namespace GWGUI.Tests.Updates;

public sealed class ModuleUpdateServiceTests
{
    [Theory]
    [InlineData("acorn")]
    [InlineData("example")]
    [InlineData("ACORN")]
    public async Task Search_contacts_only_the_requested_module_even_when_another_catalog_is_unavailable(string moduleId)
    {
        var id = moduleId.ToLowerInvariant();
        using var handler = new CatalogHandler(id);
        using var client = new HttpClient(handler);
        var service = new ModuleUpdateService(client, () => [Package("acorn"), Package("example")], modulesDirectory: Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        var result = await service.SearchAsync(moduleId);

        Assert.Equal(new Uri($"https://example.test/{id}.json"), Assert.Single(handler.Requests));
        var component = Assert.Single(result.Components);
        Assert.Equal(id, component.ComponentId);
        Assert.Equal(UpdateAvailability.Available, component.Availability);
        Assert.True(result.Plan.IsCompatible);
        Assert.Equal(id, Assert.Single(result.Plan.Items).ComponentId);
    }

    [Fact]
    public async Task Search_of_an_unknown_module_does_not_contact_other_catalogs()
    {
        using var handler = new CatalogHandler("acorn");
        using var client = new HttpClient(handler);
        var service = new ModuleUpdateService(client, () => [Package("acorn")], modulesDirectory: Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync("unknown"));

        Assert.Empty(handler.Requests);
    }

    private static LoadedEmulationModule Package(string id) => new(null!, new EmulationModuleManifest(2, id, $"gwgui.emulation.{id}.dll", "1.0.0", "1.2", "1.2", $"https://example.test/{id}.json"), string.Empty);

    private sealed class CatalogHandler(string availableModuleId) : HttpMessageHandler
    {
        internal List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request.RequestUri!);
            if (request.RequestUri != new Uri($"https://example.test/{availableModuleId}.json")) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            var json = $$"""
                {
                  "schemaVersion": 2,
                  "kind": "module",
                  "generatedAtUtc": "2026-10-25T10:00:00Z",
                  "components": [{
                    "id": "{{availableModuleId}}",
                    "kind": "module",
                    "releases": [{
                      "version": "1.1.0",
                      "packageUrl": "https://example.test/{{availableModuleId}}-1.1.0.zip",
                      "sha256": "{{new string('a', 64)}}",
                      "notesUrl": "https://example.test/releases/{{availableModuleId}}-1.1.0",
                      "hostApiMinimum": "1.2",
                      "hostApiMaximum": "1.2"
                    }]
                  }]
                }
                """;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
