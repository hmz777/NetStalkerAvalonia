using System;
using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;

namespace NetStalkerAvalonia.Core.Behaviors;

public class ContextMenuDisableRightClickBehavior : AvaloniaObject
{
    static ContextMenuDisableRightClickBehavior()
    {
        IsContextMenuRightClickDisabledProperty.Changed.Subscribe(
            (x) => { OnIsContextMenuRightClickDisabledChanged(x.Sender, x.NewValue.GetValueOrDefault<bool>()); });
    }

    public static readonly StyledProperty<bool> IsContextMenuRightClickDisabledProperty =
        AvaloniaProperty
            .RegisterAttached<ContextMenuDisableRightClickBehavior, Interactive, bool>(
                "IsContextMenuRightClickDisabled", default(bool), false, BindingMode.OneTime);

    public static void SetIsContextMenuRightClickDisabled(AvaloniaObject element, bool value)
    {
        element.SetValue(IsContextMenuRightClickDisabledProperty, value);
    }

    public static bool GetIsContextMenuRightClickDisabled(AvaloniaObject element)
    {
        return (bool)element.GetValue(IsContextMenuRightClickDisabledProperty);
    }

    private static void OnIsContextMenuRightClickDisabledChanged(AvaloniaObject sender, bool e)
    {
        var element = sender as Control;

        if (element != null)
        {
            var isEnabled = e;

            if (isEnabled)
            {
                element.ContextRequested += OnMouseRightButtonUp;
            }
            else
            {
                element.ContextRequested -= OnMouseRightButtonUp;
            }
        }
    }

    private static void OnMouseRightButtonUp(object sender, ContextRequestedEventArgs e)
    {
        e.Handled = true;
    }
}