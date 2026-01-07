using UnityEngine;

namespace Pacman.Core
{
    /// <summary>
    /// ScriptableObject containing all game constants for Pacman.
    /// Includes colors, speeds, scoring, and maze dimensions.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConstants", menuName = "Pacman/Game Constants")]
    public class GameConstants : ScriptableObject
    {
        [Header("=== MAZE DIMENSIONS ===")]
        [Tooltip("Width of the maze in tiles")]
        public int MazeWidth = 28;
        
        [Tooltip("Height of the maze in tiles")]
        public int MazeHeight = 31;
        
        [Tooltip("Size of each tile in world units")]
        public float TileSize = 1f;

        [Header("=== COLORS ===")]
        [Tooltip("Wall color - Blue")]
        public Color WallColor = new Color(0.13f, 0.13f, 1f, 1f); // #2121FF
        
        [Tooltip("Background color - Black")]
        public Color BackgroundColor = Color.black; // #000000
        
        [Tooltip("Dot/Pellet color - Cream/Pink")]
        public Color DotColor = new Color(1f, 0.72f, 0.68f, 1f); // #FFB8AE
        
        [Tooltip("Ghost house door color")]
        public Color GhostDoorColor = new Color(1f, 0.72f, 1f, 1f); // #FFB8FF

        [Header("=== CHARACTER COLORS ===")]
        [Tooltip("Pacman color - Yellow")]
        public Color PacmanColor = Color.yellow; // #FFFF00
        
        [Tooltip("Blinky (Red Ghost) color")]
        public Color BlinkyColor = Color.red; // #FF0000
        
        [Tooltip("Pinky (Pink Ghost) color")]
        public Color PinkyColor = new Color(1f, 0.72f, 1f, 1f); // #FFB8FF
        
        [Tooltip("Inky (Cyan Ghost) color")]
        public Color InkyColor = Color.cyan; // #00FFFF
        
        [Tooltip("Clyde (Orange Ghost) color")]
        public Color ClydeColor = new Color(1f, 0.72f, 0.32f, 1f); // #FFB852
        
        [Tooltip("Frightened ghost color - Blue")]
        public Color FrightenedGhostColor = new Color(0.2f, 0.2f, 0.8f, 1f);
        
        [Tooltip("Frightened ghost blink color - White")]
        public Color FrightenedBlinkColor = Color.white;

        [Header("=== SPEEDS (as multipliers of base speed) ===")]
        [Tooltip("Base movement speed in tiles per second")]
        public float BaseSpeed = 8f;
        
        [Tooltip("Pacman normal speed multiplier")]
        [Range(0f, 2f)]
        public float PacmanNormalSpeed = 0.8f;
        
        [Tooltip("Pacman speed while eating dots")]
        [Range(0f, 2f)]
        public float PacmanEatingSpeed = 0.71f;
        
        [Tooltip("Pacman speed in tunnel")]
        [Range(0f, 2f)]
        public float PacmanTunnelSpeed = 0.4f;
        
        [Tooltip("Ghost normal speed multiplier")]
        [Range(0f, 2f)]
        public float GhostNormalSpeed = 0.75f;
        
        [Tooltip("Ghost speed in tunnel")]
        [Range(0f, 2f)]
        public float GhostTunnelSpeed = 0.4f;
        
        [Tooltip("Ghost frightened speed")]
        [Range(0f, 2f)]
        public float GhostFrightenedSpeed = 0.5f;
        
        [Tooltip("Ghost eaten (eyes only) speed")]
        [Range(0f, 3f)]
        public float GhostEatenSpeed = 1.5f;

        [Header("=== SCORING ===")]
        [Tooltip("Points for eating a small dot")]
        public int SmallDotPoints = 10;
        
        [Tooltip("Points for eating a power pellet")]
        public int PowerPelletPoints = 50;
        
        [Tooltip("Points for eating ghosts (multiplied)")]
        public int[] GhostEatPoints = { 200, 400, 800, 1600 };
        
        [Tooltip("Score threshold for extra life")]
        public int ExtraLifeScore = 10000;

        [Header("=== GAME SETTINGS ===")]
        [Tooltip("Starting number of lives")]
        public int StartingLives = 3;
        
        [Tooltip("Maximum number of lives")]
        public int MaxLives = 5;
        
        [Tooltip("Duration of frightened mode in seconds (Level 1)")]
        public float FrightenedDuration = 6f;
        
        [Tooltip("Duration of frightened blink warning")]
        public float FrightenedBlinkDuration = 2f;
        
        [Tooltip("Blink frequency during warning")]
        public float FrightenedBlinkRate = 0.2f;

        [Header("=== GHOST MODE TIMINGS (Level 1) ===")]
        [Tooltip("Scatter mode duration in seconds")]
        public float[] ScatterDurations = { 7f, 7f, 5f, 5f };
        
