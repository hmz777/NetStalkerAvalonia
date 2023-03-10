using Avalonia.Controls;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.ViewModels
{
    public class PasswordViewModel : ViewModelBase
    {
        private readonly IAppLockService _appLockService;

        public PasswordViewModel()
        {

        }

        public PasswordViewModel(IAppLockService appLockService)
        {
            this._appLockService = appLockService;

            Submit = ReactiveCommand.Create(() =>
            {
                var result = _appLockService!.Unlock(Password!);

                if (result)
                {
                    _ = this.CloseWindow?.Execute().Subscribe();
                }
                else
                {
                    ErrorMessage = "Invalid password";
                    ShowError = true;
                }
            });

            Exit = ReactiveCommand.Create(Tools.ExitApp);
        }

        public ReactiveCommand<Unit, Unit> Submit { get; set; }
        public ReactiveCommand<Unit, Unit> Exit { get; set; }
        public ReactiveCommand<Unit, Unit> CloseWindow { get; set; }

        private string? _password;

        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private bool _showError;

        public bool ShowError
        {
            get => _showError;
            set => this.RaiseAndSetIfChanged(ref _showError, value);
        }

        private string? _errorMessage;

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }
    }
}