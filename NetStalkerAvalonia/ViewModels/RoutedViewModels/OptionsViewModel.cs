using NetStalkerAvalonia.Helpers;
using NetStalkerAvalonia.Models;
using NetStalkerAvalonia.Services;
using NetStalkerAvalonia.ViewModels.InteractionViewModels;
using ReactiveUI;
using System.Reactive;

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

#if DEBUG

		public OptionsViewModel()
		{

		}

#endif

		[Splat.DependencyInjectionConstructor]
		public OptionsViewModel(IRouter screen, IAppLockService appLockService)
		{
			HostScreen = screen;
			_appLockService = appLockService;

			SetPassword = ReactiveCommand.Create(SetPasswordImpl);
			ClearPassword = ReactiveCommand.Create(ClearPasswordImpl);

			_isAppLocked = this.WhenAnyValue(x => x._appLockService.IsLocked)
						   .ToProperty(this, x => x.IsAppLocked);
		}

		#endregion

		#region Password Section

		private readonly ObservableAsPropertyHelper<bool> _isAppLocked;
		private readonly IAppLockService appLockService;

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
				Tools.ShowMessage(new StatusMessageModel(MessageType.Error, "Current password is incorrect!"));
			else if (string.IsNullOrWhiteSpace(NewPassword))
				Tools.ShowMessage(new StatusMessageModel(MessageType.Error, "New password is invalid!"));
			else
			{
				var result = _appLockService.SetPassword(NewPassword, CurrentPassword);

				if (result == false)
					Tools.ShowMessage(new StatusMessageModel(MessageType.Error, "Unable to set the password!"));
				else
					Tools.ShowMessage(new StatusMessageModel(MessageType.Success, "Password has been set!"));
			}

			ClearPasswordFields();
		}

		public void ClearPasswordImpl()
		{
			if (string.IsNullOrWhiteSpace(CurrentPassword))
				Tools.ShowMessage(new StatusMessageModel(MessageType.Error, "Current password is incorrect!"));
			else
			{
				var result = _appLockService.ClearPassword(CurrentPassword);

				if (result == false)
					Tools.ShowMessage(new StatusMessageModel(MessageType.Error, "Unable to clear the password!"));
				else
					Tools.ShowMessage(new StatusMessageModel(MessageType.Success, "Password has been cleared!"));
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