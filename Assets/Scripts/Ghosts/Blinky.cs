using UnityEngine;
using Pacman.Core;

namespace Pacman.Ghosts
{
    /// <summary>
    /// Blinky (Red Ghost) - "Shadow" - Direct Chaser
    /// Always targets Pacman's current position.
    /// The most aggressive ghost.
    /// </summary>
    public class Blinky : GhostBase
    {
        protected override void Awake()
        {
            base.Awake();
            
            _ghostName = "Blinky";
            _ghostColor = Color.red;
        }

        protected override void Start()
        {
            // Set Blinky-specific values from GameConstants
            if (_gameConstants != null)
            {
                _ghostColor = _gameConstants.BlinkyColor;
                _scatterCorner = _gameConstants.BlinkyScatterCorner;
                _spawnPosition = _gameConstants.BlinkySpawnPosition;
            }

            base.Start();

            // Blinky starts outside the ghost house, moving LEFT (not up!)
            _currentDirection = Vector2.left;
            SetState(GhostState.Scatter);
        }

        /// <summary>
        /// Override to set initial direction to LEFT for Blinky
        /// </summary>
        public override void InitializePosition()
        {
            base.InitializePosition();
            // Blinky starts moving LEFT (classic Pacman behavior)
            _currentDirection = Vector2.left;
        }

        /// <summary>
        /// Blinky directly chases Pacman's current tile
        /// </summary>
        protected override Vector2Int GetChaseTarget()
        {
            if (_pacman == null)
                return _scatterCorner;

            return _pacman.CurrentTile;
        }

        /// <summary>
        /// Blinky resets outside the ghost house
        /// </summary>
        public override void ResetGhost()
        {
            InitializePosition();
            _modeIndex = 0;
            _modeTimer = _gameConstants.ScatterDurations[0];
            _isInChaseMode = false;
            _isBlinking = false;
            
            // Blinky starts outside, moving LEFT
            _currentDirection = Vector2.left;
            SetState(GhostState.Scatter);
        }
    }
}
