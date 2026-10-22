using System.IO;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using Rpcs3Core = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.CoreConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class CoreDirectoryTests
{
    [Fact]
    public void InstalledSystemAndPackagesSurviveAChangeOfSession()
    {
        const string saves = @"F:\Runtime\States\machine-one\Saves";
        var first = CoreDirectoryFunctions.NativeDataDirectories(Rpcs3Core.Definition, @"F:\Runtime\Sessions\first", saves);
        var second = CoreDirectoryFunctions.NativeDataDirectories(Rpcs3Core.Definition, @"F:\Runtime\Sessions\second", saves);
        Assert.Equal(first, second);
        Assert.StartsWith(Path.Combine(saves, Rpcs3Core.Id) + Path.DirectorySeparatorChar, first.SystemDirectory);
        Assert.StartsWith(Path.Combine(saves, Rpcs3Core.Id) + Path.DirectorySeparatorChar, first.ContentDirectory);
        Assert.NotEqual(first.SystemDirectory, first.ContentDirectory);
    }

    [Fact]
    public void DifferentMachinesAndBackendsDoNotShareInstalledData()
    {
        const string session = @"F:\Runtime\Sessions\first";
        const string saves = @"F:\Runtime\States\machine-one\Saves";
        var first = CoreDirectoryFunctions.NativeDataDirectories(Rpcs3Core.Definition, session, saves);
        var otherMachine = CoreDirectoryFunctions.NativeDataDirectories(Rpcs3Core.Definition, session, @"F:\Runtime\States\machine-two\Saves");
        var otherBackend = CoreDirectoryFunctions.NativeDataDirectories(Rpcs3Core.Definition with { Id = "other-backend" }, session, saves);
        Assert.NotEqual(first.SystemDirectory, otherMachine.SystemDirectory);
        Assert.NotEqual(first.ContentDirectory, otherMachine.ContentDirectory);
        Assert.NotEqual(first.SystemDirectory, otherBackend.SystemDirectory);
        Assert.NotEqual(first.ContentDirectory, otherBackend.ContentDirectory);
    }

    [Fact]
    public void OtherSharedHostBackendsRetainTheirSessionDirectories()
    {
        var others = CoreCatalog.All.Where(definition => definition.Id != Rpcs3Core.Id).ToArray();
        Assert.NotEmpty(others);
        foreach (var definition in others)
        {
            Assert.False(definition.PersistentDataDirectories);
            var first = CoreDirectoryFunctions.NativeDataDirectories(definition, @"F:\Runtime\Sessions\first", @"F:\Runtime\States\Saves");
            var second = CoreDirectoryFunctions.NativeDataDirectories(definition, @"F:\Runtime\Sessions\second", @"F:\Runtime\States\Saves");
            Assert.StartsWith(@"F:\Runtime\Sessions\first\", first.SystemDirectory);
            Assert.StartsWith(@"F:\Runtime\Sessions\first\", first.ContentDirectory);
            Assert.NotEqual(first.SystemDirectory, second.SystemDirectory);
            Assert.NotEqual(first.ContentDirectory, second.ContentDirectory);
        }
    }
}
