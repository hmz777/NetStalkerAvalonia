using System;

namespace NetStalkerAvalonia.Models
{
    public class Device
    {
        public string IP { get; set; }
        public string Mac { get; set; }
        public bool Blocked { get; set; }
        public bool Redirected { get; set; }
        public string Download { get; set; }
        public string Upload { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
