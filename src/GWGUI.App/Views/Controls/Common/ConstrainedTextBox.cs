using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Views.Controls.Common;

/// <summary>Filtre aussi le collage et conserve la dernière valeur complète pendant une saisie intermédiaire.</summary>
internal sealed class ConstrainedTextBox : TextBox
{
    private readonly Func<string, bool> _editingValidation;
    private readonly Func<string, bool> _valueValidation;
    private readonly Func<string, string> _normalize;
    private string _acceptedText;
    private string _committedText;
    private bool _restoring;

    internal string CommittedValue => _normalize(_committedText);

    internal ConstrainedTextBox(string value, Func<string, bool> editingValidation, Func<string, bool> valueValidation,
        Func<string, string>? normalize = null)
    {
        _editingValidation = editingValidation; _valueValidation = valueValidation;
        _normalize = normalize ?? (text => text);
        _acceptedText = _committedText = value;
        SetResourceReference(StyleProperty, typeof(TextBox));
        InputMethod.SetIsInputMethodEnabled(this, false);
        Text = value;
        DataObject.AddPastingHandler(this, (_, args) =>
        {
            if (!args.DataObject.GetDataPresent(DataFormats.UnicodeText)
                || args.DataObject.GetData(DataFormats.UnicodeText) is not string pasted
                || !_editingValidation(Replacement(pasted))) args.CancelCommand();
        });
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs args)
    {
        if (!_editingValidation(Replacement(args.Text))) args.Handled = true;
        base.OnPreviewTextInput(args);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs args)
    {
        if (args.Key == Key.Space) args.Handled = true;
        base.OnPreviewKeyDown(args);
    }

    protected override void OnTextChanged(TextChangedEventArgs args)
    {
        if (_editingValidation is not null && !_restoring)
        {
            if (!_editingValidation(Text)) Restore(_acceptedText);
            else
            {
                _acceptedText = Text;
                if (_valueValidation(Text)) _committedText = Text;
            }
        }
        base.OnTextChanged(args);
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs args)
    {
        if (!_valueValidation(Text)) Restore(_committedText);
        base.OnLostKeyboardFocus(args);
    }

    private string Replacement(string inserted) => Text.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, inserted);

    private void Restore(string value)
    {
        _restoring = true;
        try { Text = _acceptedText = value; CaretIndex = Text.Length; }
        finally { _restoring = false; }
    }
}
