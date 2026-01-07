using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Pacman.Core;
using Pacman.Maze;
using Pacman.Utilities;

namespace Pacman.Player
{
    /// <summary>
    /// Controls Pacman's movement, animation, and state.
    /// Features grid-based movement with input buffering.
    /// Uses the new Input System.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PacmanController : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [SerializeField] private GameConstants _gameConstants;
        [SerializeField] private MazeGenerator _mazeGenerator;

        [Header("=== COMPONENTS ===")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private CircleCollider2D _collider;

        [Header("=== SETTINGS ===")]
        [SerializeField] private int _spriteSize = 32;

        // Events
        public static event Action OnPacmanDeath;
        public static event Action OnPacmanRespawn;

        // Movement state
        private Vector2 _currentDirection;
        private Vector2 _queuedDirection;
        private Vector2Int _currentTile;
        private Vector2Int _targetTile;
        private bool _isMoving;
        private bool _canMove = false;  // Start disabled, enabled after full initialization
        private bool _isEating;
        private bool _isInitialized = false;

        // Animation state
        private float _mouthAngle;
        private float _mouthAnimTimer;
        private bool _mouthOpening = true;

        // Sprites for animation
        private Sprite[] _pacmanSprites;
        private int _currentSpriteIndex;

        // Input
        private Vector2 _inputDirection;

        // Properties
        public Vector2 CurrentDirection => _currentDirection;
        public Vector2Int CurrentTile => _currentTile;
        public bool IsMoving => _isMoving;
        public bool CanMove
        {
            get => _canMove;
            set => _canMove = value;
        }

        private void Awake()
        {
            // Get components
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_collider == null) _collider = GetComponent<CircleCollider2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

            // Setup rigidbody (with null check)
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            // Setup collider (with null check)
            if (_collider != null)
            {
                _collider.radius = 0.4f;
                _collider.isTrigger = false;
            }

            // No tags - using component-based detection
        }

        private void Start()
        {
            // Wait for references to be set by GameBootstrapper
            if (_gameConstants == null || _mazeGenerator == null)
            {
                Debug.LogWarning("PacmanController: Waiting for references to be set...");
                return;
            }
            
            CreateSprites();
            InitializePosition();
            _isInitialized = true;
        }

        /// <summary>
        /// Creates procedural Pacman sprites for animation
        /// </summary>
        private void CreateSprites()
        {
            // Create sprites for different mouth angles
            _pacmanSprites = new Sprite[4];
            float[] angles = { 0f, 15f, 30f, 45f };
            
            for (int i = 0; i < 4; i++)
            {
                _pacmanSprites[i] = ProceduralSpriteGenerator.CreatePacman(
                    _spriteSize, 
                    _gameConstants.PacmanColor, 
                    angles[i],
                    Vector2.right
                );
            }

            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            _spriteRenderer.sprite = _pacmanSprites[3];
            _spriteRenderer.sortingOrder = 10;
        }

        /// <summary>
        /// Initialize Pacman at spawn position
        /// </summary>
        public void InitializePosition()
        {
            Vector2 spawnPos = _gameConstants.PacmanSpawnPosition;
            transform.position = _mazeGenerator.GetWorldPosition(spawnPos.x, spawnPos.y);
            
            _currentTile = new Vector2Int(Mathf.RoundToInt(spawnPos.x), Mathf.RoundToInt(spawnPos.y));
            _currentDirection = Vector2.left;
            _queuedDirection = Vector2.zero;
            _isMoving = false;
            
            UpdateSpriteRotation();
        }

        private void Update()
        {
            if (!_canMove) return;

            HandleInput();
            UpdateMouthAnimation();
        }

        private void FixedUpdate()
        {
            if (!_canMove) return;

            Move();
            HandleTunnelWrap();
        }

