using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using System.Reflection;

namespace GWGUI.Tests;

/// <summary>Vérifie les identifiants techniques des lecteurs de systèmes de fichiers.</summary>
public sealed class FileSystemIdsTests
{
    /// <summary>Vérifie que chaque lecteur enregistré possède un identifiant central distinct.</summary>
    [Fact]
    public void RegisteredReadersUseEveryCentralIdentifierExactlyOnce()
    {
        var constants = typeof(FileSystemIds).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => Assert.IsType<string>(field.GetRawConstantValue())).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var readerIds = new FileSystemRegistry().Readers.Select(reader => reader.Id).ToArray();
        Assert.Equal(readerIds.Length, readerIds.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.True(readerIds.ToHashSet(StringComparer.OrdinalIgnoreCase).IsSubsetOf(constants));
    }
}
