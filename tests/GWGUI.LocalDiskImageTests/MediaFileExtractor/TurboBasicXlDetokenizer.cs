using System.Globalization;
using System.Text;

namespace GWGUI.MediaFileExtractor;

internal static class TurboBasicXlDetokenizer
{
    private const int HeaderLength = 14;
    private const int TableAddressOffset = 0xF2;
    private const byte RemCommand = 0x00;
    private const byte DataCommand = 0x01;
    private const byte ErrorCommand = 0x37;
    private const byte HexConstantOperator = 0x0D;
    private const byte NumericConstantOperator = 0x0E;
    private const byte StringConstantOperator = 0x0F;
    private const byte AtasciiEndOfLine = 0x9B;

    private static readonly string?[] Commands = BuildCommands();
    private static readonly string?[] Operators = BuildOperators();

    public static bool TryDecode(IReadOnlyList<byte> data, out string listing)
    {
        listing = string.Empty;
        if (!TryReadLayout(data, out var layout))
            return false;

        var variableNames = ReadVariableNames(data, layout.VariableNameStart, layout.VariableValueStart);
        var output = new StringBuilder();
        var linePosition = layout.CodeStart;
        var previousLineNumber = -1;

        while (linePosition + 3 <= data.Count && linePosition < layout.CodeEnd)
        {
            var lineNumber = ReadUInt16(data, linePosition);
            var lineLength = data[linePosition + 2];
            if (lineLength < 5 || linePosition + lineLength > data.Count || linePosition + lineLength > layout.CodeEnd)
                return false;
            if (lineNumber <= previousLineNumber)
                return false;

            previousLineNumber = lineNumber;
            if (lineNumber == 32768)
                break;

            output.Append(lineNumber).Append(' ');
            var statementPosition = linePosition + 3;
            var lineEnd = linePosition + lineLength;
            while (statementPosition < lineEnd)
            {
                var nextStatementOffset = data[statementPosition];
                var statementEnd = linePosition + nextStatementOffset;
                if (nextStatementOffset == 0 || statementEnd <= statementPosition || statementEnd > lineEnd)
                    return false;

                statementPosition++;
                var command = data[statementPosition++];
                AppendCommand(output, command);

                if (command is RemCommand or DataCommand or ErrorCommand)
                {
                    while (statementPosition < statementEnd && data[statementPosition] != AtasciiEndOfLine)
                        AppendAtascii(output, data[statementPosition++]);
                }
                else
                {
                    while (statementPosition < statementEnd)
                    {
                        var token = data[statementPosition++];
                        switch (token)
                        {
                            case 0:
                                if (statementPosition >= statementEnd)
                                    return false;
                                AppendVariable(output, variableNames, data[statementPosition++] | 0x80);
                                break;

                            case NumericConstantOperator:
                            case HexConstantOperator:
                                if (statementPosition + 6 > statementEnd)
                                    return false;
                                AppendNumber(output, data, statementPosition, token == HexConstantOperator);
                                statementPosition += 6;
                                break;

                            case StringConstantOperator:
                                if (statementPosition >= statementEnd)
                                    return false;
                                var stringLength = data[statementPosition++];
                                if (statementPosition + stringLength > statementEnd)
                                    return false;
                                output.Append('"');
                                for (var index = 0; index < stringLength; index++)
                                    AppendAtascii(output, data[statementPosition++]);
                                output.Append('"');
                                break;

                            default:
                                if ((token & 0x80) != 0)
                                    AppendVariable(output, variableNames, token & 0x7F);
                                else
                                    output.Append(Operators[token] ?? $"[OP ${token:X2}]");
                                break;
                        }
                    }
                }

                statementPosition = statementEnd;
            }

            output.AppendLine();
            linePosition = lineEnd;
        }

        listing = output.ToString();
        return listing.Length > 0;
    }

