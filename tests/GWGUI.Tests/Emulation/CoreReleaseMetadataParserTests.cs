using GWGUI.Emulation;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreReleaseMetadataParserTests
{
    private enum Emulator { First }
    private enum Model { First }
    private static EmulationCoreCatalogEntry<Emulator, Model> Entry() => new(
        Emulator.First, "id", "library", "library.dll", "archive.zip",
        new Uri("https://example.invalid/archive"), new Uri("https://example.invalid/source"),
        "revision", new HashSet<Model> { Model.First });

    [Theory]
    [InlineData("fr-FR", 0)]
    [InlineData("fr-FR", 1)]
    [InlineData("fr-FR", 2)]
    [InlineData("ar-SA", 0)]
    [InlineData("ar-SA", 1)]
    [InlineData("ar-SA", 2)]
    public async Task OfficialMetadataKeepsOptionalDatesCurrentCultureAndBorrowedResponse(string culture, int dateSource)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new ByteArrayContent([1, 2, 3])
        };
        var settings = new EmulationCoreReleaseSettings(new Uri("https://example.invalid/archive"),
            "archive.dll", "installed.dll", "Nom 日本語");
        var headerDate = new DateTimeOffset(2025, 12, 31, 23, 59, 58, TimeSpan.Zero);
        var contentDate = new DateTimeOffset(2026, 1, 2, 8, 9, 10, TimeSpan.FromHours(2));
        if (dateSource > 0) response.Headers.Date = headerDate;
        if (dateSource > 1) response.Content.Headers.LastModified = contentDate;
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var release = EmulationSdk.CoreReleases.ReadOfficialRelease(settings, response);
            var expectedDate = dateSource switch { 1 => headerDate, 2 => contentDate, _ => (DateTimeOffset?)null };
            Assert.Equal(expectedDate, release.PublishedUtc);
            Assert.Same(settings.OfficialArchiveUri, release.DownloadUri);
            Assert.True(release.IsRequired);
            Assert.True(release.IsZip);
            if (expectedDate is null)
            {
                Assert.Equal("official-latest", release.Id);
                Assert.Equal("Nom 日本語 · latest", release.DisplayName);
            }
            else
            {
                if (culture == "fr-FR")
                    Assert.Equal(dateSource == 1 ? "official-20251231-2359" : "official-20260102-0609", release.Id);
                else
                    Assert.StartsWith("official-1447", release.Id);
                Assert.Equal(expectedDate.Value.LocalDateTime.ToString("dd/MM/yyyy HH:mm") + " · Nom 日本語", release.DisplayName);
            }
            Assert.Equal(new byte[] { 1, 2, 3 }, await response.Content.ReadAsByteArrayAsync());
        }
        finally { CultureInfo.CurrentCulture = previousCulture; }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UsesOriginalDatePriorityUtcFormatAndBorrowedResponse(bool contentDate)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new ByteArrayContent([1, 2, 3])
        };
        response.Headers.Date = new DateTimeOffset(2025, 12, 31, 23, 59, 58, TimeSpan.Zero);
        if (contentDate) response.Content.Headers.LastModified = new DateTimeOffset(2026, 1, 2, 8, 9, 10, TimeSpan.FromHours(2));
        response.Content.Headers.ContentLength = 55;
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var calls = 0;
            var entry = Entry();
            const string prefix = " opaque-日本語-";
            var release = EmulationSdk.CoreReleases.ParseRelease(entry, response, prefix, "yyyyMMdd-HHmmss",
                () => { calls++; return "unused"; });
            var version = contentDate ? "20260102-060910" : "20251231-235958";
            Assert.Equal(version, release.DeclaredVersion);
            Assert.Equal(prefix + version, release.Id);
            Assert.Equal(Emulator.First, release.Emulator);
            Assert.Same(entry.ArchiveUri, release.DownloadUri);
            Assert.Equal(response.Content.Headers.LastModified ?? response.Headers.Date, release.PublishedUtc);
            Assert.Equal(55, release.ExpectedArchiveSize);
            Assert.Equal(0, calls);
            Assert.Equal(new byte[] { 1, 2, 3 }, await response.Content.ReadAsByteArrayAsync());
        }
        finally { CultureInfo.CurrentCulture = previousCulture; }
    }

    [Fact]
    public void MissingDateEvaluatesTranslatedMessageBeforeEntryAccess()
    {
        using var response = new HttpResponseMessage();
        var calls = 0;
        var error = Assert.Throws<InvalidDataException>(() =>
            EmulationSdk.CoreReleases.ParseRelease<Emulator, Model>(null!, response, "prefix", "yyyyMMdd-HHmmss",
                () => { calls++; return "message traduit"; }));
        Assert.Equal("message traduit", error.Message);
        Assert.Equal(1, calls);
        Assert.Null(error.InnerException);
    }

    [Fact]
    public void MissingDateGetterFailureIsNotWrapped()
    {
        using var response = new HttpResponseMessage();
        var original = new InvalidOperationException("translation failed");
        Assert.Same(original, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.CoreReleases.ParseRelease(Entry(), response, "prefix", "yyyyMMdd-HHmmss", () => throw original)));
    }

    [Fact]
    public void ValidDateDoesNotValidateOrEvaluateMissingDateCallback()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Date = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var release = EmulationSdk.CoreReleases.ParseRelease(Entry() with { ArchiveUri = null! }, response,
            null!, "yyyyMMdd-HHmmss", null!);
        Assert.Equal("20260102-030405", release.Id);
        Assert.Null(release.DownloadUri);
    }

    [Fact]
    public void ValidDatePreservesOriginalNullEntryFailure()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Date = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        Assert.Throws<NullReferenceException>(() =>
            EmulationSdk.CoreReleases.ParseRelease<Emulator, Model>(null!, response, "prefix", "yyyyMMdd-HHmmss", null!));
    }
}
