using System;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Views
{
    public partial class PasswordView : ReactiveWindow<PasswordViewModel>
    {
        public PasswordView()
        {
            this.WhenActivated(disposables =>
            {
                ViewModel!.CloseWindow = ReactiveCommand.Create(() =>
                {
                    this.Close();
                });
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}