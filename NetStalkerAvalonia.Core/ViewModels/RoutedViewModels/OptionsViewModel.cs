using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Models;
using NetStalkerAvalonia.Core.Services;
using NetStalkerAvalonia.Core.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace NetStalkerAvalonia.Core.ViewModels.RoutedViewModels
{
	public class OptionsViewModel : ViewModelBase, IRoutableViewModel
	{
		#region Routing

		public string? UrlPathSegment { get; } = "Options";
		public IScreen? HostScreen { get; }

		#endregion

		#region Services

		private readonly IAppLockService _appLockService;
		private readonly IStatusMessageService _statusMessageService;

		#endregion

		#region Constructors

#if DEBUG

		public OptionsViewModel()
		{

		}

#endif

		[Splat.DependencyInjectionConstructor]
		public OptionsViewModel(
			IRouter screen,
			IAppLockService appLockService,
			IStatusMessageService statusMessageService)
		{
			HostScreen = screen;
			_appLockService = appLockService;
			_statusMessageService = statusMessageService;

			SetPassword = ReactiveCommand.CreateFromTask(SetPasswordImpl);
			ClearPassword = ReactiveCommand.CreateFromTask(ClearPasswordImpl);

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

		public async Task SetPasswordImpl()
		{
			if (IsAppLocked && string.IsNullOrWhiteSpace(CurrentPassword))
				await _statusMessageService.ShowMessage(new StatusMessageModel(MessageType.Error, "Current password is incorrect!"));
			else if (string.IsNullOrWhiteSpace(NewPassword))
				await _statusMessageService.ShowMessage(new StatusMessageModel(MessageType.Error, "New password is invalid!"));
			else
			{
				var result = _appLockService.SetPassword(NewPassword, CurrentPassword);

				if (result == false)
					await _statusMessageService.ShowMessage(new StatusMessageModel(MessageType.Error, "Unable to set the password!"));
				else
					await _statusMessageService.ShowMessage(new StatusMessageModel(MessageType.Success, "Password has been set!"));
			}

			ClearPasswordFields();
		}

		public async Task ClearPasswordImpl()
		{
			if (string.IsNullOrWhiteSpace(CurrentPassword))
				await _statusMessageService.ShowMessage(new StatusMessageModel(MessageType.Error, "Current password is incorrect!"));
			else
			{
				var result = _appLockService.ClearPassword(CurrentPassword);

				if (result == false)
					await _statusMessageService.ShowMessage(new StatusMessageModel(MessageType.Error, "Unable to clear the password!"));
				else
					await _statusMessageService.ShowMessage(new StatusMessageModel(MessageType.Success, "Password has been cleared!"));
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