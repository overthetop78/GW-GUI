using GWGUI.MediaEngine.Primitives;
using System.Text;

namespace GWGUI.Tests;

public sealed class Crc16CalculatorTests
{
    [Fact]
    public void ComputeMatchesKnownCcittAndIbmVectors()
    {
        var values = Encoding.ASCII.GetBytes("123456789");

        Assert.Equal(0x29B1, Crc16Calculator.Compute(values));
        Assert.Equal(0xFEE8, Crc16Calculator.Compute(values, Crc16Calculator.IbmPolynomial, Crc16Calculator.ZeroInitialValue));
    }

    [Fact]
    public void SuccessiveUpdatesMatchCompute()
    {
        byte[] values = [0x12, 0x34, 0x56, 0x78, 0x9A];
        var updated = Crc16Calculator.AllBitsSetInitialValue;

        foreach (var value in values) updated = Crc16Calculator.Update(updated, value);

        Assert.Equal(Crc16Calculator.Compute(values), updated);
    }

    [Fact]
    public void ComputeUsesExplicitPolynomialAndInitialValue()
    {
        var values = Encoding.ASCII.GetBytes("123456789");

        Assert.Equal(0x31C3, Crc16Calculator.Compute(values, polynomial: Crc16Calculator.CcittPolynomial, initial: Crc16Calculator.ZeroInitialValue));
    }

    [Fact]
    public void ComputeRejectsNullSequence()
    {
        Assert.Throws<ArgumentNullException>(() => Crc16Calculator.Compute(null!));
    }
}
