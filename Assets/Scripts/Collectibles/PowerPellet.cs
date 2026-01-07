using UnityEngine;

namespace Pacman.Collectibles
{
    /// <summary>
    /// Power pellet that makes ghosts frightened when collected.
    /// Features a pulsing animation.
    /// </summary>
    public class PowerPellet : Dot
    {
        [Header("=== POWER PELLET SETTINGS ===")]
        [SerializeField] private float _pulseSpeed = 4f;
        [SerializeField] private float _minAlpha = 0.3f;
        [SerializeField] private float _maxAlpha = 1f;

        private float _pulseTimer;

        protected override void Awake()
        {
            base.Awake();
            _isPowerPellet = true;
            _points = 50;
        }

        private void Update()
        {
            // Pulsing animation
            _pulseTimer += Time.deltaTime * _pulseSpeed;
            float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, (Mathf.Sin(_pulseTimer) + 1f) / 2f);
            
            if (_spriteRenderer != null)
            {
                Color color = _spriteRenderer.color;
                color.a = alpha;
                _spriteRenderer.color = color;
            }
        }

        public override void Initialize(Sprite sprite, int points, bool isPowerPellet = false)
        {
            base.Initialize(sprite, points, true);
            
            // Larger collider for power pellet
            if (_collider != null)
            {
                _collider.radius = 0.4f;
            }
        }

        public override void ResetDot()
        {
            base.ResetDot();
            _pulseTimer = 0f;
            
            if (_spriteRenderer != null)
            {
                Color color = _spriteRenderer.color;
                color.a = 1f;
                _spriteRenderer.color = color;
            }
        }
    }
}
