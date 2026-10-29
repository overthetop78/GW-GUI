using System.Globalization;
using System.Windows;
using System.Windows.Input;
using GWGUI.App.Views.Controls.Common;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static partial class EmulationSettingsFieldControlFunctions
{
    internal static ConstrainedTextBox CreateNumber(EmulationSettingsField field)
    {
        var range = field.NumericRange ?? new(double.MinValue, double.MaxValue, 1);
        var value = double.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && double.IsFinite(parsed)
            ? Math.Clamp(parsed, range.Minimum, range.Maximum) : Math.Clamp(0, range.Minimum, range.Maximum);
        bool Number(string text, out double number) => double.TryParse(
            text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number) && double.IsFinite(number);
        bool Editing(string text)
        {
            if (range.Step % 1 == 0 && (text.Contains('.') || text.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))) return false;
            if (text.Length == 0 || text == "." || text == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) return true;
            if (range.Minimum < 0 && (text == "-" || text == "-." || text == "-" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)) return true;
            return Number(text, out var candidate) && candidate >= Math.Min(0, range.Minimum) && candidate <= Math.Max(0, range.Maximum)
                && (range.Step % 1 != 0 || candidate % 1 == 0);
        }
        var input = new ConstrainedTextBox(value.ToString("0.################", CultureInfo.CurrentCulture), Editing,
            text => Number(text, out var candidate) && candidate >= range.Minimum && candidate <= range.Maximum,
            text => Number(text, out var candidate) ? candidate.ToString("0.################", CultureInfo.InvariantCulture) : text)
        {
            MinWidth = 100, MaxWidth = 160, HorizontalAlignment = HorizontalAlignment.Left,
            FlowDirection = FlowDirection.LeftToRight
        };
        input.PreviewKeyDown += (_, args) =>
        {
            if (args.Key is not (Key.Up or Key.Down)) return;
            var current = Number(input.Text, out var editing) ? editing : double.Parse(input.CommittedValue, CultureInfo.InvariantCulture);
            var next = Math.Clamp(current + (args.Key == Key.Up ? range.Step : -range.Step), range.Minimum, range.Maximum);
            input.Text = next.ToString("0.################", CultureInfo.CurrentCulture);
            input.CaretIndex = input.Text.Length;
            args.Handled = true;
        };
        return input;
    }

    internal static ConstrainedTextBox CreateIpv4(string value)
    {
        static bool Editing(string text)
        {
            if (text.Length == 0) return true;
            var octets = text.Split('.');
            return octets.Length <= 4 && octets.Select((octet, index) =>
                octet.Length == 0 ? index == octets.Length - 1
                    : octet.Length <= 3 && octet.All(character => character is >= '0' and <= '9')
                      && (octet.Length == 1 || octet[0] != '0') && int.Parse(octet, CultureInfo.InvariantCulture) <= 255).All(valid => valid);
        }
        return new(value, Editing, text => Editing(text) && text.Split('.') is { Length: 4 } octets && octets.All(octet => octet.Length > 0))
        {
            MaxLength = 15, MinWidth = 150, MaxWidth = 240, HorizontalAlignment = HorizontalAlignment.Left,
            FlowDirection = FlowDirection.LeftToRight
        };
    }
}
