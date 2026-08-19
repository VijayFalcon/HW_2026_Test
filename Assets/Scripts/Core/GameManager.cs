using System;
using UnityEngine;
using DoofusDiaries.Pulpits;
using DoofusDiaries.Player;

namespace DoofusDiaries.Core
{
    public enum GameState
    {
        Start,
        Playing,
        GameOver
    }

    /// <summary>
    /// Top-level orchestrator and state machine (Start -> Playing -> GameOver
    /// -> Playing ...). Owns nothing about rendering/UI directly; instead it
    /// exposes OnStateChanged and a ScoreManager that UIManager subscribes to.
    /// This keeps gameplay and presentation decoupled and independently testable.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Start;
        public ScoreManager Score { get; private set; }
        public GameConfig Config { get; private set; }

        public event Action<GameState> OnStateChanged;

        private PulpitSpawner _spawner;
        private PlayerController _player;
        private const int StartSlot = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Score = new ScoreManager();
        }

        /// <summary>Wires this manager to its gameplay systems. Call once, right after everything is instantiated.</summary>
        public void Configure(GameConfig config, PulpitSpawner spawner, PlayerController player)
        {
            Config = config;
            _spawner = spawner;
            _player = player;

            _player.OnMovedToPulpit += HandlePlayerMoved;
            _player.OnFell += HandlePlayerFell;

            _player.SetInputEnabled(false);
            SetState(GameState.Start);
        }

        /// <summary>Called by the Start screen's Start button (and by RestartGame).</summary>
        public void StartGame()
        {
            Score.Reset();
            _spawner.ResetRing();
            _spawner.BeginSpawning(StartSlot);
            _player.Initialize(_spawner, Config, StartSlot);
            _player.SetInputEnabled(true);
            SetState(GameState.Playing);
        }

        /// <summary>Called by the Game Over screen's Restart button.</summary>
        public void RestartGame() => StartGame();

        private void HandlePlayerMoved(int fromSlot, int toSlot) => Score.RegisterSuccessfulMove(fromSlot, toSlot);

        private void HandlePlayerFell()
        {
            _spawner.StopSpawning();
            _player.SetInputEnabled(false);
            SetState(GameState.GameOver);
        }

        private void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
