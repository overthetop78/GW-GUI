namespace GWGUI.Tests.Hardware.CommandsAndParsing;
public class CommandsAndParsingTests
{
    [Theory] [InlineData(false)] [InlineData(true)] public void HardwareInfoParsesFieldsAndRejectsMisleadingPrefixes(bool malformed) => ExternalOutputParsingScenarios.Information(malformed);
    [Fact] public void ConflictingOptionsAndUnclosedExpertQuotesAreRejected() => CommandArgumentsScenarios.ExclusiveAndExpert();
    [Theory]
    [InlineData("--revs","0",false)] [InlineData("--retries","-1",false)] [InlineData("--retries","2147483648",false)]
    [InlineData("tracks","c=0:h=0",false)] [InlineData("--raw","true",false)] [InlineData("--densel","X",false)]
    [InlineData("--tracks","c=2-1:h=0",false)] [InlineData("--tracks","c=0:h=2",false)] [InlineData("--tracks","c=0:h=0:c=1",false)]
    [InlineData("--pll","lowpass=0",false)] [InlineData("--pll","unknown=1",false)] [InlineData("--precomp","type=unknown:40=125",false)]
    [InlineData("--precomp","type=mfm",false)] [InlineData("--adjust-speed","0rpm",false)] [InlineData("--fake-index","text",false)]
    [InlineData("--raw",null,true)] [InlineData("--retries","0",true)] [InlineData("--revs","1",true)]
    [InlineData("--tracks","c=0-4/2:h=0-1:step=1/2:h0.off=+1:hswap",true)] [InlineData("--pll","period=5:phase=60:lowpass=1.5",true)]
    [InlineData("--precomp","type=mfm:40=125",true)] [InlineData("--adjust-speed",".5ms",true)]
    public void OptionArgumentsValidateValuesWithoutSplittingPaths(string argument,string? value,bool valid) => CommandArgumentsScenarios.Options(argument,value,valid);
    [Theory]
    [InlineData(0,"Error.Description.OperationTimeout")] [InlineData(1,"Error.Description.OperationCancelled")]
    [InlineData(2,"Error.Description.AccessDenied")] [InlineData(3,"Error.Description.InvalidData")]
    [InlineData(4,"Error.Description.InvalidArgument")] [InlineData(5,"Error.Description.InvalidOperation")]
    [InlineData(6,"Error.Description.NotSupported")] [InlineData(7,"Error.Description.OutOfMemory")]
    [InlineData(8,"Error.Description.DirectoryNotFound")] [InlineData(9,"Error.Description.PathTooLong")]
    [InlineData(10,"Error.Description.Unexpected")] [InlineData(11,"Error.Description.NetworkUnavailable")]
    public void ErrorDescriptionsPreserveWrappedCause(int kind,string key) => ProgressAndLogsScenarios.ErrorDescriptions(kind,key);
    [Theory] [InlineData(1)] [InlineData(3)]
    public Task OperationLogsRotateAndPreserveNewestEntries(int count) => ProgressAndLogsScenarios.Rotation(count);
    [Theory] [InlineData(false)] [InlineData(true)]
    public Task ConsoleLogsTrimOrArchiveAndRecover(bool archives) => ProgressAndLogsScenarios.Console(archives);
    [Fact] public void CapabilitiesParserUsesOnlyDeclaredSections()=>ExternalOutputParsingScenarios.Parse();
    [Fact] public void TrackProgressCountsEachTrackOnce()=>ProgressAndLogsScenarios.Progress();
    [Fact] public void ReadArgumentsPreservePathsAndExpertTokens()=>CommandArgumentsScenarios.Read();
    [Fact] public void WriteArgumentsPreserveTargetAndVerification()=>CommandArgumentsScenarios.Write();
    [Fact] public void InvalidRequestsAreRejectedBeforeExecution()=>CommandArgumentsScenarios.Invalid();
    [Fact] public void MaintenancePinAndSeekHaveDistinctArguments()=>CommandArgumentsScenarios.Tools();
}
