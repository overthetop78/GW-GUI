using System.Windows;

namespace GWGUI.App.Controls;

public sealed record InputBindingDefinition(string Id, string Label, string DefaultBinding);
public enum InputBindingState { Valid, Conflict, Reserved, Unassigned }
[Flags]
public enum InputCaptureSources { Keyboard = 1, Mouse = 2, Controller = 4 }
public sealed record ControllerCapturedEventArgs(int Port);
public sealed record InputBindingPart(string Text, Visibility SeparatorVisibility);
