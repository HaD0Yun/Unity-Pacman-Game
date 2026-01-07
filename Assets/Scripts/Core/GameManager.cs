using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using Pacman.Maze;
using Pacman.Player;
using Pacman.Collectibles;
using Pacman.Ghosts;
using Pacman.Utilities;

namespace Pacman.Core
{
    /// <summary>
    /// Central game manager handling score, lives, and game states.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("=== REFERENCES ===")]
        [SerializeField] private GameConstants _gameConstants;
        [SerializeField] private MazeGenerator _mazeGenerator;
        [SerializeField] private DotManager _dotManager;
        [SerializeField] private PacmanController _pacman;
        [SerializeField] private GhostManager _ghostManager;

        [Header("=== GAME STATE ===")]
        [SerializeField] private GameState _currentState = GameState.Ready;
        [SerializeField] private int _score;
        [SerializeField] private int _highScore;
        [SerializeField] private int _lives;
        [SerializeField] private int _level = 1;

        [Header("=== FRIGHTENED MODE ===")]
        [SerializeField] private bool _isFrightened;
        [SerializeField] private float _frightenedTimer;
        [SerializeField] private int _ghostEatCombo;

        // Events
        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnHighScoreChanged;
        public static event Action<int> OnLivesChanged;
        public static event Action<int> OnLevelChanged;
        public static event Action<GameState> OnGameStateChanged;
        public static event Action OnFrightenedModeStart;
        public static event Action OnFrightenedModeEnd;
        public static event Action OnFrightenedModeWarning;

