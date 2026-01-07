using UnityEngine;
using System.Collections.Generic;
using Pacman.Core;
using Pacman.Maze;
using Pacman.Collectibles;

namespace Pacman.Ghosts
{
    /// <summary>
    /// Manages all ghosts in the game.
    /// Handles spawning, release timing, and coordinated behaviors.
    /// </summary>
    public class GhostManager : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [SerializeField] private GameConstants _gameConstants;
        [SerializeField] private MazeGenerator _mazeGenerator;

        [Header("=== GHOSTS ===")]
        [SerializeField] private Blinky _blinky;
        [SerializeField] private Pinky _pinky;
        [SerializeField] private Inky _inky;
        [SerializeField] private Clyde _clyde;

        [Header("=== STATE ===")]
        [SerializeField] private int _dotsEaten;
        [SerializeField] private bool _pinkyReleased;
        [SerializeField] private bool _inkyReleased;
        [SerializeField] private bool _clydeReleased;

        private List<GhostBase> _allGhosts = new List<GhostBase>();

        public Blinky Blinky => _blinky;
        public List<GhostBase> AllGhosts => _allGhosts;

        private void Awake()
        {
            // Collect all ghosts
            if (_blinky != null) _allGhosts.Add(_blinky);
            if (_pinky != null) _allGhosts.Add(_pinky);
            if (_inky != null) _allGhosts.Add(_inky);
            if (_clyde != null) _allGhosts.Add(_clyde);
        }

        private void OnEnable()
        {
            DotManager.OnDotCountChanged += OnDotCountChanged;
        }

        private void OnDisable()
        {
            DotManager.OnDotCountChanged -= OnDotCountChanged;
        }

        /// <summary>
        /// Spawns all ghosts at their initial positions
        /// </summary>
        public void SpawnGhosts()
        {
            foreach (var ghost in _allGhosts)
            {
                ghost.InitializePosition();
            }

            // Blinky starts outside
            if (_blinky != null)
            {
                _blinky.Release();
            }

            // Reset release flags
            _dotsEaten = 0;
            _pinkyReleased = false;
            _inkyReleased = false;
            _clydeReleased = false;

            // Pinky releases immediately
            if (_pinky != null)
            {
                _pinky.Release();
                _pinkyReleased = true;
            }
        }

        /// <summary>
        /// Called when dot count changes
        /// </summary>
        private void OnDotCountChanged(int count)
        {
            _dotsEaten = count;
            CheckGhostRelease();
        }

        /// <summary>
        /// Checks if any ghost should be released
        /// </summary>
        private void CheckGhostRelease()
        {
            // Release Inky after threshold
            if (!_inkyReleased && _dotsEaten >= _gameConstants.InkyExitDotCount)
            {
                _inky?.Release();
                _inkyReleased = true;
            }

            // Release Clyde after threshold
            if (!_clydeReleased && _dotsEaten >= _gameConstants.ClydeExitDotCount)
            {
                _clyde?.Release();
                _clydeReleased = true;
            }
        }

        /// <summary>
        /// Resets all ghosts to initial state
        /// </summary>
        public void ResetGhosts()
        {
            foreach (var ghost in _allGhosts)
            {
                ghost.ResetGhost();
            }

            _dotsEaten = 0;
            _pinkyReleased = false;
            _inkyReleased = false;
            _clydeReleased = false;

            // Blinky starts outside immediately
            if (_blinky != null)
            {
                _blinky.Release();
            }

            // Pinky releases immediately
            if (_pinky != null)
            {
                _pinky.Release();
                _pinkyReleased = true;
            }
        }

        /// <summary>
        /// Gets a ghost by name
        /// </summary>
        public GhostBase GetGhost(string name)
        {
            foreach (var ghost in _allGhosts)
            {
                if (ghost.GhostName == name)
                    return ghost;
            }
            return null;
        }

        /// <summary>
        /// Sets all ghosts to frightened state
        /// </summary>
        public void SetAllFrightened()
        {
            foreach (var ghost in _allGhosts)
            {
                if (ghost.CurrentState != GhostState.Caged && 
                    ghost.CurrentState != GhostState.Eaten)
                {
                    ghost.SetState(GhostState.Frightened);
                }
            }
        }

        /// <summary>
        /// Ends frightened mode for all ghosts
        /// </summary>
        public void EndAllFrightened()
        {
            foreach (var ghost in _allGhosts)
            {
                if (ghost.CurrentState == GhostState.Frightened)
                {
                    ghost.SetState(GhostState.Scatter);
                }
            }
        }
    }
}
