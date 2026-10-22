using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class CreationContextRecordTests
{
    private sealed record Configuration(string Name);
    private sealed record OtherConfiguration(string Name);

    [Fact]
    public void ConstructionCopyAndDeconstructionKeepFactoriesDeferred()
    {
        var audioCalls = 0;
        var saveCalls = 0;
        Func<IAudioOutput?> audio = () => { audioCalls++; return null; };
        Func<Configuration, string> saves = configuration => { saveCalls++; return configuration.Name; };
        var original = new EmulatorCreationContext<Configuration>("sessions", "core", "host", audio, saves);
        var copy = original with { CorePath = "other-core" };
        var (sessions, core, host, copiedAudio, copiedSaves) = copy;
        Assert.Equal(0, audioCalls);
        Assert.Equal(0, saveCalls);
        Assert.Equal("sessions", sessions);
        Assert.Equal("other-core", core);
        Assert.Equal("host", host);
        Assert.Equal("core", original.CorePath);
        Assert.Same(audio, copiedAudio);
        Assert.Same(saves, copiedSaves);
        Assert.Null(copiedAudio!());
        Assert.Equal("save-target", copiedSaves!(new Configuration("save-target")));
        Assert.Equal(1, audioCalls);
        Assert.Equal(1, saveCalls);
    }

    [Fact]
    public void ValueEqualityKeepsClosedConfigurationTypesSeparate()
    {
        var first = new EmulatorCreationContext<Configuration>("sessions", "core", "host", null, null);
        var equivalent = first with { };
        var different = new EmulatorCreationContext<OtherConfiguration>("sessions", "core", "host", null, null);
        Assert.NotSame(first, equivalent);
        Assert.Equal(first, equivalent);
        Assert.NotEqual(first, first with { CorePath = "changed" });
        Assert.NotEqual((object)first, different);
        Assert.True(typeof(EmulatorCreationContext<Configuration>).IsSealed);
    }

    [Fact]
    public void RecordDoesNotIntroduceValidationOrDefaultFactories()
    {
        var context = new EmulatorCreationContext<Configuration>(null!, null!, null!, null, null);
        Assert.Null(context.SessionsDirectory);
        Assert.Null(context.CorePath);
        Assert.Null(context.HostExecutablePath);
        Assert.Null(context.AudioOutputFactory);
        Assert.Null(context.SaveDirectoryResolver);
    }
}
