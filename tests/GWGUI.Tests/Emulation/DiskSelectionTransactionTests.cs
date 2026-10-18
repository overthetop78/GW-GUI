using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class DiskSelectionTransactionTests
{
    [Theory]
    [InlineData(false, false, "", "get-index|get-eject|eject:True|index:7|eject:False")]
    [InlineData(true, false, "", "get-index|get-eject|index:7|eject:False")]
    [InlineData(false, false, "eject", "get-index|get-eject|eject:True|error:eject")]
    [InlineData(false, false, "select", "get-index|get-eject|eject:True|index:7|error:select|index:3|eject:False")]
    [InlineData(true, false, "select", "get-index|get-eject|index:7|error:select|index:3")]
    [InlineData(false, false, "insert", "get-index|get-eject|eject:True|index:7|eject:False|error:insert|index:3|eject:False")]
    [InlineData(true, false, "insert", "get-index|get-eject|index:7|eject:False|error:insert|index:3")]
    [InlineData(false, true, "select", "get-index|get-eject|eject:True|index:7|error:select|eject:False")]
    [InlineData(true, true, "insert", "get-index|get-eject|index:7|eject:False|error:insert")]
    public void SelectionAndRecoveryKeepNativeCallOrderAndLazyErrors(
        bool wasEjected, bool noPreviousImage, string failure, string expected)
    {
        var calls = new List<string>();
        var error = new InvalidOperationException(failure);
        var insertAttempts = 0;
        Exception MakeError(string operation)
        {
            calls.Add("error:" + operation);
            Assert.Equal(failure, operation);
            return error;
        }
        void Select() => ExternalCoreEnvironmentFunctions.SelectDisk(7,
            () => { calls.Add("get-index"); return noPreviousImage ? uint.MaxValue : 3; },
            () => { calls.Add("get-eject"); return wasEjected; },
            ejected =>
            {
                calls.Add("eject:" + ejected);
                if (ejected) return failure != "eject";
                insertAttempts++;
                return failure != "insert" || insertAttempts > 1;
            },
            index => { calls.Add("index:" + index); return index != 7 || failure != "select"; },
            () => MakeError("eject"), () => MakeError("select"), () => MakeError("insert"));
        if (failure.Length == 0) Select();
        else Assert.Same(error, Assert.Throws<InvalidOperationException>(Select));
        Assert.Equal(expected.Split('|'), calls);
    }

    [Fact]
    public void NativeSelectionExceptionRestoresThePreviousImageBeforeInsertion()
    {
        var calls = new List<string>();
        var error = new InvalidOperationException("native callback");
        var actual = Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.SelectDisk(7, () => 3, () => false,
                ejected => { calls.Add("eject:" + ejected); return true; },
                index =>
                {
                    calls.Add("index:" + index);
                    if (index == 7) throw error;
                    return true;
                },
                UnexpectedError, UnexpectedError, UnexpectedError));
        Assert.Same(error, actual);
        Assert.Equal(new[] { "eject:True", "index:7", "index:3", "eject:False" }, calls);
    }

    [Fact]
    public void RollbackExceptionKeepsTheExistingPropagationAndStopsFurtherRecovery()
    {
        var calls = new List<string>();
        var rollbackError = new InvalidOperationException("rollback callback");
        var originalError = new InvalidOperationException("selection refused");
        var actual = Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreEnvironmentFunctions.SelectDisk(7, () => 3, () => false,
                ejected => { calls.Add("eject:" + ejected); return true; },
                index =>
                {
                    calls.Add("index:" + index);
                    if (index == 3) throw rollbackError;
                    return false;
                },
                UnexpectedError, () => originalError, UnexpectedError));
        Assert.Same(rollbackError, actual);
        Assert.Equal(new[] { "eject:True", "index:7", "index:3" }, calls);
    }

    private static Exception UnexpectedError() => throw new InvalidOperationException("unexpected error factory");
}