    private static bool TryReadLayout(IReadOnlyList<byte> data, out Layout layout)
    {
        layout = default;
        if (data.Count < HeaderLength || ReadUInt16(data, 0) != 0)
            return false;

        var variableNameTable = ReadUInt16(data, 2);
        var variableValueTable = ReadUInt16(data, 6);
        var statementTable = ReadUInt16(data, 8);
        var stringArrayTable = ReadUInt16(data, 12);
        if (variableNameTable < 0x100 || variableValueTable < variableNameTable ||
            statementTable < variableValueTable || stringArrayTable < statementTable)
            return false;

        var variableNameStart = variableNameTable - TableAddressOffset;
        var variableValueStart = variableValueTable - TableAddressOffset;
        var codeStart = statementTable - TableAddressOffset - (variableNameTable - 0x100);
        var codeEnd = stringArrayTable - TableAddressOffset;
        if (variableNameStart > HeaderLength)
        {
            var adjustment = variableNameStart - HeaderLength;
            variableNameStart -= adjustment;
            variableValueStart -= adjustment;
        }

        if (variableNameStart < HeaderLength || variableValueStart < variableNameStart ||
            codeStart < variableValueStart || codeEnd <= codeStart || codeEnd > data.Count)
            return false;

        layout = new Layout(variableNameStart, variableValueStart, codeStart, codeEnd);
        return true;
    }

    private static IReadOnlyList<string> ReadVariableNames(
        IReadOnlyList<byte> data,
        int variableNameStart,
        int variableValueStart)
    {
        var names = new List<string>();
        var name = new StringBuilder();
        for (var position = variableNameStart; position < variableValueStart; position++)
        {
            var value = data[position];
            if (value == 0 && position == variableValueStart - 1)
                break;

            AppendAtascii(name, (byte)(value & 0x7F));
            if ((value & 0x80) == 0)
                continue;

            names.Add(name.ToString());
            name.Clear();
        }

        return names;
    }

    private static void AppendCommand(StringBuilder output, byte token)
    {
        var command = Commands[token];
        if (command is null)
            output.Append($"[CMD ${token:X2}] ");
        else if (command.Length > 0)
            output.Append(command).Append(' ');
    }

    private static void AppendVariable(StringBuilder output, IReadOnlyList<string> names, int index)
    {
        output.Append(index < names.Count ? names[index] : $"[VAR ${index:X2}]");
    }

    private static void AppendNumber(
        StringBuilder output,
        IReadOnlyList<byte> data,
        int position,
        bool hexadecimal)
    {
        var value = DecodeBcdNumber(data, position);
        if (hexadecimal)
            output.Append('$').Append(((uint)value).ToString(value > 0xFF ? "X4" : "X2", CultureInfo.InvariantCulture));
        else
            output.Append(value.ToString("G", CultureInfo.InvariantCulture));
    }

    private static double DecodeBcdNumber(IReadOnlyList<byte> data, int position)
    {
        var exponentAndSign = data[position];
        if (exponentAndSign == 0)
            return 0;

        var sign = (exponentAndSign & 0x80) == 0 ? 1.0 : -1.0;
        var exponent = (exponentAndSign & 0x7F) - 0x40;
        double value = 0;
        for (var index = 1; index < 6; index++)
        {
            var packed = data[position + index];
            value = value * 100 + (packed >> 4) * 10 + (packed & 0x0F);
        }

        return value * Math.Pow(100, exponent - 4) * sign;
    }

    private static void AppendAtascii(StringBuilder output, byte value)
    {
        var character = value & 0x7F;
        if (character is >= 0x20 and <= 0x7E)
            output.Append((char)character);
        else
            output.Append($"\\x{value:X2}");
    }

    private static ushort ReadUInt16(IReadOnlyList<byte> data, int position) =>
        (ushort)(data[position] | data[position + 1] << 8);

