namespace NeuroMate
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Dodaj globalną obsługę nieobsłużonych wyjątków
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            MainPage = new AppShell();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            LogException(exception, "UnhandledException");
            
            // W trybie debug, zatrzymaj aplikację aby można było debugować
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception, "UnobservedTaskException");
            e.SetObserved(); // Oznacz jako obsłużone, aby zapobiec crashowi
        }

        private void LogException(Exception? exception, string source)
        {
            if (exception == null) return;

            var errorMessage = $"[{source}] {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                              $"Type: {exception.GetType().Name}\n" +
                              $"Message: {exception.Message}\n" +
                              $"StackTrace: {exception.StackTrace}\n" +
                              $"InnerException: {exception.InnerException?.Message}\n" +
                              new string('-', 50);

            // Log do konsoli debugowania
            System.Diagnostics.Debug.WriteLine(errorMessage);

            // W środowisku produkcyjnym, można dodać logowanie do pliku lub serwisu
            #if DEBUG
            Console.WriteLine(errorMessage);
            #endif

            // Opcjonalnie: wyświetl alert użytkownikowi (tylko dla krytycznych błędów)
            if (MainPage != null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await MainPage.DisplayAlert("Błąd aplikacji", 
                            $"Wystąpił nieoczekiwany błąd: {exception.Message}", "OK");
                    }
                    catch
                    {
                        // Jeśli nie można wyświetlić alertu, nic nie rób
                    }
                });
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            
            var displayInfo = DeviceDisplay.MainDisplayInfo;
            
            window.Created += (s, e) => System.Diagnostics.Debug.WriteLine("Window Created");
            
            window.Width = 390;
            window.Height = 640;
            
            window.X =  displayInfo.Width-window.Width;
            window.Y =  displayInfo.Height-window.Height-40;
            
            return window;
        }
    }
}
