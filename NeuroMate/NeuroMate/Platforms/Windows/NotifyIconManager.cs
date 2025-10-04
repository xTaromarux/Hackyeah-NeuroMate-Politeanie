using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMate.Platforms.Windows
{
    using System;
    using System.Runtime.InteropServices;

    namespace NeuroMate.Platforms.Windows
    {
        public class NotifyIconManager
        {
            private const int NIF_MESSAGE = 0x00000001;
            private const int NIF_ICON = 0x00000002;
            private const int NIF_TIP = 0x00000004;
            private const int NIM_ADD = 0x00000000;
            private const int NIM_DELETE = 0x00000002;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private struct NOTIFYICONDATA
            {
                public uint cbSize;
                public IntPtr hWnd;
                public uint uID;
                public uint uFlags;
                public uint uCallbackMessage;
                public IntPtr hIcon;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string szTip;
            }

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA pnid);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            private NOTIFYICONDATA notifyIconData;

            public void CreateTrayIcon()
            {
                IntPtr hwnd = GetForegroundWindow();

                notifyIconData = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                    hWnd = hwnd,
                    uID = 1,
                    uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE,
                    szTip = "NeuroMate - działa w tle",
                    hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00) // standardowa ikona
                };

                Shell_NotifyIcon(NIM_ADD, ref notifyIconData);
            }

            public void RemoveTrayIcon()
            {
                Shell_NotifyIcon(NIM_DELETE, ref notifyIconData);
            }
        }
    }

}
