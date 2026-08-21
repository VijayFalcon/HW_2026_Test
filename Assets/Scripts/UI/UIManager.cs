// Builds and drives the Start, HUD, and end-of-run screens entirely at
// runtime via UIFactory, reacting to GameManager's state/score events. The
// end-of-run panel is shared between GameOver and Won for now (just its
// title swaps) -- a distinct, polished win screen and the difficulty
// picker on the Start screen are Level 3 work, not yet built here.

using UnityEngine;
using UnityEngine.UI;
using DoofusDiaries.Core;

namespace DoofusDiaries.UI
{
    public class UIManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private Canvas _canvas;

        private GameObject _startScreen;
        private GameObject _hud;
        private GameObject _endScreen;

        private Text _scoreText;
        private Text _endTitleText;
        private Text _finalScoreText;
        private Text _bestScoreText;

        public void Bind(GameManager gameManager)
        {
            _gameManager = gameManager;

            BuildCanvas();
            BuildStartScreen();
            BuildHud();
            BuildEndScreen();

            _gameManager.OnStateChanged += HandleStateChanged;
            _gameManager.Score.OnScoreChanged += HandleScoreChanged;

            HandleStateChanged(_gameManager.State);
        }

        private void OnDestroy()
        {
            if (_gameManager == null) return;
            _gameManager.OnStateChanged -= HandleStateChanged;
            if (_gameManager.Score != null) _gameManager.Score.OnScoreChanged -= HandleScoreChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            bool ended = state == GameState.GameOver || state == GameState.Won;

            _startScreen.SetActive(state == GameState.Start);
            _hud.SetActive(state == GameState.Playing);
            _endScreen.SetActive(ended);

            if (ended)
            {
                _endTitleText.text = state == GameState.Won ? "YOU WIN!" : "GAME OVER";
                _finalScoreText.text = $"Score: {_gameManager.Score.CurrentScore} / {_gameManager.TargetScore}";
                _bestScoreText.text = $"Best: {_gameManager.Score.BestScore}";
            }
            else if (state == GameState.Playing)
            {
                _scoreText.text = $"Score: {_gameManager.Score.CurrentScore} / {_gameManager.TargetScore}";
            }
        }

        private void HandleScoreChanged(int score)
        {
            _scoreText.text = $"Score: {score} / {_gameManager.TargetScore}";
        }

        private void BuildCanvas()
        {
            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(transform, false);

            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        private void BuildStartScreen()
        {
            _startScreen = UIFactory.Panel(_canvas.transform, "StartScreen", new Color(0f, 0f, 0f, 0.75f));
            UIFactory.Text(_startScreen.transform, "Title", "DOOFUS DIARIES", 96, new Vector2(0, 200));
            UIFactory.Text(
                _startScreen.transform,
                "Subtitle",
                "Walk across the pulpits before they collapse!\nWASD or Arrow keys to move freely.",
                36,
                new Vector2(0, 40));
            UIFactory.Button(_startScreen.transform, "StartButton", "START", new Vector2(0, -220), () => _gameManager.StartGame());
        }

        private void BuildHud()
        {
            _hud = UIFactory.Panel(_canvas.transform, "HUD", new Color(0f, 0f, 0f, 0f));
            _scoreText = UIFactory.Text(_hud.transform, "ScoreText", "Score: 0", 56, new Vector2(0, 850));
        }

        private void BuildEndScreen()
        {
            _endScreen = UIFactory.Panel(_canvas.transform, "EndScreen", new Color(0f, 0f, 0f, 0.85f));
            _endTitleText = UIFactory.Text(_endScreen.transform, "EndTitle", "GAME OVER", 96, new Vector2(0, 260));
            _finalScoreText = UIFactory.Text(_endScreen.transform, "FinalScore", "Score: 0", 48, new Vector2(0, 100));
            _bestScoreText = UIFactory.Text(_endScreen.transform, "BestScore", "Best: 0", 40, new Vector2(0, 30));
            UIFactory.Button(_endScreen.transform, "RestartButton", "RESTART", new Vector2(0, -200), () => _gameManager.RestartGame());
        }
    }
}
