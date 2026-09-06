using GWGUI.Domain.Naming;
namespace GWGUI.Tests.Application.OutputNaming;
public class OutputNamingTests
{
    [Theory]
    [InlineData(0,SequenceKind.Numeric,3,"000")]
    [InlineData(42,SequenceKind.Numeric,4,"0042")]
    [InlineData(0,SequenceKind.Alphabetic,1,"A")]
    [InlineData(25,SequenceKind.Alphabetic,1,"Z")]
    [InlineData(26,SequenceKind.Alphabetic,1,"AA")]
    [InlineData(701,SequenceKind.Alphabetic,1,"ZZ")]
    [InlineData(702,SequenceKind.Alphabetic,1,"AAA")]
    public void SequenceHasExpectedRepresentation(long value,SequenceKind kind,int width,string expected) =>
        SequenceAndTagsScenarios.Sequence(value,kind,width,expected);
    [Theory]
    [InlineData("",SequenceKind.Numeric)]
    [InlineData("-1",SequenceKind.Numeric)]
    [InlineData("A1",SequenceKind.Alphabetic)]
    [InlineData("ZZZZZZZZZZZZZZZZZZZZ",SequenceKind.Alphabetic)]
    public void InvalidSequenceIsRejected(string text,SequenceKind kind) => SequenceAndTagsScenarios.Invalid(text,kind);
    [Fact] public void InvalidFormattingBoundsAreRejected()=>SequenceAndTagsScenarios.Bounds();
    [Fact] public void TagsUseInjectedTimeAndSource()=>SequenceAndTagsScenarios.Tags();
    [Fact] public void ConflictsUseOnlyInjectedExistence()=>OutputConflictScenarios.Next();
    [Fact] public void ExhaustedSequenceFails()=>OutputConflictScenarios.Exhausted();
}