        // Properties
        public GameState CurrentState => _currentState;
        public int Score => _score;
        public int HighScore => _highScore;
        public int Lives => _lives;
        public int Level => _level;
        public bool IsFrightened => _isFrightened;
        public GameConstants GameConstants => _gameConstants;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load high score
            _highScore = PlayerPrefs.GetInt("PacmanHighScore", 0);
        }

        private void OnEnable()
        {
            // Subscribe to events
            DotManager.OnScoreAdded += AddScore;
            DotManager.OnPowerPelletCollected += StartFrightenedMode;
            DotManager.OnAllDotsCollected += OnLevelComplete;
            PacmanController.OnPacmanDeath += OnPacmanDeath;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            DotManager.OnScoreAdded -= AddScore;
            DotManager.OnPowerPelletCollected -= StartFrightenedMode;
            DotManager.OnAllDotsCollected -= OnLevelComplete;
            PacmanController.OnPacmanDeath -= OnPacmanDeath;
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (_currentState == GameState.Playing)
            {
                UpdateFrightenedMode();
            }

            // Debug input using new Input System
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }

        /// <summary>
        /// Initializes the game
        /// </summary>
        public void InitializeGame()
        {
            _lives = _gameConstants.StartingLives;
            _score = 0;
            _level = 1;
            _ghostEatCombo = 0;

            // Generate maze
            _mazeGenerator.GenerateMaze();
            
            // Spawn dots
            _dotManager.SpawnDots();

            // Setup camera
            SetupCamera();

            // Set state to ready
            SetGameState(GameState.Ready);

            // Notify UI
            OnScoreChanged?.Invoke(_score);
            OnLivesChanged?.Invoke(_lives);
            OnLevelChanged?.Invoke(_level);
            OnHighScoreChanged?.Invoke(_highScore);

            // Start game after delay
            StartCoroutine(StartGameSequence());
        }

        /// <summary>
        /// Sets up the camera to fit the maze
        /// </summary>
        private void SetupCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            // Calculate orthographic size to fit maze
            float mazeWidth = _gameConstants.MazeWidth * _gameConstants.TileSize;
            float mazeHeight = _gameConstants.MazeHeight * _gameConstants.TileSize;

            float screenRatio = (float)Screen.width / Screen.height;
            float targetRatio = mazeWidth / mazeHeight;

            if (screenRatio >= targetRatio)
            {
                mainCam.orthographicSize = mazeHeight / 2f + 2f;
            }
            else
            {
                mainCam.orthographicSize = (mazeWidth / screenRatio) / 2f + 2f;
            }

            // Center camera on maze
            mainCam.transform.position = new Vector3(
                mazeWidth / 2f - 0.5f,
                mazeHeight / 2f - 0.5f,
                -10f
            );

            // Set background color
            mainCam.backgroundColor = _gameConstants.BackgroundColor;
        }

        /// <summary>
        /// Start game sequence with "READY!" display
        /// </summary>
        private IEnumerator StartGameSequence()
        {
            yield return new WaitForSeconds(2f);
            SetGameState(GameState.Playing);
            
            if (_pacman != null)
                _pacman.CanMove = true;
        }

        /// <summary>
        /// Sets the game state
        /// </summary>
        public void SetGameState(GameState newState)
        {
            _currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Ready:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.PacmanDeath:
                    break;
                case GameState.GameOver:
                    SaveHighScore();
                    break;
            }
        }

        /// <summary>
        /// Adds score
        /// </summary>
        public void AddScore(int points)
        {
            _score += points;
            OnScoreChanged?.Invoke(_score);

            // Check for extra life
            if (_score >= _gameConstants.ExtraLifeScore && 
                _score - points < _gameConstants.ExtraLifeScore)
            {
                AddLife();
            }

            // Update high score
            if (_score > _highScore)
            {
                _highScore = _score;
                OnHighScoreChanged?.Invoke(_highScore);
            }
        }

        /// <summary>
        /// Adds a life
        /// </summary>
        public void AddLife()
        {
            if (_lives < _gameConstants.MaxLives)
            {
                _lives++;
                OnLivesChanged?.Invoke(_lives);
            }
        }

        /// <summary>
        /// Called when Pacman dies
        /// </summary>
        private void OnPacmanDeath()
        {
            SetGameState(GameState.PacmanDeath);
            _lives--;
            OnLivesChanged?.Invoke(_lives);

            if (_lives <= 0)
            {
                StartCoroutine(GameOverSequence());
            }
            else
            {
                StartCoroutine(RespawnSequence());
            }
        }

        /// <summary>
        /// Respawn sequence after death
        /// </summary>
        private IEnumerator RespawnSequence()
        {
            yield return new WaitForSeconds(2f);
            
            // Reset positions
            _pacman.Respawn();
            _ghostManager?.ResetGhosts();
            
            // Reset frightened mode
            EndFrightenedMode();

            SetGameState(GameState.Ready);
            yield return new WaitForSeconds(2f);
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// Game over sequence
        /// </summary>
        private IEnumerator GameOverSequence()
        {
            yield return new WaitForSeconds(2f);
            SetGameState(GameState.GameOver);
        }

        /// <summary>
        /// Called when all dots are collected
        /// </summary>
        private void OnLevelComplete()
        {
            SetGameState(GameState.LevelComplete);
            StartCoroutine(NextLevelSequence());
        }

        /// <summary>
        /// Next level sequence
        /// </summary>
        private IEnumerator NextLevelSequence()
        {
            _pacman.CanMove = false;
            
            // Flash maze (simplified)
            yield return new WaitForSeconds(2f);

            _level++;
            OnLevelChanged?.Invoke(_level);

            // Reset maze
            _dotManager.ResetDots();
            _pacman.InitializePosition();
            _ghostManager?.ResetGhosts();
            
            EndFrightenedMode();

            SetGameState(GameState.Ready);
            yield return new WaitForSeconds(2f);
            
            _pacman.CanMove = true;
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// Starts frightened mode
        /// </summary>
        public void StartFrightenedMode()
        {
            _isFrightened = true;
            _frightenedTimer = _gameConstants.FrightenedDuration;
            _ghostEatCombo = 0;

            OnFrightenedModeStart?.Invoke();
        }

        /// <summary>
        /// Updates frightened mode timer
        /// </summary>
        private void UpdateFrightenedMode()
        {
            if (!_isFrightened) return;

            _frightenedTimer -= Time.deltaTime;

            // Warning phase
            if (_frightenedTimer <= _gameConstants.FrightenedBlinkDuration && 
                _frightenedTimer > 0)
            {
                OnFrightenedModeWarning?.Invoke();
            }

            if (_frightenedTimer <= 0)
            {
                EndFrightenedMode();
            }
        }

        /// <summary>
        /// Ends frightened mode
        /// </summary>
        public void EndFrightenedMode()
        {
            _isFrightened = false;
            _frightenedTimer = 0;
            _ghostEatCombo = 0;

            OnFrightenedModeEnd?.Invoke();
        }

        /// <summary>
        /// Called when a ghost is eaten
        /// </summary>
        public void OnGhostEaten()
        {
            int points = _gameConstants.GetGhostPoints(_ghostEatCombo);
            AddScore(points);
            _ghostEatCombo++;

            // Brief pause
            StartCoroutine(GhostEatenPause());
        }

        /// <summary>
        /// Brief pause when ghost is eaten
        /// </summary>
        private IEnumerator GhostEatenPause()
        {
            SetGameState(GameState.GhostEaten);
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.5f);
            Time.timeScale = 1f;
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// Saves high score
        /// </summary>
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("PacmanHighScore", _highScore);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Restarts the game
        /// </summary>
        public void RestartGame()
        {
            ProceduralSpriteGenerator.CleanupTextures();
            InitializeGame();
        }
    }
}
