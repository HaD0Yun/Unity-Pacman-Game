using UnityEngine;
using System.Collections.Generic;
using Pacman.Core;
using Pacman.Maze;
using Pacman.Player;
using Pacman.Utilities;

namespace Pacman.Ghosts
{
    /// <summary>
    /// Base class for all ghost AI.
    /// Handles movement, state transitions, and basic behavior.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public abstract class GhostBase : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [SerializeField] protected GameConstants _gameConstants;
        [SerializeField] protected MazeGenerator _mazeGenerator;
        [SerializeField] protected PacmanController _pacman;

        [Header("=== COMPONENTS ===")]
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] protected Rigidbody2D _rb;
        [SerializeField] protected CircleCollider2D _collider;

        [Header("=== GHOST IDENTITY ===")]
        [SerializeField] protected string _ghostName;
        [SerializeField] protected Color _ghostColor;
        [SerializeField] protected Vector2Int _scatterCorner;
        [SerializeField] protected Vector2 _spawnPosition;

        [Header("=== STATE ===")]
        [SerializeField] protected GhostState _currentState = GhostState.Caged;
        [SerializeField] protected Vector2 _currentDirection;
        [SerializeField] protected Vector2Int _currentTile;
        [SerializeField] protected Vector2Int _targetTile;

        // Sprites
        protected Sprite _normalSprite;
        protected Sprite _frightenedSprite;
        protected Sprite _frightenedBlinkSprite;
        protected Sprite _eyesSprite;

        // State
        protected bool _canMove = true;
        protected bool _isBlinking;
        protected float _modeTimer;
        protected int _modeIndex;
        protected bool _isInChaseMode;

        // Properties
        public GhostState CurrentState => _currentState;
        public Vector2Int CurrentTile => _currentTile;
        public string GhostName => _ghostName;

        protected virtual void Awake()
        {
            // Get components
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_collider == null) _collider = GetComponent<CircleCollider2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

            // Setup rigidbody
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Setup collider
            _collider.radius = 0.4f;
            _collider.isTrigger = true;

            // No tags - using component-based detection
        }

        protected virtual void Start()
        {
            CreateSprites();
            InitializePosition();
        }

        protected virtual void OnEnable()
        {
            GameManager.OnFrightenedModeStart += OnFrightenedModeStart;
            GameManager.OnFrightenedModeEnd += OnFrightenedModeEnd;
            GameManager.OnFrightenedModeWarning += OnFrightenedModeWarning;
        }

        protected virtual void OnDisable()
        {
            GameManager.OnFrightenedModeStart -= OnFrightenedModeStart;
            GameManager.OnFrightenedModeEnd -= OnFrightenedModeEnd;
            GameManager.OnFrightenedModeWarning -= OnFrightenedModeWarning;
        }

        /// <summary>
        /// Creates the ghost sprites
        /// </summary>
        protected virtual void CreateSprites()
        {
            int spriteSize = 32;
            
            _normalSprite = ProceduralSpriteGenerator.CreateGhost(spriteSize, spriteSize, _ghostColor, Vector2.up);
            _frightenedSprite = ProceduralSpriteGenerator.CreateFrightenedGhost(spriteSize, spriteSize, _gameConstants.FrightenedGhostColor, false);
            _frightenedBlinkSprite = ProceduralSpriteGenerator.CreateFrightenedGhost(spriteSize, spriteSize, _gameConstants.FrightenedBlinkColor, true);
            _eyesSprite = ProceduralSpriteGenerator.CreateGhostEyes(spriteSize, spriteSize, Vector2.up);

            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            _spriteRenderer.sprite = _normalSprite;
            _spriteRenderer.sortingOrder = 9;
        }

        /// <summary>
        /// Initialize ghost at spawn position
        /// </summary>
        public virtual void InitializePosition()
        {
            transform.position = _mazeGenerator.GetWorldPosition(_spawnPosition.x, _spawnPosition.y);
            _currentTile = new Vector2Int(Mathf.RoundToInt(_spawnPosition.x), Mathf.RoundToInt(_spawnPosition.y));
            _currentDirection = Vector2.up;
            
            UpdateSprite();
        }

        protected virtual void Update()
        {
            if (_currentState != GhostState.Frightened)
            {
                UpdateModeTimer();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!_canMove) return;

            switch (_currentState)
            {
                case GhostState.Caged:
                    UpdateCagedBehavior();
                    break;
                case GhostState.Exiting:
                    UpdateExitingBehavior();
                    break;
                case GhostState.Chase:
                case GhostState.Scatter:
                    UpdateNormalMovement();
                    break;
                case GhostState.Frightened:
                    UpdateFrightenedMovement();
                    break;
                case GhostState.Eaten:
                    UpdateEatenMovement();
                    break;
            }

            HandleTunnelWrap();
        }

        /// <summary>
        /// Updates the scatter/chase mode timer
        /// </summary>
        protected virtual void UpdateModeTimer()
        {
            if (_currentState == GhostState.Caged || 
                _currentState == GhostState.Exiting || 
                _currentState == GhostState.Eaten)
                return;

            _modeTimer -= Time.deltaTime;
            
            if (_modeTimer <= 0)
            {
                ToggleMode();
            }
        }

        /// <summary>
        /// Toggles between scatter and chase modes
        /// </summary>
        protected virtual void ToggleMode()
        {
            float[] durations = _isInChaseMode ? 
                _gameConstants.ScatterDurations : 
                _gameConstants.ChaseDurations;

            if (_modeIndex >= durations.Length)
            {
                // Stay in chase mode indefinitely
                SetState(GhostState.Chase);
                return;
            }

            float duration = durations[_modeIndex];
            
            if (duration < 0)
            {
                // Infinite chase
                SetState(GhostState.Chase);
                return;
            }

            _isInChaseMode = !_isInChaseMode;
            _modeTimer = duration;
            _modeIndex++;

            SetState(_isInChaseMode ? GhostState.Chase : GhostState.Scatter);
            
            // Reverse direction on mode change
            _currentDirection = -_currentDirection;
        }

        /// <summary>
        /// Sets the ghost state
        /// </summary>
        public virtual void SetState(GhostState newState)
        {
            GhostState previousState = _currentState;
            _currentState = newState;

            // Reverse direction when entering frightened
            if (newState == GhostState.Frightened && 
                previousState != GhostState.Caged && 
                previousState != GhostState.Exiting)
            {
                _currentDirection = -_currentDirection;
            }

            UpdateSprite();
        }

        /// <summary>
        /// Updates the sprite based on current state
        /// </summary>
        protected virtual void UpdateSprite()
        {
            switch (_currentState)
            {
                case GhostState.Frightened:
                    _spriteRenderer.sprite = _isBlinking ? _frightenedBlinkSprite : _frightenedSprite;
                    break;
                case GhostState.Eaten:
                    _spriteRenderer.sprite = _eyesSprite;
                    break;
                default:
                    _spriteRenderer.sprite = _normalSprite;
                    break;
            }
        }

        /// <summary>
        /// Updates movement during normal gameplay
        /// </summary>
        protected virtual void UpdateNormalMovement()
        {
            // Determine target based on state
            _targetTile = _currentState == GhostState.Chase ? 
                GetChaseTarget() : 
                _scatterCorner;

            Move(GetSpeed());
        }

        /// <summary>
        /// Updates movement during frightened mode
        /// </summary>
        protected virtual void UpdateFrightenedMovement()
        {
            Move(_gameConstants.GetGhostSpeed(GhostState.Frightened));
        }

        /// <summary>
        /// Updates movement when eaten (eyes returning to house)
        /// </summary>
        protected virtual void UpdateEatenMovement()
        {
            _targetTile = _mazeGenerator.MazeData.GetGhostHouseDoorPosition();
            Move(_gameConstants.GetGhostSpeed(GhostState.Eaten));

            // Check if reached ghost house
            if (_currentTile == _targetTile)
            {
                // Respawn in ghost house
                Respawn();
            }
        }

        /// <summary>
        /// Updates behavior while caged in ghost house
        /// </summary>
        protected virtual void UpdateCagedBehavior()
        {
            // Bob up and down in the ghost house
            float bobSpeed = 2f;
            float bobAmount = 0.2f;
            
            Vector3 pos = transform.position;
            pos.y = _spawnPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = pos;
        }

        /// <summary>
        /// Updates behavior while exiting ghost house
        /// </summary>
        protected virtual void UpdateExitingBehavior()
        {
            Vector2Int doorPos = _mazeGenerator.MazeData.GetGhostHouseDoorPosition();
            Vector3 targetPos = _mazeGenerator.GetWorldPosition(doorPos.x, doorPos.y);
            
            float speed = _gameConstants.GetGhostSpeed(GhostState.Exiting);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                // Exited ghost house
                _currentTile = doorPos;
                _currentDirection = Vector2.left;
                SetState(_isInChaseMode ? GhostState.Chase : GhostState.Scatter);
            }
        }

        /// <summary>
        /// Main movement logic
        /// </summary>
        protected virtual void Move(float speed)
        {
            // Get current tile
            _currentTile = _mazeGenerator.GetTilePosition(transform.position);

            // At intersection, choose direction
            if (IsAtIntersection())
            {
                ChooseDirection();
            }

            // Move in current direction
            Vector2 movement = _currentDirection * speed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + movement);
        }

        /// <summary>
        /// Checks if ghost is at an intersection
        /// </summary>
        protected virtual bool IsAtIntersection()
        {
            Vector3 worldPos = transform.position;
            Vector3 tileCenter = _mazeGenerator.GetWorldPosition(_currentTile.x, _currentTile.y);
            
            return Vector3.Distance(worldPos, tileCenter) < 0.1f;
        }

        /// <summary>
        /// Chooses the next direction at an intersection
        /// </summary>
        protected virtual void ChooseDirection()
        {
            Vector2Int[] possibleDirs = {
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.down,
                Vector2Int.right
            };

            Vector2Int reverseDir = Vector2IntFromVector2(-_currentDirection);
            Vector2Int bestDir = Vector2IntFromVector2(_currentDirection);
            float bestDistance = float.MaxValue;

            foreach (var dir in possibleDirs)
            {
                // Cannot reverse
                if (dir == reverseDir) continue;

                // Check if can move in this direction
                Vector2Int nextTile = _currentTile + dir;
                
                // Handle tunnel wrap
                var mazeData = _mazeGenerator.MazeData;
                if (nextTile.x < 0) nextTile.x = mazeData.Width - 1;
                else if (nextTile.x >= mazeData.Width) nextTile.x = 0;

                if (!mazeData.IsWalkable(nextTile.x, nextTile.y)) continue;

                // Ghost house restriction (can't enter unless eaten)
                if (_currentState != GhostState.Eaten && 
                    mazeData.IsInGhostHouseArea(nextTile.x, nextTile.y)) continue;

                // In frightened mode, choose randomly
                if (_currentState == GhostState.Frightened)
                {
                    if (Random.value > 0.5f || bestDistance == float.MaxValue)
                    {
                        bestDir = dir;
                        bestDistance = Random.value;
                    }
                    continue;
                }

                // Calculate distance to target
                float distance = Vector2Int.Distance(nextTile, _targetTile);
                
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDir = dir;
                }
            }

            _currentDirection = new Vector2(bestDir.x, bestDir.y);
        }

        /// <summary>
        /// Gets the ghost's chase target (to be implemented by subclasses)
        /// </summary>
        protected abstract Vector2Int GetChaseTarget();

        /// <summary>
        /// Gets current speed based on state and location
        /// </summary>
        protected virtual float GetSpeed()
        {
            bool inTunnel = _mazeGenerator.IsInTunnel(transform.position);
            return _gameConstants.GetGhostSpeed(_currentState, inTunnel);
        }

        /// <summary>
        /// Handles tunnel wrap-around
        /// </summary>
        protected virtual void HandleTunnelWrap()
        {
            Vector3 pos = transform.position;
            var mazeData = _mazeGenerator.MazeData;
            
            float leftBound = -0.5f;
            float rightBound = (mazeData.Width - 0.5f);

            if (pos.x < leftBound)
            {
                pos.x = rightBound - 0.1f;
                transform.position = pos;
                _rb.position = pos;
            }
            else if (pos.x > rightBound)
            {
                pos.x = leftBound + 0.1f;
                transform.position = pos;
                _rb.position = pos;
            }
        }

        /// <summary>
        /// Called when ghost is eaten by Pacman
        /// </summary>
        public virtual void Eaten()
        {
            SetState(GhostState.Eaten);
            GameManager.Instance?.OnGhostEaten();
        }

        /// <summary>
        /// Respawns the ghost in the ghost house
        /// </summary>
        public virtual void Respawn()
        {
            transform.position = _mazeGenerator.GetWorldPosition(_spawnPosition.x, _spawnPosition.y);
            _currentTile = new Vector2Int(Mathf.RoundToInt(_spawnPosition.x), Mathf.RoundToInt(_spawnPosition.y));
            
            SetState(GhostState.Exiting);
        }

        /// <summary>
        /// Resets the ghost to initial state
        /// </summary>
        public virtual void ResetGhost()
        {
            InitializePosition();
            _modeIndex = 0;
            _modeTimer = _gameConstants.ScatterDurations[0];
            _isInChaseMode = false;
            _isBlinking = false;
            SetState(GhostState.Caged);
        }

        /// <summary>
        /// Releases the ghost from the ghost house
        /// </summary>
        public virtual void Release()
        {
            if (_currentState == GhostState.Caged)
            {
                SetState(GhostState.Exiting);
            }
        }

        // Event handlers
        protected virtual void OnFrightenedModeStart()
        {
            if (_currentState != GhostState.Caged && 
                _currentState != GhostState.Eaten)
            {
                SetState(GhostState.Frightened);
            }
            _isBlinking = false;
        }

        protected virtual void OnFrightenedModeEnd()
        {
            if (_currentState == GhostState.Frightened)
            {
                SetState(_isInChaseMode ? GhostState.Chase : GhostState.Scatter);
            }
            _isBlinking = false;
        }

        protected virtual void OnFrightenedModeWarning()
        {
            _isBlinking = !_isBlinking;
            if (_currentState == GhostState.Frightened)
            {
                UpdateSprite();
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // Check for PacmanController component instead of tag
            PacmanController pacman = other.GetComponent<PacmanController>();
            if (pacman != null)
            {
                if (_currentState == GhostState.Frightened)
                {
                    Eaten();
                }
                else if (_currentState != GhostState.Eaten)
                {
                    // Kill Pacman
                    pacman.Die();
                }
            }
        }

        /// <summary>
        /// Helper to convert Vector2 to Vector2Int
        /// </summary>
        protected Vector2Int Vector2IntFromVector2(Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }
    }
}
