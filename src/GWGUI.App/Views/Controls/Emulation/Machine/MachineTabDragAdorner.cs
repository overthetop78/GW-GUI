using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Emulation.Machine;

internal sealed class MachineTabDragAdorner : Adorner
{
    private readonly VisualBrush _tabBrush;
    private readonly Size _tabSize;
    private readonly Point _pointerOffset;
    private Point _pointer;
    private double? _insertionX;

    internal MachineTabDragAdorner(
        UIElement adornedElement, TabItem tab, Point pointerOffset)
        : base(adornedElement)
    {
        IsHitTestVisible = false;
        _tabBrush = new VisualBrush(tab) { Opacity = 0.78, Stretch = Stretch.None };
        _tabSize = tab.RenderSize;
        _pointerOffset = pointerOffset;
        _pointer = pointerOffset;
    }

    internal void Update(Point pointer, double? insertionX)
    {
        _pointer = pointer;
        _insertionX = insertionX;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var origin = new Point(_pointer.X - _pointerOffset.X, _pointer.Y - _pointerOffset.Y);
        drawingContext.PushOpacity(0.88);
        drawingContext.DrawRoundedRectangle(_tabBrush, new Pen(Brushes.DodgerBlue, 1.5),
            new Rect(origin, _tabSize), 9, 9);
        drawingContext.Pop();
        if (_insertionX is { } x)
            drawingContext.DrawLine(new Pen(Brushes.DodgerBlue, 3), new Point(x, 2),
                new Point(x, Math.Max(2, _tabSize.Height - 2)));
    }
}
