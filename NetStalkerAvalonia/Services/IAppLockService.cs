namespace NetStalkerAvalonia.Services
{
    public interface IAppLockService
    {
        void SetPassword(string newPassword, string currentPassword);
        void ClearPassword(string currentPassword);
        public bool IsLocked { get; }
    }
}