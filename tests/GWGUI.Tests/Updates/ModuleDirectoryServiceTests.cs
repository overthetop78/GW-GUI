using System.Net;
using System.Net.Http;
using System.Text;
using GWGUI.App.Services.Updates;

namespace GWGUI.Tests.Updates;

public sealed class ModuleDirectoryServiceTests
{
    private static readonly Uri DirectoryUri = new("https://example.test/module-directory.json");

    [Fact]
    public async Task Search_returns_latest_host_compatible_release()
    {
        using var client = Client(
            (DirectoryUri, DirectoryJson(("amiga", "Amiga", "https://example.test/amiga.json"))),
            (new Uri("https://example.test/amiga.json"), CatalogJson("amiga",
                ("1.0.0", "1.0", "1.0"), ("1.2.0", "1.0", "1.0"),
                ("2.0.0", "2.0", "2.0"))));
        var service = new ModuleDirectoryService(client, DirectoryUri,
            installedVersions: () => new Dictionary<string, string>(),
            hostApiVersion: () => "1.0");

        var module = Assert.Single(await service.SearchAsync());

        Assert.Equal("1.2.0", module.AvailableVersion);
        Assert.True(module.CanInstall);
    }

    [Fact]
    public async Task Installed_module_is_listed_but_cannot_be_installed_again()
    {
        using var client = Client(
            (DirectoryUri, DirectoryJson(("amiga", "Amiga", "https://example.test/amiga.json"))),
            (new Uri("https://example.test/amiga.json"), CatalogJson("amiga",
                ("1.0.0", "1.0", "1.0"))));
        var service = new ModuleDirectoryService(client, DirectoryUri,
            installedVersions: () => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["amiga"] = "1.0.0"
            },
            hostApiVersion: () => "1.0");

        var module = Assert.Single(await service.SearchAsync());

        Assert.Equal("1.0.0", module.InstalledVersion);
        Assert.False(module.CanInstall);
    }

    private static HttpClient Client(params (Uri Uri, string Json)[] responses) =>
        new(new JsonResponseHandler(responses.ToDictionary(item => item.Uri, item => item.Json)));

    private static string DirectoryJson(params (string Id, string Name, string CatalogUrl)[] modules) => $$"""
        {
          "schemaVersion": 1,
          "generatedAtUtc": "2026-09-09T12:00:00Z",
          "modules": [
            {{string.Join(",", modules.Select(module => $$"""{"id":"{{module.Id}}","displayName":"{{module.Name}}","catalogUrl":"{{module.CatalogUrl}}"}"""))}}
          ]
        }
        """;

    private static string CatalogJson(string id,
        params (string Version, string Minimum, string Maximum)[] releases) => $$"""
        {
          "schemaVersion": 2,
          "kind": "module",
          "generatedAtUtc": "2026-09-09T12:00:00Z",
          "components": [{
            "id": "{{id}}",
            "kind": "module",
            "releases": [
              {{string.Join(",", releases.Select(release => $$"""{"version":"{{release.Version}}","packageUrl":"https://example.test/{{id}}-{{release.Version}}.zip","sha256":"{{new string('a', 64)}}","notesUrl":"https://example.test/{{id}}/{{release.Version}}","hostApiMinimum":"{{release.Minimum}}","hostApiMaximum":"{{release.Maximum}}"}"""))}}
            ]
          }]
        }
        """;

    private sealed class JsonResponseHandler(IReadOnlyDictionary<Uri, string> responses) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.RequestUri is null || !responses.TryGetValue(request.RequestUri, out var json))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