        [Tooltip("Chase mode duration in seconds")]
        public float[] ChaseDurations = { 20f, 20f, 20f, -1f }; // -1 = infinite

        [Header("=== GHOST HOUSE SETTINGS ===")]
        [Tooltip("Dots required before Inky exits")]
        public int InkyExitDotCount = 30;
        
        [Tooltip("Dots required before Clyde exits")]
        public int ClydeExitDotCount = 60;
        
        [Tooltip("Time between ghost exits")]
        public float GhostExitDelay = 0.5f;

        [Header("=== SPAWN POSITIONS ===")]
        [Tooltip("Pacman spawn position (tile coordinates)")]
        public Vector2 PacmanSpawnPosition = new Vector2(13.5f, 7f);
        
        [Tooltip("Blinky spawn position (outside house)")]
        public Vector2 BlinkySpawnPosition = new Vector2(13.5f, 19f);
        
        [Tooltip("Pinky spawn position (inside house center)")]
        public Vector2 PinkySpawnPosition = new Vector2(13.5f, 16f);
        
        [Tooltip("Inky spawn position (inside house left)")]
        public Vector2 InkySpawnPosition = new Vector2(11.5f, 16f);
        
        [Tooltip("Clyde spawn position (inside house right)")]
        public Vector2 ClydeSpawnPosition = new Vector2(15.5f, 16f);

        [Header("=== SCATTER CORNERS ===")]
        [Tooltip("Blinky scatter corner (top-right)")]
        public Vector2Int BlinkyScatterCorner = new Vector2Int(25, 0);
        
        [Tooltip("Pinky scatter corner (top-left)")]
        public Vector2Int PinkyScatterCorner = new Vector2Int(2, 0);
        
        [Tooltip("Inky scatter corner (bottom-right)")]
        public Vector2Int InkyScatterCorner = new Vector2Int(27, 30);
        
        [Tooltip("Clyde scatter corner (bottom-left)")]
        public Vector2Int ClydeScatterCorner = new Vector2Int(0, 30);

        [Header("=== ANIMATION SETTINGS ===")]
        [Tooltip("Pacman mouth animation speed")]
        public float MouthAnimationSpeed = 15f;
        
        [Tooltip("Pacman maximum mouth angle")]
        public float MaxMouthAngle = 45f;
        
        [Tooltip("Power pellet pulse speed")]
        public float PowerPelletPulseSpeed = 4f;

        // === HELPER METHODS ===
        
        /// <summary>
        /// Get the actual speed value for Pacman
        /// </summary>
        public float GetPacmanSpeed(bool isEating = false, bool inTunnel = false)
        {
            float multiplier = isEating ? PacmanEatingSpeed : PacmanNormalSpeed;
            if (inTunnel) multiplier = PacmanTunnelSpeed;
            return BaseSpeed * multiplier;
        }

        /// <summary>
        /// Get the actual speed value for a ghost
        /// </summary>
        public float GetGhostSpeed(GhostState state, bool inTunnel = false)
        {
            float multiplier = state switch
            {
                GhostState.Frightened => GhostFrightenedSpeed,
                GhostState.Eaten => GhostEatenSpeed,
                _ => GhostNormalSpeed
            };
            
            if (inTunnel && state != GhostState.Eaten)
                multiplier = GhostTunnelSpeed;
                
            return BaseSpeed * multiplier;
        }

        /// <summary>
        /// Get points for eating a ghost based on combo count
        /// </summary>
        public int GetGhostPoints(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= GhostEatPoints.Length)
                return GhostEatPoints[GhostEatPoints.Length - 1];
            return GhostEatPoints[comboIndex];
        }

        /// <summary>
        /// Convert tile position to world position
        /// </summary>
        public Vector3 TileToWorld(Vector2 tilePos)
        {
            return new Vector3(tilePos.x * TileSize, tilePos.y * TileSize, 0f);
        }

        /// <summary>
        /// Convert world position to tile position
        /// </summary>
        public Vector2Int WorldToTile(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / TileSize),
                Mathf.FloorToInt(worldPos.y / TileSize)
            );
        }
    }

    /// <summary>
    /// Ghost behavior states
    /// </summary>
    public enum GhostState
    {
        Caged,      // Inside ghost house, waiting to exit
        Exiting,    // Leaving ghost house
        Scatter,    // Going to scatter corner
        Chase,      // Chasing Pacman with personality
        Frightened, // Blue, can be eaten
        Eaten       // Eyes only, returning to house
    }

    /// <summary>
    /// Game states
    /// </summary>
    public enum GameState
    {
        Ready,          // "READY!" displayed
        Playing,        // Normal gameplay
        PacmanDeath,    // Death animation
        GhostEaten,     // Brief pause when ghost eaten
        LevelComplete,  // All dots eaten
        GameOver        // No lives remaining
    }
}
