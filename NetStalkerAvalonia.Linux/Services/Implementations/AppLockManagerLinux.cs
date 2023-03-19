using NetStalkerAvalonia.Core.Helpers;
using NetStalkerAvalonia.Core.Services;
using ReactiveUI;
using System.IO.Abstractions;
using System.Text;

namespace NetStalkerAvalonia.Linux.Services.Implementations
{
	public class AppLockManagerLinux : ReactiveObject, IAppLockService
	{
		// Note: This locking mechanism is not in anyway reliable and could be easily 
		// compromised via scanning the assembly.
		// With that being said, its just a simple locking mechanism that is suitable for this kind of app
		// that doesn't really need locking :)

		private string _lockFilePath = "/etc/opt/NetStalker/lm.ns";
		private string _vaultKey = "NSALockKey";

		private readonly IFileSystem fileSystem;

		public AppLockManagerLinux(IFileSystem fileSystem)
		{
			this.fileSystem = fileSystem;

			IsLocked = IsPasswordSet();
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
				SetPasswordInternal(newPassword);
				IsLocked = true;

				return true;
			}

			return false;
		}

		public bool ClearPassword(string currentPassword)
		{
			if (CheckIfCurrentPasswordCorrect(currentPassword))
			{
				ClearPasswordInternal();
				IsLocked = false;

				return true;
			}

			return false;
		}

		public bool Unlock(string currentPassword) => CheckIfCurrentPasswordCorrect(currentPassword);

		#endregion

		#region Internal

		private bool SetPasswordInternal(string newPassword)
		{
			var passBytes = EncryptionHelper.StringEncrypt(newPassword, key: Encoding.UTF8.GetBytes(_vaultKey));

			if (passBytes == null)
			{
				return false;
			}

			fileSystem.File.WriteAllBytes(_lockFilePath, passBytes);

			return true;
		}

		private void ClearPasswordInternal()
		{
			if (IsPasswordSet())
			{
				fileSystem.File.Delete(_lockFilePath);
			}
		}

		private bool CheckIfCurrentPasswordCorrect(string currentPassword)
		{
			if (IsPasswordSet() == false)
			{
				// No pass is set
				return true;
			}

			var decPassBytes = GetDecryptedBytes();

			if (decPassBytes == null)
			{
				// Someting went wrong
				return false;
			}

			var pass = Encoding.UTF8.GetString(decPassBytes);
			return pass == currentPassword;
		}

		private byte[]? GetDecryptedBytes()
		{
			var encPassBytes = fileSystem.File.ReadAllBytes(_lockFilePath);
			return EncryptionHelper.StringDecrypt(encPassBytes, key: Encoding.UTF8.GetBytes(_vaultKey));
		}

		private bool IsPasswordSet() => fileSystem.File.Exists(_lockFilePath);

		#endregion
	}
}