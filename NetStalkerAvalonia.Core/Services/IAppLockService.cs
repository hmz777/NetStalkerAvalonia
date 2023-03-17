namespace NetStalkerAvalonia.Services
{
    public interface IAppLockService
    {
        bool SetPassword(string newPassword, string currentPassword);
        bool ClearPassword(string currentPassword);
        bool Unlock(string currentPassword);
        public bool IsLocked { get; }
    }
}