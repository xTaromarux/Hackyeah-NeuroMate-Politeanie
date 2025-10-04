using NeuroMate.Views;
using Microsoft.Maui.Controls;

namespace NeuroMate.Services
{
    public interface IFloatingAvatarService
    {
        void ShowAvatar();
        void HideAvatar();
        void ShowFeedback(string message);
        void ShowReaction(string reactionType);
        bool IsVisible { get; }
    }

    public class FloatingAvatarService : IFloatingAvatarService
    {
        private Frame? _avatarFrame;
        private bool _isVisible = false;
        private ContentPage? _currentPage;

        public bool IsVisible => _isVisible;

        public void ShowAvatar()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Znajdź aktualną stronę
                    _currentPage = GetCurrentPage();
                    if (_currentPage == null) return;

                    // Utwórz floating avatar jako Frame jeśli nie istnieje
                    if (_avatarFrame == null)
                    {
                        _avatarFrame = CreateFloatingAvatarFrame();
                    }

                    // Dodaj avatar do strony
                    AddAvatarToCurrentPage();
                    _isVisible = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing avatar: {ex.Message}");
                }
            });
        }

        private Frame CreateFloatingAvatarFrame()
        {
            var lottieView = new SkiaSharp.Extended.UI.Controls.SKLottieView
            {
                Source = new SkiaSharp.Extended.UI.Controls.SKFileLottieImageSource
                {
                    File = "turtle.json"
                },
                RepeatCount = -1,
                IsAnimationEnabled = true,
                WidthRequest = 60,
                HeightRequest = 60
            };

            var avatarFrame = new Frame
            {
                WidthRequest = 80,
                HeightRequest = 80,
                CornerRadius = 40,
                BackgroundColor = Colors.White,
                BorderColor = Color.FromArgb("#6366F1"),
                HasShadow = true,
                Padding = 10,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 20, 100),
                Content = lottieView
            };

            // Dodaj gesture recognizer dla tap
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnAvatarTapped;
            avatarFrame.GestureRecognizers.Add(tapGesture);

            return avatarFrame;
        }

        private async void OnAvatarTapped(object? sender, EventArgs e)
        {
            // Pokaż dialog avatara
            var avatarDialog = new FloatingAvatarView();
            await Application.Current?.MainPage?.Navigation.PushModalAsync(avatarDialog);
        }

        private void AddAvatarToCurrentPage()
        {
            if (_currentPage?.Content == null || _avatarFrame == null) return;

            try
            {
                // Sprawdź czy strona już ma Grid jako główny kontener
                if (_currentPage.Content is Grid mainGrid)
                {
                    // Usuń avatar jeśli już istnieje
                    if (mainGrid.Children.Contains(_avatarFrame))
                        mainGrid.Children.Remove(_avatarFrame);

                    // Dodaj avatar na koniec (będzie na wierzchu)
                    mainGrid.Children.Add(_avatarFrame);
                }
                else
                {
                    // Zawiń obecną zawartość w Grid i dodaj avatar
                    var originalContent = _currentPage.Content;
                    var containerGrid = new Grid();
                    
                    containerGrid.Children.Add(originalContent);
                    containerGrid.Children.Add(_avatarFrame);
                    
                    _currentPage.Content = containerGrid;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding avatar to page: {ex.Message}");
            }
        }

        public void HideAvatar()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (_avatarFrame != null && _currentPage?.Content is Grid grid)
                    {
                        if (grid.Children.Contains(_avatarFrame))
                        {
                            grid.Children.Remove(_avatarFrame);
                        }
                    }
                    _isVisible = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error hiding avatar: {ex.Message}");
                }
            });
        }

        public void ShowFeedback(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_avatarFrame != null)
                {
                    // Animacja feedback - pulsowanie
                    _avatarFrame.ScaleTo(1.2, 150, Easing.BounceOut).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _avatarFrame.ScaleTo(1.0, 150, Easing.BounceIn);
                        });
                    });
                }
            });
        }

        public void ShowReaction(string reactionType)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_avatarFrame != null)
                {
                    // Różne animacje w zależności od typu reakcji
                    switch (reactionType.ToLower())
                    {
                        case "success":
                            _avatarFrame.BorderColor = Colors.Green;
                            break;
                        case "warning":
                            _avatarFrame.BorderColor = Colors.Orange;
                            break;
                        case "error":
                            _avatarFrame.BorderColor = Colors.Red;
                            break;
                        default:
                            _avatarFrame.BorderColor = Color.FromArgb("#6366F1");
                            break;
                    }

                    // Animacja bounce
                    _avatarFrame.ScaleTo(1.1, 100, Easing.BounceOut).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _avatarFrame.ScaleTo(1.0, 100, Easing.BounceIn).ContinueWith(__ =>
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    // Wróć do domyślnego koloru po 2 sekundach
                                    Task.Delay(2000).ContinueWith(___ =>
                                    {
                                        MainThread.BeginInvokeOnMainThread(() =>
                                        {
                                            _avatarFrame.BorderColor = Color.FromArgb("#6366F1");
                                        });
                                    });
                                });
                            });
                        });
                    });
                }
            });
        }

        private ContentPage? GetCurrentPage()
        {
            try
            {
                if (Application.Current?.MainPage is Shell shell)
                {
                    return shell.CurrentPage as ContentPage;
                }
                else if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    return navPage.CurrentPage as ContentPage;
                }
                else if (Application.Current?.MainPage is ContentPage page)
                {
                    return page;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
