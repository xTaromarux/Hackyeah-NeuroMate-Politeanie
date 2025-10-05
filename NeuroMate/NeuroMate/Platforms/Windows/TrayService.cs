using System;
using System.Runtime.InteropServices;

namespace NeuroMate.Platforms.Windows
{
    public class TrayService
    {
        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;
        private const int NIM_ADD = 0x00000000;
        private const int NIM_DELETE = 0x00000002;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

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

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private NOTIFYICONDATA _notifyIconData;
        private IntPtr _hwnd;

        public void Initialize()
        {
            _hwnd = GetForegroundWindow();

            _notifyIconData = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hWnd = _hwnd,
                uID = 1,
                uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP,
                szTip = "NeuroMate - działa w tle",
                hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00) // standardowa ikona
            };

            Shell_NotifyIcon(NIM_ADD, ref _notifyIconData);
        }

        public void HideWindow()
        {
            ShowWindow(_hwnd, SW_HIDE);
        }

        public void ShowWindow()
        {
            ShowWindow(_hwnd, SW_SHOW);
        }

        public void RemoveTrayIcon()
        {
            Shell_NotifyIcon(NIM_DELETE, ref _notifyIconData);
        }
    }
}
