using System.Security;

namespace NetStalkerAvalonia.Services.Implementations.AppLocking
{
    public class AppLockManager : IAppLockService
    {
        private readonly SecureString? _password;

        public void ClearPassword()
        {
            ClearPasswordInternal();
        }

        public void Lock()
        {
            LockInternal();
        }

        public void SetPassword(string newPassword, string oldPassword)
        {
            // Validate old password here

            SetPasswordInternal();
        }

        public void Unlock()
        {
            UnlockInternal();
        }

        private void SetPasswordInternal()
        {

        }

        private void ClearPasswordInternal()
        {

        }

        private void LockInternal()
        {

        }

        private void UnlockInternal()
        {

        }
    }
}
