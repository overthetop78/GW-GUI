using GWGUI.Domain.Commands.Progress;
namespace GWGUI.Tests.Hardware.CommandsAndParsing;
internal static class ProgressAndLogsScenarios
{
    public static void ErrorDescriptions(int kind, string key)
    {
        Exception error = kind switch
        {
            0 => new TimeoutException(),1 => new OperationCanceledException(),2 => new UnauthorizedAccessException(),
            3 => new InvalidDataException(),4 => new ArgumentException(),5 => new InvalidOperationException(),
            6 => new NotSupportedException(),7 => new OutOfMemoryException(),8 => new DirectoryNotFoundException(),
            9 => new PathTooLongException(),10 => new Exception(),11 => new System.Net.Http.HttpRequestException(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        var expected = GWGUI.App.Localization.Extensions.LocExtension.Get(key);
        Assert.Equal(expected,GWGUI.App.Functions.Localization.ExceptionDescriptionFunctions.Describe(error));
        Assert.Equal(expected,GWGUI.App.Functions.Localization.ExceptionDescriptionFunctions.Describe(
            new System.Reflection.TargetInvocationException(new AggregateException(error))));
    }
    public static async Task Rotation(int maximumFiles)
    {
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryLogFiles();
        var time = new DateTimeOffset(2026,1,2,3,4,5,TimeSpan.Zero);
        var command = new GWGUI.Domain.Commands.GwCommand("virtual tool","read",["virtual image"]);
        var writer = new GWGUI.Infrastructure.Processes.RotatingOperationLogWriter("logs",1,maximumFiles,files,() => time);
        for(var index=0;index<4;index++)
            await writer.WriteAsync(command,new(index,false,TimeSpan.FromSeconds(2),
                [new(time,GWGUI.Domain.Commands.Execution.GwOutputStream.Error,"line-" + index)]));
        Assert.Equal(maximumFiles,files.Files.Count);
        var current = files.Files[Path.Combine("logs","operations.log")];
        Assert.Contains("2026-01-02T03:04:05.0000000+00:00 | exit=3",current);
        Assert.Contains("\"virtual tool\" read \"virtual image\"",current);
        Assert.Contains("[Error] line-3",current);
        if(maximumFiles == 3)
        {
            Assert.Contains("line-2",files.Files[Path.Combine("logs","operations.1.log")]);
            Assert.Contains("line-1",files.Files[Path.Combine("logs","operations.2.log")]);
            Assert.DoesNotContain(files.Files.Values,text => text.Contains("line-0"));
        }
        var before = files.Calls.Count;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer.WriteAsync(command,new(0,false,TimeSpan.Zero,[]),new CancellationToken(true)));
        Assert.Equal(before,files.Calls.Count);
    }
    public static async Task Console(bool archives)
    {
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryLogFiles();
        var settings = new GWGUI.Domain.Settings.Logging.OperationLogSettings { MaximumKilobytes = 1,KeepArchives = archives };
        var time = new DateTimeOffset(2026,1,2,3,4,5,TimeSpan.Zero);
        var session = new GWGUI.Infrastructure.Processes.ConsoleLogSession("logs",() => settings,files,() => time);
        await session.AppendAsync("before start"); Assert.Empty(files.Calls);
        await session.BeginAsync("READ","virtual command"); var active = Path.Combine("logs","read.log");
        Assert.Contains("> virtual command",files.Files[active]);
        var first = new string('é',400); await session.AppendAsync(first);
        await session.AppendAsync(new string('b',800)); await session.AppendAsync(new string('c',800));
        Assert.EndsWith(new string('c',800)+Environment.NewLine,files.Files[active]);
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(files.Files[active]) <= 1024);
        if(archives)
        {
            Assert.Equal(3,files.Files.Count);
            Assert.Contains(first,files.Files[Path.Combine("logs","read-2026-01-02_03-04-05.log")]);
            Assert.Contains(new string('b',800),files.Files[Path.Combine("logs","read-2026-01-02_03-04-05-2.log")]);
        }
        else { Assert.Single(files.Files); Assert.DoesNotContain(first,files.Files[active]); }
        settings.Enabled = false; var calls = files.Calls.Count; await session.AppendAsync("disabled"); Assert.Equal(calls,files.Calls.Count);
        settings.Enabled = true; files.Fail = true; await session.AppendAsync("failure is contained");
        files.Fail = false; settings.MaximumKilobytes = 0; await session.AppendTextAsync("recovered"); Assert.EndsWith("recovered",files.Files[active]);
    }
    public static void Progress()
    {
        var tracker=new GwProgressTracker();
        Assert.Null(tracker.Accept("Reading c=0-1:h=0-1"));
        var retry=tracker.Accept("T0.0: Retry")!;
        Assert.Equal(0,retry.CompletedTracks);Assert.Equal(GwTrackState.Retry,retry.State);
        var first=tracker.Accept("T0.0: done")!;
        Assert.Equal(0.25,first.Fraction);Assert.Equal(0.5,first.HeadFraction);
        Assert.Equal(1,tracker.Accept("T0.0: done")!.CompletedTracks);
        Assert.Equal(GwTrackState.Failed,tracker.Accept("T0.1: error")!.State);
        tracker.Accept("T1.0: done");
        var last=tracker.Accept("T1.1: done")!;
        Assert.Equal(1,last.Fraction);Assert.Null(last.NextCylinder);Assert.Null(last.NextHead);
        tracker.Reset();
        Assert.Null(tracker.Accept("T0.0: done")!.Fraction);
        Assert.Null(tracker.Accept("unrelated output"));
    }
}
