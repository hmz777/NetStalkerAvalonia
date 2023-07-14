using System;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NetStalkerAvalonia.Core.ViewModels;
using ReactiveUI;

namespace NetStalkerAvalonia.Core.Views
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

            InitializeComponent();
        }
    }
}