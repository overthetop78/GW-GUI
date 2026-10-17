using System.IO;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class ExternalFileSettingsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GamePatchSettingsAreVisibleOutsideGeneralWithoutDiscardingSelections(bool enabled)
    {
        var configuration = Configuration(enabled);
        var fields = new Pcsx2MachineFactory().GetSettingsBlocks(configuration).SelectMany(block => block.Fields).ToArray();
        Assert.Equal(fields.Length, fields.Select(field => field.Id).Distinct().Count());
        var patchIds = new[] { FileSystemConstants.GameIndexPathKey, OptionConstants.UseExternalGameindexKey,
            OptionConstants.HintLanguageUnlockKey, OptionConstants.GameEnhancementsHintKey };
        var patches = patchIds.Select(id => Assert.Single(fields, field => field.Id == id)).ToArray();
        Assert.All(patches, field =>
        {
            Assert.Equal(EmulationMachineTab.Cheats, field.Tab);
            Assert.True(field.IsVisible);
        });
        Assert.Single(patches.Select(field => field.BlockId).Distinct());
        Assert.NotEqual(fields.Single(field => field.Id == OptionConstants.EnableCheatsKey).BlockId, patches[0].BlockId);
        var database = patches.Single(field => field.Id == FileSystemConstants.GameIndexPathKey);
        Assert.Equal(EmulationSettingsEditor.Path, database.Editor);
        Assert.Equal(enabled, database.IsEnabled);
        Assert.Equal(@"C:\databases\selected.yaml", configuration.Options![FileSystemConstants.GameIndexPathKey]);
        var folder = fields.Single(field => field.Id == FileSystemConstants.CheatsDirectoryKey);
        Assert.Equal(EmulationSettingsEditor.OpenDirectory, folder.Editor);
        Assert.True(folder.IsEnabled);
        Assert.Equal(EmulationDefaultFolderCategory.Cheats, folder.DefaultFolderCategory);
        Assert.Equal(configuration.Options[folder.Id], folder.Value);
        Assert.True(folder.RequiresRestart);
        Assert.True(fields.Single(field => field.Id == OptionConstants.EnableCheatsKey).RefreshSettingsOnChange);
        Assert.Equal(EmulationMachineTab.Cheats, folder.Tab);
    }

    [Fact]
    public void Pcee2OnlyReceivesItsOwnCheatFolderSelector()
    {
        var adapter = new GWGUI.Emulation.Sony.Emulators.Pcee2.Factories.Pcee2MachineFactory();
        var fields = adapter.GetSettingsBlocks(new MachineConfiguration(ModelConstants.PlayStation2, "pcee2")).SelectMany(block => block.Fields);
        Assert.DoesNotContain(fields, field => field.Id == FileSystemConstants.GameIndexPathKey);
        Assert.Single(fields, field => field.Id == GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CheatConstants.DirectoryKey);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pcee2CheatFolderAndNativeToggleRoundTripTogether(bool enabled)
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var selected = Assert.IsType<MachineConfiguration>(module.ApplySettings(new MachineConfiguration(ModelConstants.PlayStation2, "pcee2"), new Dictionary<string, string?>
        {
            [GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants.CheatsKey] = enabled ? "enabled" : "disabled",
            [GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CheatConstants.DirectoryKey] = @"C:\codes"
        }));
        var restored = Assert.IsType<MachineConfiguration>(JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(selected)));
        var adapter = new GWGUI.Emulation.Sony.Emulators.Pcee2.Factories.Pcee2MachineFactory();
        var fields = adapter.GetSettingsBlocks(restored).SelectMany(block => block.Fields).ToArray();
        var toggle = Assert.Single(fields, field => field.Id == GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants.CheatsKey);
        var folder = Assert.Single(fields, field => field.Id == GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CheatConstants.DirectoryKey);
        Assert.True(toggle.RefreshSettingsOnChange);
        Assert.Equal(EmulationSettingsEditor.OpenDirectory, folder.Editor);
        Assert.True(folder.IsEnabled);
        Assert.Equal(EmulationDefaultFolderCategory.Cheats, folder.DefaultFolderCategory);
        Assert.Equal(@"C:\codes", folder.Value);
        Assert.Equal(toggle.Tab, folder.Tab);
        Assert.Equal(toggle.BlockId, folder.BlockId);
        Assert.Equal(EmulationMachineTab.Cheats, folder.Tab);
        Assert.Equal(GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants.CheatsHelpResource, folder.ExplanationResourceKey);
        Assert.Equal(enabled ? "enabled" : "disabled", ((GWGUI.Emulation.Sony.Common.Interfaces.IEmulatorSettingsAdapter)adapter).GetRuntimeOptions(restored)[toggle.Id]);
        Assert.Equal("disabled", Assert.Single(new GWGUI.Emulation.Sony.Emulators.Pcee2.Factories.Pcee2MachineFactory()
            .GetSettingsBlocks(new MachineConfiguration(ModelConstants.PlayStation2, "pcee2")).SelectMany(block => block.Fields), field => field.Id == toggle.Id).Value);
    }

    [Fact]
    public void MappedExternalFilesAreReadOnlyAndCheatsCannotTraverseOutsideTheirFolder()
    {
        var configuration = Configuration(true);
        var database = FileSystemFunctions.Resolve(configuration, @"C:\sessions\System", FileSystemConstants.SystemUri + "/" + FileSystemConstants.GameIndexFile);
        Assert.Equal(configuration.Options![FileSystemConstants.GameIndexPathKey], database.Path);
        Assert.True(database.IsReadOnly);
        var cheat = FileSystemFunctions.Resolve(configuration, @"C:\sessions\System", FileSystemConstants.SystemUri + "/pcsx2/cheats/ABCDEF12.pnach");
        Assert.Equal(@"C:\codes\ABCDEF12.pnach", cheat.Path);
        Assert.True(cheat.IsReadOnly);
        Assert.Throws<InvalidDataException>(() => FileSystemFunctions.Resolve(configuration, @"C:\sessions\System", FileSystemConstants.SystemUri + "/pcsx2/cheats/../outside.pnach"));
    }

    [Fact]
    public void DisabledSelectionsDoNotNeedValidationAndKeepTheCheatFolderMountable()
    {
        var configuration = Configuration(false);
        FileSystemFunctions.Validate(configuration);
        Assert.Equal(FileSystemConstants.SystemUri, FileSystemFunctions.SystemDirectory(configuration, @"C:\sessions\System"));
        var database = FileSystemFunctions.Resolve(configuration, @"C:\sessions\System", FileSystemConstants.SystemUri + "/" + FileSystemConstants.GameIndexFile);
        Assert.Equal(@"C:\sessions\System\pcsx2\resources\GameIndex.yaml", database.Path);
        Assert.False(database.IsReadOnly);
    }

    [Fact]
    public void HddMappingRemainsAvailableAlongsideExternalFiles()
    {
        var configuration = Configuration(true) with { Media = [new MediaConfiguration(@"D:\images\disk.raw", MediaCategory.HardDisk, IsReadOnly: true)] };
        var disk = FileSystemFunctions.Resolve(configuration, @"C:\sessions\System", FileSystemConstants.SystemUri + "/" + HardDiskConstants.NativeFile);
        Assert.Equal(@"D:\images\disk.raw", disk.Path);
        Assert.True(disk.IsReadOnly);
    }

    [Theory]
    [InlineData("Auto", false, false)]
    [InlineData("Auto", true, true)]
    [InlineData("Software (SW)", true, false)]
    [InlineData("paraLLEl-GS", true, false)]
    public void DatabaseAvailabilityRespectsTheActualNativePrerequisite(string renderer, bool manualFixes, bool available)
    {
        var configuration = Configuration(true) with { Options = new Dictionary<string, string>(Configuration(true).Options!)
        {
            [OptionConstants.RendererKey] = renderer,
            [OptionConstants.EnableHwHacksKey] = manualFixes ? "enabled" : "disabled"
        } };
        var fields = new Pcsx2MachineFactory().GetSettingsBlocks(configuration).SelectMany(block => block.Fields).ToArray();
        Assert.Equal(available, fields.Single(field => field.Id == OptionConstants.UseExternalGameindexKey).IsEnabled);
        Assert.Equal(available, fields.Single(field => field.Id == FileSystemConstants.GameIndexPathKey).IsEnabled);
        Assert.Equal(manualFixes ? "enabled" : "disabled", configuration.Options![OptionConstants.EnableHwHacksKey]);
    }

    [Fact]
    public void CheatFolderCanBeActivatedAfterStartingWithoutCodes()
    {
        var configuration = Configuration(false);
        var path = FileSystemConstants.SystemUri + "/pcsx2/cheats/00000000.pnach";
        Assert.Equal(@"C:\sessions\System\pcsx2\cheats\00000000.pnach", FileSystemFunctions.Resolve(configuration, @"C:\sessions\System", path, false).Path);
        Assert.Equal(@"C:\codes\00000000.pnach", FileSystemFunctions.Resolve(configuration, @"C:\sessions\System", path, true).Path);
    }

    private static MachineConfiguration Configuration(bool enabled) => new(ModelConstants.PlayStation2, "pcsx2", Options: new Dictionary<string, string>
    {
        [OptionConstants.UseExternalGameindexKey] = enabled ? "enabled" : "disabled",
        [OptionConstants.EnableCheatsKey] = enabled ? "enabled" : "disabled",
        [OptionConstants.EnableHwHacksKey] = "enabled",
        [FileSystemConstants.GameIndexPathKey] = @"C:\databases\selected.yaml",
        [FileSystemConstants.CheatsDirectoryKey] = @"C:\codes"
    });
}
