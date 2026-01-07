using UnityEngine;
using System;

namespace Pacman.Collectibles
{
    /// <summary>
    /// Base class for collectible dots in Pacman.
    /// </summary>
    public class Dot : MonoBehaviour
    {
        [Header("=== SETTINGS ===")]
        [SerializeField] protected int _points = 10;
        [SerializeField] protected bool _isPowerPellet = false;

        [Header("=== COMPONENTS ===")]
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] protected CircleCollider2D _collider;

        // Events
        public static event Action<Dot> OnDotCollected;
        public static event Action<Dot> OnPowerPelletCollected;

        // Properties
        public int Points => _points;
        public bool IsPowerPellet => _isPowerPellet;
        public Vector2Int TilePosition { get; set; }

        protected virtual void Awake()
        {
            // Get or add components
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (_collider == null)
                _collider = GetComponent<CircleCollider2D>();

            // Setup collider as trigger
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }
        }

        /// <summary>
        /// Initialize the dot with sprite and settings
        /// </summary>
        public virtual void Initialize(Sprite sprite, int points, bool isPowerPellet = false)
        {
            _points = points;
            _isPowerPellet = isPowerPellet;

            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.sortingOrder = 5;

            if (_collider == null)
                _collider = gameObject.AddComponent<CircleCollider2D>();
            
            _collider.isTrigger = true;
            _collider.radius = 0.3f;

            // Set layer (no tags - using component detection instead)
            gameObject.layer = LayerMask.NameToLayer("Default");
        }

        /// <summary>
        /// Called when Pacman collects this dot
        /// </summary>
        public virtual void Collect()
        {
            if (_isPowerPellet)
            {
                OnPowerPelletCollected?.Invoke(this);
            }
            else
            {
                OnDotCollected?.Invoke(this);
            }

            // Deactivate instead of destroy for pooling
            gameObject.SetActive(false);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // Check for PacmanController component instead of tag
            if (other.GetComponent<Pacman.Player.PacmanController>() != null)
            {
                Collect();
            }
        }

        /// <summary>
        /// Reset for object pooling
        /// </summary>
        public virtual void ResetDot()
        {
            gameObject.SetActive(true);
        }
    }
}