        /// <summary>
        /// Handles player input using new Input System
        /// </summary>
        private void HandleInput()
        {
            // Read from Keyboard using new Input System
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            Vector2 inputDir = Vector2.zero;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                inputDir = Vector2.up;
            else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                inputDir = Vector2.down;
            else if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                inputDir = Vector2.left;
            else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                inputDir = Vector2.right;

            // Also check Gamepad
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && inputDir == Vector2.zero)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.magnitude > 0.5f)
                {
                    if (Mathf.Abs(stick.x) > Mathf.Abs(stick.y))
                        inputDir = stick.x > 0 ? Vector2.right : Vector2.left;
                    else
                        inputDir = stick.y > 0 ? Vector2.up : Vector2.down;
                }
            }

            if (inputDir != Vector2.zero)
            {
                SetDirection(inputDir);
            }
        }

        /// <summary>
        /// Sets the movement direction with input buffering
        /// </summary>
        public void SetDirection(Vector2 direction)
        {
            // Normalize to cardinal direction
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                direction = new Vector2(Mathf.Sign(direction.x), 0);
            else
                direction = new Vector2(0, Mathf.Sign(direction.y));

            // Try to change direction immediately if possible
            Vector2Int dirInt = Vector2IntFromVector2(direction);
            if (CanMoveInDirection(dirInt))
            {
                _currentDirection = direction;
                _queuedDirection = Vector2.zero;
                UpdateSpriteRotation();
            }
            else
            {
                // Queue the direction for later
                _queuedDirection = direction;
            }
        }

        /// <summary>
        /// Main movement logic
        /// </summary>
        private void Move()
        {
            // Try queued direction first
            if (_queuedDirection != Vector2.zero)
            {
                Vector2Int queuedDirInt = Vector2IntFromVector2(_queuedDirection);
                if (CanMoveInDirection(queuedDirInt))
                {
                    _currentDirection = _queuedDirection;
                    _queuedDirection = Vector2.zero;
                    UpdateSpriteRotation();
                }
            }

            // Check if current direction is valid
            Vector2Int currentDirInt = Vector2IntFromVector2(_currentDirection);
            if (!CanMoveInDirection(currentDirInt))
            {
                _isMoving = false;
                return;
            }

            _isMoving = true;

            // Calculate speed
            float speed = _gameConstants.GetPacmanSpeed(_isEating, IsInTunnel());
            
            // Move
            Vector2 movement = _currentDirection * speed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + movement);

            // Update current tile
            _currentTile = _mazeGenerator.GetTilePosition(transform.position);
        }

        /// <summary>
        /// Checks if Pacman can move in a direction
        /// </summary>
        private bool CanMoveInDirection(Vector2Int direction)
        {
            // Align to grid for checking
            Vector2Int checkTile = _currentTile + direction;
            
            // Handle tunnel wrap
            var mazeData = _mazeGenerator.MazeData;
            if (checkTile.x < 0) checkTile.x = mazeData.Width - 1;
            else if (checkTile.x >= mazeData.Width) checkTile.x = 0;

            return mazeData.IsWalkable(checkTile.x, checkTile.y);
        }

        /// <summary>
        /// Handles tunnel wrap-around
        /// </summary>
        private void HandleTunnelWrap()
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
        /// Checks if Pacman is in tunnel area
        /// </summary>
        private bool IsInTunnel()
        {
            return _mazeGenerator.IsInTunnel(transform.position);
        }

        /// <summary>
        /// Updates the mouth animation
        /// </summary>
        private void UpdateMouthAnimation()
        {
            if (_gameConstants == null || _pacmanSprites == null) return;
            
            if (!_isMoving)
            {
                // Show open mouth when stopped
                _currentSpriteIndex = 3;
                UpdateSprite();
                return;
            }

            _mouthAnimTimer += Time.deltaTime * _gameConstants.MouthAnimationSpeed;
            
            // Oscillate between 0 and 3
            _currentSpriteIndex = Mathf.RoundToInt(Mathf.PingPong(_mouthAnimTimer, 3));
            UpdateSprite();
        }

        /// <summary>
        /// Updates the sprite based on current animation frame
        /// </summary>
        private void UpdateSprite()
        {
            if (_spriteRenderer != null && _pacmanSprites != null && _currentSpriteIndex < _pacmanSprites.Length)
            {
                _spriteRenderer.sprite = _pacmanSprites[_currentSpriteIndex];
            }
        }

        /// <summary>
        /// Updates sprite rotation to face movement direction
        /// </summary>
        private void UpdateSpriteRotation()
        {
            float angle = Mathf.Atan2(_currentDirection.y, _currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// Called when Pacman dies
        /// </summary>
        public void Die()
        {
            _canMove = false;
            _isMoving = false;
            OnPacmanDeath?.Invoke();
            
            // Start death animation
            StartCoroutine(DeathAnimation());
        }

        /// <summary>
        /// Death animation coroutine
        /// </summary>
        private System.Collections.IEnumerator DeathAnimation()
        {
            // Simple spin and shrink
            float duration = 1.5f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                transform.Rotate(0, 0, 720 * Time.deltaTime);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                
                yield return null;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Respawn Pacman
        /// </summary>
        public void Respawn()
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            
            InitializePosition();
            _canMove = true;
            
            OnPacmanRespawn?.Invoke();
        }

        /// <summary>
        /// Sets whether Pacman is currently eating (affects speed)
        /// </summary>
        public void SetEating(bool eating)
        {
            _isEating = eating;
        }

        /// <summary>
        /// Helper to convert Vector2 to Vector2Int
        /// </summary>
        private Vector2Int Vector2IntFromVector2(Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ghost collision is handled by GhostBase.OnTriggerEnter2D
            // No tag-based detection needed here
        }
    }
}
