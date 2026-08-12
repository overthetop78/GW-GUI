using System.IO;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.Tests;

public sealed class AtariStHybridConversionTests
{
    [Fact]
    public async Task Generation4Number37ConvertsToReadableAtariStImage()
    {
        const string source = @"F:\Disquettes\Génération 4\Génération 4 N°37- Octobre 1991\Génération 4 N°37- Octobre 1991.scp";
        if (!File.Exists(source)) return;
        var output = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.st");
        try
        {
            await MediaEngineFactory.CreateAtariStConversionService().ConvertAsync(source, output, DiskImageFormatIds.AtariSt800);
            var image = await new AtariStReader().ReadAsync(output);
            Assert.Equal(DiskImageFormatIds.AtariSt800, image.FormatId);
            Assert.Equal(800 * 1024, image.Capacity);
            var document = await MediaEngineFactory.CreateDefaultExplorer().ExploreAsync(output);
            var auto = Assert.Single(document.Volume.Entries, entry => entry.Name.Equals("AUTO", StringComparison.OrdinalIgnoreCase));
            Assert.Equal("TERII.PRG", Assert.Single(auto.Children).Name, ignoreCase: true);
        }
        finally { if (File.Exists(output)) File.Delete(output); }
    }
}
