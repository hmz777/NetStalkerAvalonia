using NetStalkerAvalonia.Configuration;
using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace NetStalkerAvalonia.ViewModels.RoutedViewModels
{
	public class OptionsViewModel : ViewModelBase, IRoutableViewModel
	{
		#region Routing

		public string? UrlPathSegment { get; } = "Options";
		public IScreen? HostScreen { get; }

		#endregion

		#region Services

		private IAppLockService _appLockService;

		#endregion

		#region Constructors

		public OptionsViewModel()
		{
		}

		public OptionsViewModel(IScreen screen)
		{
			HostScreen = screen;

			_appLockService = Tools.ResolveIfNull<IAppLockService>(null!);

			SetPassword = ReactiveCommand.Create(SetPasswordImpl);
			ClearPassword = ReactiveCommand.Create(ClearPasswordImpl);

			_isAppLocked = this.WhenAnyValue(x => x._appLockService.IsLocked)
						   .ToProperty(this, x => x.IsAppLocked);
		}

		#endregion

		#region Password Section

		private readonly ObservableAsPropertyHelper<bool> _isAppLocked;
		public bool IsAppLocked => _isAppLocked.Value;

		public ReactiveCommand<Unit, Unit> SetPassword { get; set; }
		public ReactiveCommand<Unit, Unit> ClearPassword { get; set; }

		private string? _currentPassword;

		public string? CurrentPassword
		{
			get => _currentPassword;
			set => this.RaiseAndSetIfChanged(ref _currentPassword, value);
		}

		private string? _newPassword;

		public string? NewPassword
		{
			get => _newPassword;
			set => this.RaiseAndSetIfChanged(ref _newPassword, value);
		}

		public void ClearPasswordFields()
		{
			CurrentPassword = null;
			NewPassword = null;
		}

		public void SetPasswordImpl()
		{
			if (IsAppLocked && string.IsNullOrWhiteSpace(CurrentPassword))
				MessageBus.Current.SendMessage<StatusMessage>(new StatusMessage(MessageType.Error, "Current password is incorrect!"), ContractKeys.StatusMessage.ToString());
			else if (string.IsNullOrWhiteSpace(NewPassword))
				MessageBus.Current.SendMessage<StatusMessage>(new StatusMessage(MessageType.Error, "New password is invalid!"), ContractKeys.StatusMessage.ToString());
			else
			{
				var result = _appLockService.SetPassword(NewPassword, CurrentPassword);

				if (result == false)
					MessageBus.Current.SendMessage<StatusMessage>(new StatusMessage(MessageType.Error, "Unable to set the password!"), ContractKeys.StatusMessage.ToString());
				else
					MessageBus.Current.SendMessage<StatusMessage>(new StatusMessage(MessageType.Success, "Password has been set!"), ContractKeys.StatusMessage.ToString());
			}

			ClearPasswordFields();
		}

		public void ClearPasswordImpl()
		{
			if (string.IsNullOrWhiteSpace(CurrentPassword))
				MessageBus.Current.SendMessage<StatusMessage>(new StatusMessage(MessageType.Error, "Current password is incorrect!"), ContractKeys.StatusMessage.ToString());
			else
			{
				var result = _appLockService.ClearPassword(CurrentPassword);

				if (result == false)
					MessageBus.Current.SendMessage<StatusMessage>(new StatusMessage(MessageType.Error, "Unable to clear the password!"), ContractKeys.StatusMessage.ToString());
				else
					MessageBus.Current.SendMessage<StatusMessage>(new StatusMessage(MessageType.Success, "Password has been cleared!"), ContractKeys.StatusMessage.ToString());
			}

			ClearPasswordFields();
		}

		#endregion

		#region Configuration

		private AppSettings _appSettings;
		public AppSettings AppSettings
		{
			get => _appSettings;
			set { this.RaiseAndSetIfChanged(ref _appSettings, value); }
		}

		#endregion
	}
}