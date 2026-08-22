using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Emulation.Machine;

public sealed partial class EmulationSection
{
    private void MachineTabMouseDown(object sender, MouseButtonEventArgs args)
    {
        _tabDragStart = args.GetPosition(_machines);
        _draggedMachineTab = Ancestor<TabItem>(args.OriginalSource as DependencyObject);
        if (_draggedMachineTab is null || _machines.Items.IndexOf(_draggedMachineTab) <= 0
            || Ancestor<Button>(args.OriginalSource as DependencyObject) is not null)
            _draggedMachineTab = null;
        else
            _tabDragOffset = args.GetPosition(_draggedMachineTab);
    }

    private void MachineTabMouseMove(object sender, MouseEventArgs args)
    {
        if (_draggedMachineTab is null || args.LeftButton != MouseButtonState.Pressed) return;
        var position = args.GetPosition(_machines);
        if (Math.Abs(position.X - _tabDragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(position.Y - _tabDragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;
        var tab = _draggedMachineTab;
        _draggedMachineTab = null;
        var layer = AdornerLayer.GetAdornerLayer(_machines);
        if (layer is not null)
        {
            _tabDragAdorner = new MachineTabDragAdorner(_machines, tab, _tabDragOffset);
            layer.Add(_tabDragAdorner);
        }
        try
        {
            DragDrop.DoDragDrop(_machines, tab, DragDropEffects.Move);
        }
        finally
        {
            if (_tabDragAdorner is not null) layer?.Remove(_tabDragAdorner);
            _tabDragAdorner = null;
        }
    }

    private void MachineTabDragOver(object sender, DragEventArgs args)
    {
        var dragged = args.Data.GetData(typeof(TabItem)) as TabItem;
        var target = Ancestor<TabItem>(args.OriginalSource as DependencyObject);
        args.Effects = dragged is not null && target is not null
            && _machines.Items.IndexOf(dragged) > 0 && _machines.Items.IndexOf(target) > 0
            ? DragDropEffects.Move : DragDropEffects.None;
        var position = args.GetPosition(_machines);
        double? insertionX = null;
        if (args.Effects == DragDropEffects.Move && target is not null)
        {
            var bounds = target.TransformToAncestor(_machines)
                .TransformBounds(new Rect(new Point(), target.RenderSize));
            insertionX = position.X > bounds.Left + bounds.Width / 2 ? bounds.Right : bounds.Left;
        }
        _tabDragAdorner?.Update(position, insertionX);
        args.Handled = true;
    }

    private void MachineTabDrop(object sender, DragEventArgs args)
    {
        var dragged = args.Data.GetData(typeof(TabItem)) as TabItem;
        var target = Ancestor<TabItem>(args.OriginalSource as DependencyObject);
        if (dragged is null || target is null || ReferenceEquals(dragged, target)) return;
        MoveMachineTab(dragged, target, args.GetPosition(_machines).X);
        _machines.SelectedItem = dragged;
        args.Handled = true;
    }

    private void MoveMachineTab(TabItem dragged, TabItem target, double pointerX)
    {
        var sourceIndex = _machines.Items.IndexOf(dragged);
        var targetIndex = _machines.Items.IndexOf(target);
        if (sourceIndex <= 0 || targetIndex <= 0) return;
        var bounds = target.TransformToAncestor(_machines)
            .TransformBounds(new Rect(new Point(), target.RenderSize));
        var insertionIndex = targetIndex +
            (pointerX > bounds.Left + bounds.Width / 2 ? 1 : 0);
        if (sourceIndex < insertionIndex) insertionIndex--;
        if (sourceIndex == insertionIndex) return;
        _machines.Items.RemoveAt(sourceIndex);
        _machines.Items.Insert(Math.Clamp(insertionIndex, 1, _machines.Items.Count), dragged);
        _machines.SelectedItem = dragged;
    }

    private static T? Ancestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
