namespace NeuroMate.Helpers
{
    /// <summary>
    /// Rozszerzenia dla Shell umożliwiające wyświetlanie toastów i snackbarów
    /// </summary>
    public static class ShellExtensions
    {
        /// <summary>
        /// Wyświetla krótki komunikat u dołu ekranu (snackbar/toast)
        /// </summary>
        public static async Task DisplaySnackbar(this Shell shell, string message, int durationMs = 3000)
        {
            if (shell?.CurrentPage == null)
                return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Tworzymy prostą implementację snackbara
                var snackbar = new Frame
                {
                    BackgroundColor = Color.FromArgb("#323232"),
                    CornerRadius = 8,
                    Padding = new Thickness(16, 12),
                    Margin = new Thickness(16, 0, 16, 16),
                    HasShadow = true,
                    Content = new Label
                    {
                        Text = message,
                        TextColor = Colors.White,
                        FontSize = 14
                    }
                };

                // Dodaj do layoutu strony - sprawdzamy czy Content jest ContentView
                if (shell.CurrentPage is ContentPage contentPage && contentPage.Content is Layout layout)
                {
                    // Dla różnych typów layoutów
                    if (layout is Grid grid)
                    {
                        // Ustaw snackbar na dole
                        grid.Children.Add(snackbar);
                        if (grid.RowDefinitions.Count > 0)
                        {
                            Grid.SetRow(snackbar, grid.RowDefinitions.Count - 1);
                        }
                        snackbar.VerticalOptions = LayoutOptions.End;
                    }
                    else if (layout is StackLayout stackLayout)
                    {
                        stackLayout.Children.Add(snackbar);
                    }

                    // Animacja wejścia
                    snackbar.Opacity = 0;
                    snackbar.TranslationY = 50;
                    
                    await Task.WhenAll(
                        snackbar.FadeTo(1, 250),
                        snackbar.TranslateTo(0, 0, 250, Easing.CubicOut)
                    );

                    // Czekaj
                    await Task.Delay(durationMs);

                    // Animacja wyjścia
                    await Task.WhenAll(
                        snackbar.FadeTo(0, 250),
                        snackbar.TranslateTo(0, 50, 250, Easing.CubicIn)
                    );

                    // Usuń z layoutu
                    if (layout is Grid grid2)
                    {
                        grid2.Children.Remove(snackbar);
                    }
                    else if (layout is StackLayout stackLayout2)
                    {
                        stackLayout2.Children.Remove(snackbar);
                    }
                }
            });
        }
    }
}