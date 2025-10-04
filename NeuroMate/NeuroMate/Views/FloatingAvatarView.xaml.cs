using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuroMate.Views;

public partial class FloatingAvatarView : ContentPage
{
    private bool _isAnimating = true;

    private readonly List<string> _motivationalMessages = new()
    {
        "Czas na przerwę! Wykonajmy razem szybki test Stroop 🎨",
        "Zauważyłem, że pracujesz już długo. Może krótka gra na reakcję? ⚡",
        "Twoja koncentracja może potrzebować odświeżenia. Spróbujmy Task Switching! 🔄",
        "Pora na mikro-przerwę! Wykonaj ze mną ćwiczenia oczu 👁️",
        "Twój mózg potrzebuje wyzwania. Zagrajmy w N-back! 🧠",
        "Czas na ruch! Wstań i rozciągnij się przez minutę 🤸‍♂️",
        "Może coś do picia? Nawodnienie jest ważne dla mózgu! 💧",
        "Świetna robota! Twoje wyniki się poprawiają 📈",
        "Pamiętaj o regularnych przerwach. Twój mózg Ci podziękuje! 🧘‍♂️"
    };

    private readonly Random _random = new();

    public FloatingAvatarView()
    {
        InitializeComponent();
        SetRandomMessage();
    }

    public FloatingAvatarView(string customMessage) : this()
    {
        AvatarMessageLabel.Text = customMessage;
    }

    private void SetRandomMessage()
    {
        var message = _motivationalMessages[_random.Next(_motivationalMessages.Count)];
        AvatarMessageLabel.Text = message;
    }

    private void OnAvatarTapped(object sender, EventArgs e)
    {
        // Prosta animacja reakcji
        ShowReactionAnimation("tap");

        // Opcjonalna akcja przy kliknięciu
        AvatarTapped?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? AvatarTapped;

    public void ShowFeedback(string message)
    {
        // Uproszczona wersja - tylko animacja skalowania avatara
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DialogAvatarLottie.ScaleTo(1.2, 150, Easing.BounceOut).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DialogAvatarLottie.ScaleTo(1.0, 150, Easing.BounceIn);
                });
            });
        });
    }

    public void UpdateAnimationState(bool shouldAnimate)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            DialogAvatarLottie.IsAnimationEnabled = shouldAnimate;
        });
    }

    public void ShowReactionAnimation(string reactionType)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Prosta animacja pulsowania dla wszystkich typów reakcji
            DialogAvatarLottie.ScaleTo(1.1, 100, Easing.BounceOut).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DialogAvatarLottie.ScaleTo(1.0, 100, Easing.BounceIn);
                });
            });
        });
    }

    public async Task ShowAsync()
    {
        DialogOverlay.IsVisible = true;

        // Animacja wejścia
        await Task.WhenAll(
            DialogOverlay.FadeTo(1, 300, Easing.CubicOut),
            DialogCard.ScaleTo(1, 300, Easing.SpringOut)
        );

        // Animacja pulsowania indicator
        StartSpeechIndicatorAnimation();
    }

    public async Task HideAsync()
    {
        // Animacja wyjścia
        await Task.WhenAll(
            DialogOverlay.FadeTo(0, 250, Easing.CubicIn),
            DialogCard.ScaleTo(0.9, 250, Easing.CubicIn)
        );

        DialogOverlay.IsVisible = false;
        await Navigation.PopModalAsync();
    }

    private async void StartSpeechIndicatorAnimation()
    {
        while (DialogOverlay.IsVisible)
        {
            await SpeechIndicator.ScaleTo(1.2, 600, Easing.SinInOut);
            await SpeechIndicator.ScaleTo(1.0, 600, Easing.SinInOut);
        }
    }

    private async void OnOverlayTapped(object sender, EventArgs e)
    {
        await HideAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await HideAsync();
    }

    private async void OnStartTrainingClicked(object sender, EventArgs e)
    {
        await HideAsync();
        await Shell.Current.GoToAsync("///CognitiveGames");
    }

    private async void OnShowStatsClicked(object sender, EventArgs e)
    {
        await HideAsync();
        await Shell.Current.GoToAsync("///DailySummary");
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        // Możemy dodać stronę ustawień w przyszłości
        await DisplayAlert("Ustawienia", "Funkcja ustawień będzie dostępna w przyszłej wersji!", "OK");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Reset animacji
        DialogOverlay.Opacity = 0;
        DialogCard.Scale = 0.8;
    }

    protected override bool OnBackButtonPressed()
    {
        // Obsługa przycisku wstecz na Androidzie
        Task.Run(async () => await HideAsync());
        return true;
    }
}
