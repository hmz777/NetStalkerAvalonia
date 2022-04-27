using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Reactive;
using System.Windows.Input;

namespace NetStalkerAvalonia.Helpers
{
    public class AttachedProperties : AvaloniaObject
    {
        public AttachedProperties()
        {
            CommandProperty
                .Changed
                .Subscribe(Observer
                .Create<AvaloniaPropertyChangedEventArgs<ICommand>>(x => HandleCommandChanged(x.Sender, x.NewValue.GetValueOrDefault<ICommand>())));
        }

        public static readonly AttachedProperty<string> IconProperty = AvaloniaProperty.RegisterAttached<AttachedProperties, Interactive, string>(
                            "Icon", default(string), false, Avalonia.Data.BindingMode.OneWay, null);

        public static readonly AttachedProperty<string> TextProperty = AvaloniaProperty.RegisterAttached<AttachedProperties, Interactive, string>(
                           "Text", default(string), false, Avalonia.Data.BindingMode.OneWay, null);

        public static readonly AttachedProperty<ICommand> CommandProperty = AvaloniaProperty.RegisterAttached<AttachedProperties, Interactive, ICommand>(
               "Command", default(ICommand), false, BindingMode.OneTime);

        public static readonly AttachedProperty<object> CommandParameterProperty = AvaloniaProperty.RegisterAttached<AttachedProperties, Interactive, object>(
                     "CommandParameter", default(object), false, BindingMode.OneWay, null);

        /// <summary>
        /// Accessor for Attached property <see cref="IconProperty"/>.
        /// </summary>
        public static void SetIcon(AvaloniaObject element, object parameter)
        {
            element.SetValue(IconProperty, parameter);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="IconProperty"/>.
        /// </summary>
        public static object GetIcon(AvaloniaObject element)
        {
            return element.GetValue(IconProperty);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="TextProperty"/>.
        /// </summary>
        public static void SetText(AvaloniaObject element, object parameter)
        {
            element.SetValue(TextProperty, parameter);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="TextProperty"/>.
        /// </summary>
        public static object GetText(AvaloniaObject element)
        {
            return element.GetValue(TextProperty);
        }

        /// <summary>
        /// <see cref="CommandProperty"/> changed event handler.
        /// </summary>
        private static void HandleCommandChanged(IAvaloniaObject element, ICommand commandValue)
        {
            if (element is Interactive interactElem)
            {
                if (commandValue != null)
                {
                    // Add non-null value
                    interactElem.AddHandler(InputElement.DoubleTappedEvent, Handler);
                }
                else
                {
                    // remove prev value
                    interactElem.RemoveHandler(InputElement.DoubleTappedEvent, Handler);
                }
            }

            // local handler fcn
            void Handler(object s, RoutedEventArgs e)
            {
                // This is how we get the parameter off of the gui element.
                object commandParameter = interactElem.GetValue(CommandParameterProperty);
                if (commandValue?.CanExecute(commandParameter) == true)
                {
                    commandValue.Execute(commandParameter);
                }
            }
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandProperty"/>.
        /// </summary>
        public static void SetCommand(AvaloniaObject element, ICommand commandValue)
        {
            element.SetValue(CommandProperty, commandValue);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandProperty"/>.
        /// </summary>
        public static ICommand GetCommand(AvaloniaObject element)
        {
            return element.GetValue(CommandProperty);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandParameterProperty"/>.
        /// </summary>
        public static void SetCommandParameter(AvaloniaObject element, object parameter)
        {
            element.SetValue(CommandParameterProperty, parameter);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandParameterProperty"/>.
        /// </summary>
        public static object GetCommandParameter(AvaloniaObject element)
        {
            return element.GetValue(CommandParameterProperty);
        }
    }
}
