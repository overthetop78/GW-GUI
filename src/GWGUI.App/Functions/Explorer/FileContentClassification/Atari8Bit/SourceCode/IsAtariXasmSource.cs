using System.Text;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtariXasmSource(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 32 })
        {
            return false;
        }

        var source = new StringBuilder(data.Count);
        var lineEndCount = 0;
        foreach (var value in data)
        {
            if (value is 0x9b or 0x0a or 0x0d)
            {
                source.Append(' ');
                lineEndCount++;
            }
            else if (value == 0x7f)
            {
                source.Append(' ');
            }
            else if (value is >= 0x20 and <= 0x7e)
            {
                source.Append(char.ToLowerInvariant((char)value));
            }
            else
            {
                return false;
            }
        }

        if (lineEndCount < 3)
        {
            return false;
        }

        var text = $" {source} ";
        var directiveCount = new[] { " equ ", " org ", " dta ", " run ", " end ", " icl ", " opt " }
            .Count(text.Contains);
        var instructionCount = new[]
        {
            " lda ", " ldx ", " ldy ", " sta ", " stx ", " sty ", " adc ", " sbc ", " cmp ",
            " cpx ", " cpy ", " and ", " eor ", " ora ", " asl ", " lsr ", " rol ", " ror ",
            " inc ", " dec ", " inx ", " iny ", " dex ", " dey ", " jmp ", " jsr ", " rts ",
            " bcc ", " bcs ", " beq ", " bmi ", " bne ", " bpl ", " bvc ", " bvs "
        }.Count(text.Contains);
        var xasmSyntaxCount = new[]
        {
            " dta ", " mva ", " mwa ", ":rpl ", ":rne ", ":seq ", ":sne ", " ^0", " ^1", " ^2", " ^3", " ^4"
        }.Count(text.Contains);

        return directiveCount >= 2 && instructionCount >= 3 && xasmSyntaxCount >= 2;
    }
}
