using UnityEngine;
using Pacman.Core;

namespace Pacman.Ghosts
{
    /// <summary>
    /// Pinky (Pink Ghost) - "Speedy" - Ambusher
    /// Targets 4 tiles ahead of Pacman's current direction.
    /// Tries to ambush Pacman by getting in front.
    /// </summary>
    public class Pinky : GhostBase
    {
        [Header("=== PINKY SETTINGS ===")]
        [SerializeField] private int _tilesAhead = 4;

        protected override void Awake()
        {
            base.Awake();
            
            _ghostName = "Pinky";
            _ghostColor = new Color(1f, 0.72f, 1f, 1f); // Pink
        }

        protected override void Start()
        {
            // Set Pinky-specific values from GameConstants
            if (_gameConstants != null)
            {
                _ghostColor = _gameConstants.PinkyColor;
                _scatterCorner = _gameConstants.PinkyScatterCorner;
                _spawnPosition = _gameConstants.PinkySpawnPosition;
            }

            base.Start();
        }

        /// <summary>
        /// Pinky targets 4 tiles ahead of Pacman
        /// </summary>
        protected override Vector2Int GetChaseTarget()
        {
            if (_pacman == null)
                return _scatterCorner;

            Vector2Int pacmanTile = _pacman.CurrentTile;
            Vector2 pacmanDir = _pacman.CurrentDirection;
            
            // Calculate target 4 tiles ahead
            Vector2Int target = pacmanTile + new Vector2Int(
                Mathf.RoundToInt(pacmanDir.x * _tilesAhead),
                Mathf.RoundToInt(pacmanDir.y * _tilesAhead)
            );

            // Original bug: when Pacman faces up, also add 4 tiles left
            // (This recreates the classic overflow bug)
            if (pacmanDir == Vector2.up)
            {
                target.x -= _tilesAhead;
            }

            return target;
        }
    }
}
