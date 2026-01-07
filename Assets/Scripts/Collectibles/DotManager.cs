using UnityEngine;
using System;
using System.Collections.Generic;
using Pacman.Core;
using Pacman.Maze;
using Pacman.Utilities;

namespace Pacman.Collectibles
{
    /// <summary>
    /// Manages all dots in the maze.
    /// Handles spawning, collection tracking, and level completion.
    /// </summary>
    public class DotManager : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [SerializeField] private GameConstants _gameConstants;
        [SerializeField] private MazeGenerator _mazeGenerator;

        [Header("=== SETTINGS ===")]
        [SerializeField] private int _dotPixelSize = 4;
        [SerializeField] private int _powerPelletPixelSize = 12;
        [SerializeField] private Transform _dotsContainer;

        // Events
        public static event Action<int> OnScoreAdded;
        public static event Action OnPowerPelletCollected;
        public static event Action OnAllDotsCollected;
        public static event Action<int> OnDotCountChanged;

        // Tracking
        private List<Dot> _allDots = new List<Dot>();
        private int _totalDots;
        private int _collectedDots;
        private int _remainingDots;

        // Sprites
        private Sprite _dotSprite;
        private Sprite _powerPelletSprite;

        // Properties
        public int TotalDots => _totalDots;
        public int CollectedDots => _collectedDots;
        public int RemainingDots => _remainingDots;

        private void Awake()
        {
            // Create container for dots
            if (_dotsContainer == null)
            {
                GameObject container = new GameObject("DotsContainer");
                container.transform.SetParent(transform);
                _dotsContainer = container.transform;
            }
        }

        private void OnEnable()
        {
            Dot.OnDotCollected += HandleDotCollected;
            Dot.OnPowerPelletCollected += HandlePowerPelletCollected;
        }

        private void OnDisable()
        {
            Dot.OnDotCollected -= HandleDotCollected;
            Dot.OnPowerPelletCollected -= HandlePowerPelletCollected;
        }

        /// <summary>
        /// Initialize and spawn all dots
        /// </summary>
        public void SpawnDots()
        {
            ClearDots();
            CreateSprites();

            var mazeData = _mazeGenerator.MazeData;
            
            for (int y = 0; y < mazeData.Height; y++)
            {
                for (int x = 0; x < mazeData.Width; x++)
                {
                    if (mazeData.HasDot(x, y))
                    {
                        SpawnDot(x, y, false);
                    }
                    else if (mazeData.HasPowerPellet(x, y))
                    {
                        SpawnDot(x, y, true);
                    }
                }
            }

            _totalDots = _allDots.Count;
            _remainingDots = _totalDots;
            _collectedDots = 0;

            Debug.Log($"DotManager: Spawned {_totalDots} dots ({mazeData.GetSmallDotCount()} small, {mazeData.GetPowerPelletCount()} power pellets)");
        }

        /// <summary>
        /// Creates the dot sprites
        /// </summary>
        private void CreateSprites()
        {
            _dotSprite = ProceduralSpriteGenerator.CreateCircle(_dotPixelSize, _gameConstants.DotColor);
            _powerPelletSprite = ProceduralSpriteGenerator.CreateCircle(_powerPelletPixelSize, _gameConstants.DotColor);
        }

        /// <summary>
        /// Spawns a single dot at the given position
        /// </summary>
        private void SpawnDot(int x, int y, bool isPowerPellet)
        {
            GameObject dotObj = new GameObject(isPowerPellet ? $"PowerPellet_{x}_{y}" : $"Dot_{x}_{y}");
            dotObj.transform.SetParent(_dotsContainer);
            dotObj.transform.position = _mazeGenerator.GetWorldPosition(x, y);

            Dot dot;
            if (isPowerPellet)
            {
                dot = dotObj.AddComponent<PowerPellet>();
                dot.Initialize(_powerPelletSprite, _gameConstants.PowerPelletPoints, true);
            }
            else
            {
                dot = dotObj.AddComponent<Dot>();
                dot.Initialize(_dotSprite, _gameConstants.SmallDotPoints, false);
            }

            dot.TilePosition = new Vector2Int(x, y);
            _allDots.Add(dot);
        }

        /// <summary>
        /// Handles regular dot collection
        /// </summary>
        private void HandleDotCollected(Dot dot)
        {
            _collectedDots++;
            _remainingDots--;
            
            OnScoreAdded?.Invoke(dot.Points);
            OnDotCountChanged?.Invoke(_collectedDots);
            
            CheckAllDotsCollected();
        }

        /// <summary>
        /// Handles power pellet collection
        /// </summary>
        private void HandlePowerPelletCollected(Dot dot)
        {
            _collectedDots++;
            _remainingDots--;
            
            OnScoreAdded?.Invoke(dot.Points);
            OnDotCountChanged?.Invoke(_collectedDots);
            OnPowerPelletCollected?.Invoke();
            
            CheckAllDotsCollected();
        }

        /// <summary>
        /// Checks if all dots have been collected
        /// </summary>
        private void CheckAllDotsCollected()
        {
            if (_remainingDots <= 0)
            {
                OnAllDotsCollected?.Invoke();
            }
        }

        /// <summary>
        /// Clears all dots
        /// </summary>
        public void ClearDots()
        {
            foreach (var dot in _allDots)
            {
                if (dot != null)
                {
                    Destroy(dot.gameObject);
                }
            }
            _allDots.Clear();
            _collectedDots = 0;
            _remainingDots = 0;
        }

        /// <summary>
        /// Resets all dots for a new level
        /// </summary>
        public void ResetDots()
        {
            foreach (var dot in _allDots)
            {
                if (dot != null)
                {
                    dot.ResetDot();
                }
            }
            _collectedDots = 0;
            _remainingDots = _totalDots;
        }

        /// <summary>
        /// Gets dot at a specific position
        /// </summary>
        public Dot GetDotAt(Vector2Int position)
        {
            foreach (var dot in _allDots)
            {
                if (dot != null && dot.TilePosition == position && dot.gameObject.activeSelf)
                {
                    return dot;
                }
            }
            return null;
        }
    }
}
