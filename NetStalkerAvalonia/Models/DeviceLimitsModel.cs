namespace NetStalkerAvalonia.Models;

public class DeviceLimitsModel
{
    public int Download { get; set; }
    public int Upload { get; set; }

    public DeviceLimitsModel(int download, int upload)
    {
        Download = download;
        Upload = upload;
    }
}