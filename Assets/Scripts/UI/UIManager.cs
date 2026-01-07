using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pacman.Core;

namespace Pacman.UI
{
    /// <summary>
    /// Manages all UI elements in the game.
    /// Handles score display, lives, and game state messages.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [SerializeField] private GameConstants _gameConstants;

        [Header("=== UI ELEMENTS ===")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Transform _livesContainer;

        [Header("=== LIVES DISPLAY ===")]
        [SerializeField] private GameObject _lifePrefab;
        private GameObject[] _lifeIcons;

        private void OnEnable()
        {
            GameManager.OnScoreChanged += UpdateScore;
            GameManager.OnHighScoreChanged += UpdateHighScore;
            GameManager.OnLivesChanged += UpdateLives;
            GameManager.OnLevelChanged += UpdateLevel;
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnScoreChanged -= UpdateScore;
            GameManager.OnHighScoreChanged -= UpdateHighScore;
            GameManager.OnLivesChanged -= UpdateLives;
            GameManager.OnLevelChanged -= UpdateLevel;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void Start()
        {
            SetupUI();
        }

        /// <summary>
        /// Sets up the UI canvas and elements
        /// </summary>
        private void SetupUI()
        {
            if (_canvas == null)
            {
                // Create canvas
                GameObject canvasObj = new GameObject("UICanvas");
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasObj.transform.SetParent(transform);
            }

            // Create UI elements if not assigned
            if (_scoreText == null)
            {
                _scoreText = CreateText("ScoreText", new Vector2(10, -10), TextAnchor.UpperLeft);
                _scoreText.alignment = TextAlignmentOptions.TopLeft;
            }

            if (_highScoreText == null)
            {
                _highScoreText = CreateText("HighScoreText", new Vector2(0, -10), TextAnchor.UpperCenter);
                _highScoreText.alignment = TextAlignmentOptions.Top;
            }

            if (_livesText == null)
            {
                _livesText = CreateText("LivesText", new Vector2(10, 10), TextAnchor.LowerLeft);
                _livesText.alignment = TextAlignmentOptions.BottomLeft;
            }

            if (_levelText == null)
            {
                _levelText = CreateText("LevelText", new Vector2(-10, 10), TextAnchor.LowerRight);
                _levelText.alignment = TextAlignmentOptions.BottomRight;
            }

            if (_messageText == null)
            {
                _messageText = CreateText("MessageText", Vector2.zero, TextAnchor.MiddleCenter);
                _messageText.alignment = TextAlignmentOptions.Center;
                _messageText.fontSize = 48;
                _messageText.color = Color.yellow;
            }

            // Initial values
            UpdateScore(0);
            UpdateHighScore(0);
            UpdateLives(_gameConstants != null ? _gameConstants.StartingLives : 3);
            UpdateLevel(1);
            HideMessage();
        }

        /// <summary>
        /// Creates a text element
        /// </summary>
        private TextMeshProUGUI CreateText(string name, Vector2 position, TextAnchor anchor)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(_canvas.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            
            // Set anchor based on TextAnchor
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 1);
                    break;
                case TextAnchor.UpperCenter:
                    rect.anchorMin = new Vector2(0.5f, 1);
                    rect.anchorMax = new Vector2(0.5f, 1);
                    rect.pivot = new Vector2(0.5f, 1);
                    break;
                case TextAnchor.UpperRight:
                    rect.anchorMin = new Vector2(1, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 1);
                    break;
                case TextAnchor.MiddleCenter:
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.LowerLeft:
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(0, 0);
                    rect.pivot = new Vector2(0, 0);
                    break;
                case TextAnchor.LowerRight:
                    rect.anchorMin = new Vector2(1, 0);
                    rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(1, 0);
                    break;
            }

            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 24;
            text.color = Color.white;
            text.font = TMP_Settings.defaultFontAsset;

            return text;
        }

        /// <summary>
        /// Updates the score display
        /// </summary>
        public void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"SCORE\n{score:D6}";
            }
        }

        /// <summary>
        /// Updates the high score display
        /// </summary>
        public void UpdateHighScore(int highScore)
        {
            if (_highScoreText != null)
            {
                _highScoreText.text = $"HIGH SCORE\n{highScore:D6}";
            }
        }

        /// <summary>
        /// Updates the lives display
        /// </summary>
        public void UpdateLives(int lives)
        {
            if (_livesText != null)
            {
                _livesText.text = $"LIVES: {lives}";
            }

            // Update life icons if using visual display
            if (_lifeIcons != null)
            {
                for (int i = 0; i < _lifeIcons.Length; i++)
                {
                    if (_lifeIcons[i] != null)
                    {
                        _lifeIcons[i].SetActive(i < lives);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the level display
        /// </summary>
        public void UpdateLevel(int level)
        {
            if (_levelText != null)
            {
                _levelText.text = $"LEVEL {level}";
            }
        }

        /// <summary>
        /// Shows a message on screen
        /// </summary>
        public void ShowMessage(string message, Color color)
        {
            if (_messageText != null)
            {
                _messageText.text = message;
                _messageText.color = color;
                _messageText.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the message
        /// </summary>
        public void HideMessage()
        {
            if (_messageText != null)
            {
                _messageText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Handles game state changes
        /// </summary>
        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Ready:
                    ShowMessage("READY!", Color.yellow);
                    break;
                case GameState.Playing:
                    HideMessage();
                    break;
                case GameState.PacmanDeath:
                    // Don't show message during death animation
                    break;
                case GameState.LevelComplete:
                    ShowMessage("LEVEL COMPLETE!", Color.green);
                    break;
                case GameState.GameOver:
                    ShowMessage("GAME OVER", Color.red);
                    break;
            }
        }

        /// <summary>
        /// Shows score popup at position (for ghost eating)
        /// </summary>
        public void ShowScorePopup(int score, Vector3 worldPosition)
        {
            // Create temporary text at position
            GameObject popup = new GameObject("ScorePopup");
            popup.transform.SetParent(_canvas.transform, false);

            TextMeshProUGUI text = popup.AddComponent<TextMeshProUGUI>();
            text.text = score.ToString();
            text.fontSize = 16;
            text.color = Color.cyan;
            text.alignment = TextAlignmentOptions.Center;

            // Convert world position to screen position
            RectTransform rect = popup.GetComponent<RectTransform>();
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            rect.position = screenPos;

            // Destroy after delay
            Destroy(popup, 1f);
        }
    }
}