    private static string?[] BuildCommands()
    {
        var values = new string?[256];
        var common = new[]
        {
            "REM", "DATA", "INPUT", "COLOR", "LIST", "ENTER", "LET", "IF",
            "FOR", "NEXT", "GOTO", "GO TO", "GOSUB", "TRAP", "BYE", "CONT",
            "COM", "CLOSE", "CLR", "DEG", "DIM", "END", "NEW", "OPEN",
            "LOAD", "SAVE", "STATUS", "NOTE", "POINT", "XIO", "ON", "POKE",
            "PRINT", "RAD", "READ", "RESTORE", "RETURN", "RUN", "STOP", "POP",
            "?", "GET", "PUT", "GRAPHICS", "PLOT", "POSITION", "DOS", "DRAWTO",
            "SETCOLOR", "LOCATE", "SOUND", "LPRINT", "CSAVE", "CLOAD", "", "ERROR-"
        };
        Array.Copy(common, values, common.Length);

        var turbo = new[]
        {
            "DPOKE", "MOVE", "-MOVE", "*F", "REPEAT", "UNTIL", "WHILE", "WEND",
            "ELSE", "ENDIF", "BPUT", "BGET", "FILLTO", "DO", "LOOP", "EXIT",
            "DIR", "LOCK", "UNLOCK", "RENAME", "DELETE", "PAUSE", "TIME$=", "PROC",
            "EXEC", "ENDPROC", "FCOLOR", "*L", "------------------------------", "RENUM", "DEL", "DUMP",
            "TRACE", "TEXT", "BLOAD", "BRUN", "GO#", "#", "*B", "PAINT",
            "CLS", "DSOUND", "CIRCLE", "%PUT", "%GET"
        };
        Array.Copy(turbo, 0, values, 0x38, turbo.Length);
        return values;
    }

    private static string?[] BuildOperators()
    {
        var values = new string?[256];
        var common = new Dictionary<byte, string>
        {
            [0x12] = ",", [0x13] = "$", [0x14] = ":", [0x15] = ";", [0x16] = "",
            [0x17] = " GOTO ", [0x18] = " GOSUB ", [0x19] = " TO ", [0x1A] = " STEP ",
            [0x1B] = " THEN ", [0x1C] = "#", [0x1D] = "<=", [0x1E] = "<>", [0x1F] = ">=",
            [0x20] = "<", [0x21] = ">", [0x22] = "=", [0x23] = "^", [0x24] = "*",
            [0x25] = "+", [0x26] = "-", [0x27] = "/", [0x28] = " NOT ", [0x29] = " OR ",
            [0x2A] = " AND ", [0x2B] = "(", [0x2C] = ")", [0x2D] = "=", [0x2E] = "=",
            [0x2F] = "<=", [0x30] = "<>", [0x31] = ">=", [0x32] = "<", [0x33] = ">",
            [0x34] = "=", [0x35] = "+", [0x36] = "-", [0x37] = "(", [0x38] = "",
            [0x39] = "", [0x3A] = "(", [0x3B] = "(", [0x3C] = ",", [0x3D] = "STR$",
            [0x3E] = "CHR$", [0x3F] = "USR", [0x40] = "ASC", [0x41] = "VAL", [0x42] = "LEN",
            [0x43] = "ADR", [0x44] = "ATN", [0x45] = "COS", [0x46] = "PEEK", [0x47] = "SIN",
            [0x48] = "RND", [0x49] = "FRE", [0x4A] = "EXP", [0x4B] = "LOG", [0x4C] = "CLOG",
            [0x4D] = "SQR", [0x4E] = "SGN", [0x4F] = "ABS", [0x50] = "INT", [0x51] = "PADDLE",
            [0x52] = "STICK", [0x53] = "PTRIG", [0x54] = "STRIG"
        };
        foreach (var (token, text) in common)
            values[token] = text;

        var turbo = new[]
        {
            "DPEEK", "&", "!", "INSTR", "INKEY$", " EXOR ", "HEX$", "DEC",
            " DIV ", "FRAC", "TIME$", "TIME", " MOD ", " EXEC ", "RND", "RAND",
            "TRUNC", "%0", "%1", "%2", "%3", " GO# ", "UINSTR", "ERR", "ERL"
        };
        Array.Copy(turbo, 0, values, 0x55, turbo.Length);
        return values;
    }

    private readonly record struct Layout(
        int VariableNameStart,
        int VariableValueStart,
        int CodeStart,
        int CodeEnd);
}
