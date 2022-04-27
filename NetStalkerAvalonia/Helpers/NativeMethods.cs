using System;
using System.Runtime.InteropServices;

namespace NetStalkerAvalonia.Helpers
{
    public class NativeMethods
    {
        [DllImport("DwmApi")] //System.Runtime.InteropServices
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
    }
}