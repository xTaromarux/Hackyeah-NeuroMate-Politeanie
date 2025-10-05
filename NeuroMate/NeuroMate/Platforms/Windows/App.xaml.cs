using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NeuroMate.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            // Konfiguracja okna - małe, podłużne, w prawym rogu
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();

                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    // Ustaw rozmiar okna: szerokość 350px, wysokość 800px (podłużne)
                    appWindow.Resize(new SizeInt32(350, 800));

                    // Pozycjonuj okno w prawym rogu ekranu
                    var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
                    if (displayArea != null)
                    {
                        var workArea = displayArea.WorkArea;
                        var x = workArea.Width - 350 - 20; // 20px od prawej krawędzi
                        var y = 50; // 50px od góry

                        appWindow.Move(new PointInt32(x, y));
                    }

                    // Ustaw tytuł okna
                    appWindow.Title = "NeuroMate - Asystent Biohackingu";
                }
            });
        }
        #region Win32 interop
#if WINDOWS
        [Flags]
        public enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 0x00000080
        }

        public enum GetWindowLongFields
        {
            GWL_EXSTYLE = -20
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
        }

        public static void SetTaskbarIconVisibility(IntPtr hWnd, bool visible)
        {
            int exStyle = (int)GetWindowLong(hWnd, (int)GetWindowLongFields.GWL_EXSTYLE);

            if (visible)
                exStyle &= ~(int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            else
                exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;

            SetWindowLong(hWnd, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
#endif
        #endregion
    }
#if WINDOWS

    public static class Win32Helper
    {
        [Flags]
        public enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 0x00000080
        }

        public enum GetWindowLongFields
        {
            GWL_EXSTYLE = -20
        }

        const int WM_SIZE = 0x0005;
        const int SIZE_MINIMIZED = 1;
        const int SIZE_RESTORED = 0;

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const int GWL_WNDPROC = -4;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private static WndProcDelegate newWndProc;
        private static IntPtr oldWndProc = IntPtr.Zero;

        public static void SetTaskbarIconVisibility(IntPtr hWnd, bool visible)
        {
            int exStyle = (int)GetWindowLong(hWnd, (int)GetWindowLongFields.GWL_EXSTYLE);

            if (visible)
                exStyle &= ~(int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            else
                exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;

            SetWindowLong(hWnd, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        // ============================
        // Automatyczne ukrywanie/pokazywanie ikony przy minimalizacji
        // ============================
        public static void EnableAutoHideTaskbarIcon(Microsoft.UI.Xaml.Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);

            newWndProc = (hwnd, msg, wParam, lParam) =>
            {
                if (msg == WM_SIZE)
                {
                    if ((int)wParam == SIZE_MINIMIZED)
                    {
                        SetTaskbarIconVisibility(hwnd, false);
                    }
                    else if ((int)wParam == SIZE_RESTORED)
                    {
                        SetTaskbarIconVisibility(hwnd, true);
                    }
                }

                return CallWindowProc(oldWndProc, hwnd, msg, wParam, lParam);
            };

            oldWndProc = SetWindowLong(hWnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));
        }
    }
#endif

}
