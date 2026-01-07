using UnityEngine;
using Pacman.Core;

namespace Pacman.Ghosts
{
    /// <summary>
    /// Clyde (Orange Ghost) - "Pokey" - Shy/Erratic
    /// Targets Pacman when far away (>8 tiles), retreats to scatter corner when close.
    /// Creates unpredictable behavior.
    /// </summary>
    public class Clyde : GhostBase
    {
        [Header("=== CLYDE SETTINGS ===")]
        [SerializeField] private float _shyDistance = 8f;

        protected override void Awake()
        {
            base.Awake();
            
            _ghostName = "Clyde";
            _ghostColor = new Color(1f, 0.72f, 0.32f, 1f); // Orange
        }

        protected override void Start()
        {
            // Set Clyde-specific values from GameConstants
            if (_gameConstants != null)
            {
                _ghostColor = _gameConstants.ClydeColor;
                _scatterCorner = _gameConstants.ClydeScatterCorner;
                _spawnPosition = _gameConstants.ClydeSpawnPosition;
            }

            base.Start();
        }

        /// <summary>
        /// Clyde targets Pacman when far (>8 tiles), retreats when close
        /// </summary>
        protected override Vector2Int GetChaseTarget()
        {
            if (_pacman == null)
                return _scatterCorner;

            Vector2Int pacmanTile = _pacman.CurrentTile;
            
            // Calculate distance to Pacman
            float distance = Vector2Int.Distance(_currentTile, pacmanTile);

            // If far from Pacman, chase directly
            if (distance > _shyDistance)
            {
                return pacmanTile;
            }
            
            // If close to Pacman, retreat to scatter corner
            return _scatterCorner;
        }
    }
}
