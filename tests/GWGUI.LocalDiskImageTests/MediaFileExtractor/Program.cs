using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.MediaFileExtractor;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var imagePath = RequiredArgument(args, "--image");
            var outputDirectory = RequiredArgument(args, "--output");
            var requestedNames = Arguments(args, "--entry");
            if (requestedNames.Count == 0)
                throw new ArgumentException("At least one --entry value is required.");

            var engine = MediaEngineComposition.CreateDefault();
            var source = new MediaSourceDescriptor(Path.GetFullPath(imagePath), []);
            var recognized = await engine.Recognition.Registry.RecognizeAsync(new MediaRecognitionContext(source));
            var explored = await engine.Explorer.ExploreAsync(recognized.Document);
            var entries = explored.Volumes
                .Where(volume => volume.FileSystem is not null)
                .SelectMany(volume => Flatten(volume.FileSystem!.Entries))
                .Where(entry => entry.Kind == FileSystemEntryKind.File)
                .ToArray();

            Directory.CreateDirectory(outputDirectory);
            var manifest = new List<object>();
            foreach (var requestedName in requestedNames)
            {
                var matches = entries.Where(entry => string.Equals(entry.Name, requestedName,
                    StringComparison.OrdinalIgnoreCase)).ToArray();
                if (matches.Length == 0)
                    throw new InvalidDataException($"Media file '{requestedName}' was not found.");
                if (matches.Length > 1)
                    throw new InvalidDataException($"Media file name '{requestedName}' is ambiguous.");

                var entry = matches[0];
                if (entry.Content is null)
                    throw new InvalidDataException($"Media file '{requestedName}' has no extracted content.");

                var content = entry.Content.ToArray();
                var destinationName = SafeFileName(entry.Name);
                var destinationPath = Path.Combine(outputDirectory, destinationName);
                await File.WriteAllBytesAsync(destinationPath, content);
                string? listingPath = null;
                if (TurboBasicXlDetokenizer.TryDecode(content, out var listing))
                {
                    listingPath = destinationPath + ".listing.txt";
                    await File.WriteAllTextAsync(listingPath, listing);
                }
                manifest.Add(new
                {
                    entry.Name,
                    entry.Size,
                    entry.OccupiedSize,
                    entry.StorageReference,
                    entry.MetadataValid,
                    entry.DataValid,
                    entry.NativeTypeId,
                    Length = content.Length,
                    Sha256 = Convert.ToHexString(SHA256.HashData(content)),
                    OutputPath = Path.GetFullPath(destinationPath),
                    ListingPath = listingPath is null ? null : Path.GetFullPath(listingPath)
                });
            }

            var manifestPath = Path.Combine(outputDirectory, "manifest.json");
            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest,
                new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine(Path.GetFullPath(manifestPath));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static IEnumerable<FileSystemEntry> Flatten(IEnumerable<FileSystemEntry> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;
            foreach (var child in Flatten(entry.Children))
                yield return child;
        }
    }

    private static string RequiredArgument(IReadOnlyList<string> args, string name) =>
        Arguments(args, name).FirstOrDefault()
        ?? throw new ArgumentException($"{name} is required.");

    private static IReadOnlyList<string> Arguments(IReadOnlyList<string> args, string name)
    {
        var values = new List<string>();
        for (var index = 0; index < args.Count - 1; index++)
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
                values.Add(args[++index]);
        return values;
    }

    private static string SafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var characters = name.Select(character => invalid.Contains(character) ? '_' : character).ToArray();
        return new string(characters);
    }
}
