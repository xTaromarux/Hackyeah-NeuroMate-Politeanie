using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;

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
    }

}
