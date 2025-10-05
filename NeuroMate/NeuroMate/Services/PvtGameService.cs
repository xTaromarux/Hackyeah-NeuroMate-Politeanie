using NeuroMate.Models;
using NeuroMate.Database;

namespace NeuroMate.Services
{
    public class PvtGameService : IPVTGameService
    {
        private readonly DatabaseService _db;
        private readonly List<ReactionRecord> _reactions = new();
        private readonly Random _random = new();
        private Timer? _gameTimer;
        private DateTime _stimulusTime;
        private bool _isWaitingForReaction;

        public event EventHandler<ReactionEventArgs>? OnReactionRecorded;
        public event EventHandler<GameStateEventArgs>? OnGameStateChanged;

        public bool IsGameActive { get; private set; }

        public PvtGameService(DatabaseService db)
        {
            _db = db;
        }

        public Task StartGameAsync()
        {
            try
            {
                IsGameActive = true;
                _reactions.Clear();
                
                OnGameStateChanged?.Invoke(this, new GameStateEventArgs 
                { 
                    State = GameState.Waiting, 
                    Message = "Przygotuj się..." 
                });

                ScheduleNextStimulus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartGameAsync error: {ex.Message}");
                IsGameActive = false;
            }
            
            return Task.CompletedTask;
        }

        public Task StopGameAsync()
        {
            try
            {
                IsGameActive = false;
                _gameTimer?.Dispose();
                _gameTimer = null;
                _isWaitingForReaction = false;
                
                OnGameStateChanged?.Invoke(this, new GameStateEventArgs 
                { 
                    State = GameState.Idle, 
                    Message = "Gra zatrzymana" 
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StopGameAsync error: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        public void RecordReaction(int reactionTimeMs)
        {
            try
            {
                if (!_isWaitingForReaction || !IsGameActive)
                    return;

                var reaction = new ReactionRecord
                {
                    ReactionTimeMs = reactionTimeMs,
                    Timestamp = DateTime.Now,
                    IsValid = reactionTimeMs >= 100 && reactionTimeMs <= 2000
                };

                _reactions.Add(reaction);
                _isWaitingForReaction = false;

                OnReactionRecorded?.Invoke(this, new ReactionEventArgs
                {
                    ReactionTimeMs = reactionTimeMs,
                    IsValid = reaction.IsValid,
                    Timestamp = reaction.Timestamp
                });

                OnGameStateChanged?.Invoke(this, new GameStateEventArgs 
                { 
                    State = GameState.Completed, 
                    Message = "Reakcja zarejestrowana" 
                });

                if (IsGameActive && _reactions.Count < 10)
                {
                    Task.Delay(1000).ContinueWith(_ => ScheduleNextStimulus(), TaskContinuationOptions.OnlyOnRanToCompletion);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RecordReaction error: {ex.Message}");
            }
        }

        public PVTStatistics GetStatistics()
        {
            var validReactions = _reactions.Where(r => r.IsValid).ToList();
            
            return new PVTStatistics
            {
                BestReactionMs = validReactions.Any() ? validReactions.Min(r => r.ReactionTimeMs) : 0,
                AverageReactionMs = validReactions.Any() ? (int)validReactions.Average(r => r.ReactionTimeMs) : 0,
                TrialsCount = validReactions.Count,
                AllReactions = validReactions.Select(r => r.ReactionTimeMs).ToList(),
                SessionStart = _reactions.FirstOrDefault()?.Timestamp ?? DateTime.Now,
                SessionEnd = _reactions.LastOrDefault()?.Timestamp
            };
        }

        public void ResetStatistics()
        {
            _reactions.Clear();
        }

        private void ScheduleNextStimulus()
        {
            try
            {
                if (!IsGameActive) return;

                var waitTime = _random.Next(2000, 10000);
                
                _gameTimer?.Dispose(); // Dispose previous timer
                _gameTimer = new Timer(ShowStimulus, null, waitTime, Timeout.Infinite);
                
                OnGameStateChanged?.Invoke(this, new GameStateEventArgs 
                { 
                    State = GameState.Waiting, 
                    Message = "Czekaj..." 
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ScheduleNextStimulus error: {ex.Message}");
                IsGameActive = false;
            }
        }

        private void ShowStimulus(object? state)
        {
            try
            {
                if (!IsGameActive) return;

                _stimulusTime = DateTime.Now;
                _isWaitingForReaction = true;
                
                OnGameStateChanged?.Invoke(this, new GameStateEventArgs 
                { 
                    State = GameState.Ready, 
                    Message = "KLIKNIJ TERAZ!" 
                });

                Task.Delay(2000).ContinueWith(_ => 
                {
                    try
                    {
                        if (_isWaitingForReaction && IsGameActive)
                        {
                            _isWaitingForReaction = false;
                            OnGameStateChanged?.Invoke(this, new GameStateEventArgs 
                            { 
                                State = GameState.Completed, 
                                Message = "Czas minął" 
                            });
                            
                            if (_reactions.Count < 10)
                            {
                                Task.Delay(1000).ContinueWith(__ => ScheduleNextStimulus(), TaskContinuationOptions.OnlyOnRanToCompletion);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ShowStimulus timeout error: {ex.Message}");
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowStimulus error: {ex.Message}");
            }
        }
    }
}
