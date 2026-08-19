using UnityEngine;
using UnityEngine.UI;
using DoofusDiaries.Core;

namespace DoofusDiaries.UI
{
    /// <summary>
    /// Builds and drives the three screens the game needs: Start, in-game
    /// HUD (score), and Game Over. Everything is constructed at runtime from
    /// UIFactory helpers, so there is no scene/prefab wiring to get wrong.
    /// Purely reactive to GameManager: it never decides game logic itself.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private Canvas _canvas;

        private GameObject _startScreen;
        private GameObject _hud;
        private GameObject _gameOverScreen;

        private Text _scoreText;
        private Text _finalScoreText;
        private Text _bestScoreText;

        public void Bind(GameManager gameManager)
        {
            _gameManager = gameManager;

            BuildCanvas();
            BuildStartScreen();
            BuildHud();
            BuildGameOverScreen();

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
            _startScreen.SetActive(state == GameState.Start);
            _hud.SetActive(state == GameState.Playing);
            _gameOverScreen.SetActive(state == GameState.GameOver);

            if (state == GameState.GameOver)
            {
                _finalScoreText.text = $"Score: {_gameManager.Score.CurrentScore}";
                _bestScoreText.text = $"Best: {_gameManager.Score.BestScore}";
            }
            else if (state == GameState.Playing)
            {
                _scoreText.text = $"Score: {_gameManager.Score.CurrentScore}";
            }
        }

        private void HandleScoreChanged(int score)
        {
            _scoreText.text = $"Score: {score}";
        }

        // ---- UI construction --------------------------------------------

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
                "Hop between pulpits before they collapse!\nLeft/Right or A/D to move.",
                36,
                new Vector2(0, 40));
            UIFactory.Button(_startScreen.transform, "StartButton", "START", new Vector2(0, -220), () => _gameManager.StartGame());
        }

        private void BuildHud()
        {
            _hud = UIFactory.Panel(_canvas.transform, "HUD", new Color(0f, 0f, 0f, 0f));
            _scoreText = UIFactory.Text(_hud.transform, "ScoreText", "Score: 0", 56, new Vector2(0, 850));
        }

        private void BuildGameOverScreen()
        {
            _gameOverScreen = UIFactory.Panel(_canvas.transform, "GameOverScreen", new Color(0f, 0f, 0f, 0.85f));
            UIFactory.Text(_gameOverScreen.transform, "GameOverTitle", "GAME OVER", 96, new Vector2(0, 260));
            _finalScoreText = UIFactory.Text(_gameOverScreen.transform, "FinalScore", "Score: 0", 48, new Vector2(0, 100));
            _bestScoreText = UIFactory.Text(_gameOverScreen.transform, "BestScore", "Best: 0", 40, new Vector2(0, 30));
            UIFactory.Button(_gameOverScreen.transform, "RestartButton", "RESTART", new Vector2(0, -200), () => _gameManager.RestartGame());
        }
    }
}
