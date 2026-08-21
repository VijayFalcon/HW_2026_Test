// Builds and drives the Start (with a difficulty picker), HUD, and
// end-of-run screens entirely at runtime via UIFactory, reacting to
// GameManager's state/score events. The end-of-run panel is shared between
// GameOver and Won for now (just its title swaps) -- a distinct, polished
// win screen is still open work.

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

        private Image _easyButtonImage;
        private Image _mediumButtonImage;
        private Image _hardButtonImage;

        private static readonly Color UnselectedDifficultyColor = new Color(0.2f, 0.6f, 0.9f);
        private static readonly Color SelectedDifficultyColor = new Color(0.95f, 0.75f, 0.15f);

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
            // 0.5 splits the difference between matching width and height,
            // so layout doesn't blow out badly in either a landscape editor
            // Game view or a portrait build -- this reference resolution
            // was chosen portrait-first, but nothing here should assume a
            // specific aspect ratio actually being tested.
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        private void BuildStartScreen()
        {
            _startScreen = UIFactory.Panel(_canvas.transform, "StartScreen", new Color(0f, 0f, 0f, 0.75f));
            UIFactory.Text(_startScreen.transform, "Title", "DOOFUS DIARIES", 96, new Vector2(0, 280));
            UIFactory.Text(
                _startScreen.transform,
                "Subtitle",
                "Walk across the pulpits before they collapse!\nWASD or Arrow keys to move freely.",
                36,
                new Vector2(0, 140));

            UIFactory.Text(_startScreen.transform, "DifficultyLabel", "DIFFICULTY", 32, new Vector2(0, 20));

            Vector2 difficultyButtonSize = new Vector2(210, 90);
            Button easyButton = UIFactory.Button(_startScreen.transform, "EasyButton", "EASY", new Vector2(-240, -70), () => SelectDifficulty(Difficulty.Easy), difficultyButtonSize, 30);
            Button mediumButton = UIFactory.Button(_startScreen.transform, "MediumButton", "MEDIUM", new Vector2(0, -70), () => SelectDifficulty(Difficulty.Medium), difficultyButtonSize, 30);
            Button hardButton = UIFactory.Button(_startScreen.transform, "HardButton", "HARD", new Vector2(240, -70), () => SelectDifficulty(Difficulty.Hard), difficultyButtonSize, 30);

            _easyButtonImage = easyButton.GetComponent<Image>();
            _mediumButtonImage = mediumButton.GetComponent<Image>();
            _hardButtonImage = hardButton.GetComponent<Image>();

            UIFactory.Button(_startScreen.transform, "StartButton", "START", new Vector2(0, -230), () => _gameManager.StartGame());

            SelectDifficulty(_gameManager.SelectedDifficulty);
        }

        private void SelectDifficulty(Difficulty difficulty)
        {
            _gameManager.SelectedDifficulty = difficulty;

            if (_easyButtonImage != null) _easyButtonImage.color = difficulty == Difficulty.Easy ? SelectedDifficultyColor : UnselectedDifficultyColor;
            if (_mediumButtonImage != null) _mediumButtonImage.color = difficulty == Difficulty.Medium ? SelectedDifficultyColor : UnselectedDifficultyColor;
            if (_hardButtonImage != null) _hardButtonImage.color = difficulty == Difficulty.Hard ? SelectedDifficultyColor : UnselectedDifficultyColor;
        }

        private void BuildHud()
        {
            _hud = UIFactory.Panel(_canvas.transform, "HUD", new Color(0f, 0f, 0f, 0f));
            // Anchored to the top-center of the screen itself (not offset
            // from canvas center), so it stays visibly near the top edge
            // regardless of the actual aspect ratio being played in --
            // a fixed offset from center only lines up right for the one
            // aspect ratio it was tuned against.
            _scoreText = UIFactory.Text(_hud.transform, "ScoreText", "Score: 0", 56, new Vector2(0, -80), new Vector2(0.5f, 1f));
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
