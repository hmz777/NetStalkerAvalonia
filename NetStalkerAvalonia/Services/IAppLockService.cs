namespace NetStalkerAvalonia.Services
{
    public interface IAppLockService
    {
        void Lock();
        void Unlock();
        void SetPassword(string newPassword, string oldPassword);
        void ClearPassword();
    }
}