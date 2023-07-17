namespace NetStalkerAvalonia.Core.Helpers;

public class LogMessageTemplates
{
    // Exceptions & Errors
    public const string ExceptionTemplate =
        "Exception of type {Type} triggered in service {ServiceType} with message:{Message}";

    // Services
    public const string ServiceResolveError = "Service resolve error: {Message}";
    public const string ServiceInit = "Service of type: {Type}, initialized";
    public const string ServiceStart = "Service of type: {Type}, started";
    public const string ServiceStop = "Service of type: {Type}, stopped";
    public const string ServiceDispose = "Service of type: {Type}, disposed";

    // Ops:Blocking / Redirection
    public const string DeviceBlock = "Service of type: {Type}, Block device with MAC: {Mac} - IP: {Ip}";
    public const string DeviceRedirect = "Service of type: {Type}, Redirect device with MAC: {Mac} - IP: {Ip}";
    public const string DeviceUnblock = "Service of type: {Type}, Unblock device with MAC: {Mac} - IP: {Ip}";
    public const string DeviceUnRedirect = "Service of type: {Type}, UnRedirect device with MAC: {Mac} - IP: {Ip}";

    public const string DeviceLimit =
        "Service of type: {Type}, Limit device with MAC: {Mac} - IP: {Ip} - Download: {Download} - Upload: {Upload}";

    public const string DeviceLimitsClear = "Service of type: {Type}, Limits cleared for device with MAC: {Mac}";

    public const string DeviceDownloadLimitClear =
        "Service of type: {Type}, Clear download limit for device with MAC: {Mac}";

    public const string DeviceUploadLimitClear =
        "Service of type: {Type}, Clear upload limit for device with MAC: {Mac}";

    // Ops:Name Resolving
}