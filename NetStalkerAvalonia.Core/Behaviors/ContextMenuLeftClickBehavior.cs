using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;

namespace NetStalkerAvalonia.Core.Behaviors;

public class ContextMenuLeftClickBehavior : AvaloniaObject
{
    static ContextMenuLeftClickBehavior()
    {
        IsLeftClickEnabledProperty.Changed.Subscribe(
            (x) => { OnIsLeftClickEnabledChanged(x.Sender, x.NewValue.GetValueOrDefault<bool>()); });
    }

    public static bool GetIsLeftClickEnabled(AvaloniaObject obj)
    {
        return obj.GetValue(IsLeftClickEnabledProperty);
    }

    public static void SetIsLeftClickEnabled(AvaloniaObject obj, bool value)
    {
        obj.SetValue(IsLeftClickEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsLeftClickEnabledProperty =
        AvaloniaProperty
            .RegisterAttached<ContextMenuLeftClickBehavior, Interactive, bool>(
                "IsLeftClickEnabled", default(bool), false, BindingMode.OneTime);

    private static void OnIsLeftClickEnabledChanged(IAvaloniaObject sender, bool e)
    {
        var uiElement = sender as Control;

        if (uiElement != null)
        {
            // var isEnabled = e.NewValue is bool && (bool)e.NewValue;
            var isEnabled = e;

            if (isEnabled)
            {
                if (uiElement is Button)
                    ((Button)uiElement).Click += OnMouseLeftButtonUp;
                else
                    uiElement.KeyUp += OnMouseLeftButtonUp;
            }
            else
            {
                if (uiElement is Button)
                    ((Button)uiElement).Click -= OnMouseLeftButtonUp;
                else
                    uiElement.KeyUp -= OnMouseLeftButtonUp;
            }
        }
    }

    private static void OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        var fe = sender as Control;
        if (fe != null)
        {
            if (fe.ContextMenu!.DataContext == null)
            {
                fe.ContextMenu.Bind(StyledElement.DataContextProperty,
                    fe.GetObservable(StyledElement.DataContextProperty));
            }

            fe.ContextMenu.Open(fe);
        }
    }
}