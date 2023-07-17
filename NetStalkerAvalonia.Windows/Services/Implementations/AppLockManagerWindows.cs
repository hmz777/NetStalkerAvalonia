using NetStalkerAvalonia.Core.Services;
using ReactiveUI;
using Windows.Security.Credentials;

namespace NetStalkerAvalonia.Windows.Services.Implementations
{
	public class AppLockManagerWindows : ReactiveObject, IAppLockService
	{
		// Note: This locking mechanism is not in anyway reliable and could be easily 
		// compromised via scanning the assembly.
		// With that being said, its just a simple locking mechanism that is suitable for this kind of app
		// that doesn't really need locking :)

		private PasswordVault _passwordVault;
		private string _vaultKey = "NSALockKey";
		private string _vaultUsername = "NSA";

		public AppLockManagerWindows()
		{
			_passwordVault = new PasswordVault();
			IsLocked = GetLockStatus();
		}

		#region API

		private bool _isLocked;

		public bool IsLocked
		{
			get => _isLocked;
			set => this.RaiseAndSetIfChanged(ref _isLocked, value);
		}

		public bool SetPassword(string newPassword, string currentPassword)
		{
			if (CheckIfCurrentPasswordCorrect(currentPassword))
			{
				if (SetPasswordInternal(newPassword))
				{
					IsLocked = true;

					return true;
				}
			}

			return false;
		}

		public bool ClearPassword(string currentPassword)
		{
			if (CheckIfCurrentPasswordCorrect(currentPassword))
			{
				if (ClearPasswordInternal())
				{
					IsLocked = false;

					return true;
				}
			}

			return false;
		}

		public bool Unlock(string currentPassword)
		{
			var result = CheckIfCurrentPasswordCorrect(currentPassword);
			if (result)
			{
				return result;
			}

			return result;
		}

		#endregion

		#region Internal

		private bool SetPasswordInternal(string newPassword)
		{
			try
			{
				_passwordVault.Add(new PasswordCredential(_vaultKey, _vaultUsername, newPassword));
			}
			catch
			{
				return false;
			}

			return true;
		}

		private bool ClearPasswordInternal()
		{
			try
			{
				var cred = _passwordVault.Retrieve(_vaultKey, _vaultUsername);

				_passwordVault.Remove(cred);

			}
			catch // PasswordVault throws an exception when no match is found,
				  // so we just swallow the exception since we're clearing the password either way
			{
				return false;
			}

			return true;
		}

		private bool CheckIfCurrentPasswordCorrect(string currentPassword)
		{
			PasswordCredential? cred;

			try
			{
				cred = _passwordVault.Retrieve(_vaultKey, _vaultUsername);
				cred.RetrievePassword();
			}
			catch
			{
				// This means that no password is set
				return true;
			}

			return cred?.Password == currentPassword;
		}

		private bool GetLockStatus()
		{
			PasswordCredential? cred = null;

			try
			{
				cred = _passwordVault.Retrieve(_vaultKey, _vaultUsername);
			}
			catch // We swallow the exception since no match means false
			{
			}

			return cred != null;
		}

		#endregion
	}
}