using UnityEngine;
using Pacman.Core;

namespace Pacman.Ghosts
{
    /// <summary>
    /// Inky (Cyan Ghost) - "Bashful" - Flanker
    /// Uses a complex targeting based on both Pacman and Blinky positions.
    /// The most unpredictable ghost.
    /// </summary>
    public class Inky : GhostBase
    {
        [Header("=== INKY SETTINGS ===")]
        [SerializeField] private Blinky _blinky;
        [SerializeField] private int _tilesAhead = 2;

        protected override void Awake()
        {
            base.Awake();
            
            _ghostName = "Inky";
            _ghostColor = Color.cyan;
        }

        protected override void Start()
        {
            // Set Inky-specific values from GameConstants
            if (_gameConstants != null)
            {
                _ghostColor = _gameConstants.InkyColor;
                _scatterCorner = _gameConstants.InkyScatterCorner;
                _spawnPosition = _gameConstants.InkySpawnPosition;
            }

            base.Start();
        }

        /// <summary>
        /// Sets the reference to Blinky (needed for targeting calculation)
        /// </summary>
        public void SetBlinkyReference(Blinky blinky)
        {
            _blinky = blinky;
        }

        /// <summary>
        /// Inky's target is calculated using Blinky's position:
        /// 1. Get tile 2 tiles ahead of Pacman
        /// 2. Draw vector from Blinky to that tile
        /// 3. Double the vector length for final target
        /// </summary>
        protected override Vector2Int GetChaseTarget()
        {
            if (_pacman == null)
                return _scatterCorner;

            // Get tile 2 ahead of Pacman
            Vector2Int pacmanTile = _pacman.CurrentTile;
            Vector2 pacmanDir = _pacman.CurrentDirection;
            
            Vector2Int tileAhead = pacmanTile + new Vector2Int(
                Mathf.RoundToInt(pacmanDir.x * _tilesAhead),
                Mathf.RoundToInt(pacmanDir.y * _tilesAhead)
            );

            // Original bug: when Pacman faces up, also add 2 tiles left
            if (pacmanDir == Vector2.up)
            {
                tileAhead.x -= _tilesAhead;
            }

            // If no Blinky reference, just target ahead of Pacman
            if (_blinky == null)
                return tileAhead;

            // Get Blinky's position
            Vector2Int blinkyTile = _blinky.CurrentTile;

            // Calculate vector from Blinky to tile ahead
            Vector2Int vectorToBlinky = tileAhead - blinkyTile;

            // Double the vector
            Vector2Int target = tileAhead + vectorToBlinky;

            return target;
        }
    }
}
