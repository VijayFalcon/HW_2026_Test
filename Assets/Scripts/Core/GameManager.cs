// Top-level state machine (Start -> Playing -> GameOver/Won -> Playing ...)
// that wires the tile spawner, player, and score manager together, drives
// Start/Restart, and decides the win condition (score reaches the target
// for the selected Difficulty). Owns no rendering/UI directly -- UIManager
// reacts to the events this exposes instead.

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
        GameOver,
        Won
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Start;
        public ScoreManager Score { get; private set; }
        public GameConfig Config { get; private set; }

        public Difficulty SelectedDifficulty { get; set; } = Difficulty.Easy;
        public int TargetScore => DifficultyTargets.GetTarget(SelectedDifficulty);

        public event Action<GameState> OnStateChanged;

        private PulpitSpawner _spawner;
        private PlayerController _player;

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

        public void Configure(GameConfig config, PulpitSpawner spawner, PlayerController player)
        {
            Config = config;
            _spawner = spawner;
            _player = player;

            _player.OnLandedOnNewTile += HandleLandedOnNewTile;
            _player.OnFell += HandlePlayerFell;
            Score.OnScoreChanged += HandleScoreChanged;

            _player.SetInputEnabled(false);
            SetState(GameState.Start);
        }

        public void StartGame()
        {
            Score.Reset();
            _spawner.ResetGrid();

            Pulpit startTile = _spawner.BeginSpawning();
            Vector3 startPosition = _spawner.GridToWorld(startTile.GridPosition) + Vector3.up * 1.5f;

            _player.Initialize(Config, startPosition);
            _player.SetInputEnabled(true);
            SetState(GameState.Playing);
        }

        public void RestartGame() => StartGame();

        private void HandleLandedOnNewTile(Vector2Int tileGridPosition) => Score.RegisterTileEntered(tileGridPosition);

        private void HandleScoreChanged(int score)
        {
            if (State != GameState.Playing) return;
            if (score < TargetScore) return;

            _spawner.StopSpawning();
            _player.SetInputEnabled(false);
            SetState(GameState.Won);
        }

        private void HandlePlayerFell()
        {
            if (State != GameState.Playing) return;

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
