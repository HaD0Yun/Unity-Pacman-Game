using UnityEngine;
using UnityEngine.Tilemaps;
using Pacman.Core;
using Pacman.Utilities;

namespace Pacman.Maze
{
    /// <summary>
    /// Generates the Pacman maze using Unity Tilemaps.
    /// Creates walls, floor, and collision at runtime.
    /// </summary>
    public class MazeGenerator : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [SerializeField] private GameConstants _gameConstants;
        [SerializeField] private MazeData _mazeData;

        [Header("=== GENERATED OBJECTS ===")]
        [SerializeField] private Grid _grid;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private Tilemap _floorTilemap;

        [Header("=== SETTINGS ===")]
        [SerializeField] private int _tilePixelSize = 16;

        // Generated tiles
        private Tile _wallTile;
        private Tile _floorTile;

        // References for external access
        public Grid Grid => _grid;
        public Tilemap WallTilemap => _wallTilemap;
        public MazeData MazeData => _mazeData;
        public GameConstants GameConstants => _gameConstants;

        private void Awake()
        {
            if (_gameConstants == null || _mazeData == null)
            {
                Debug.LogError("MazeGenerator: GameConstants or MazeData not assigned!");
                return;
            }
        }

        /// <summary>
        /// Generates the entire maze
        /// </summary>
        public void GenerateMaze()
        {
            CreateGridHierarchy();
            CreateTiles();
            PopulateTilemaps();
            SetupCollision();
        }

        /// <summary>
        /// Creates the Grid and Tilemap GameObjects
        /// </summary>
        private void CreateGridHierarchy()
        {
            // Create Grid
            if (_grid == null)
            {
                GameObject gridObj = new GameObject("MazeGrid");
                gridObj.transform.SetParent(transform);
                gridObj.transform.localPosition = Vector3.zero;
                _grid = gridObj.AddComponent<Grid>();
                _grid.cellSize = new Vector3(_gameConstants.TileSize, _gameConstants.TileSize, 0);
            }

            // Create Floor Tilemap (rendered first)
            if (_floorTilemap == null)
            {
                GameObject floorObj = new GameObject("FloorTilemap");
                floorObj.transform.SetParent(_grid.transform);
                floorObj.transform.localPosition = Vector3.zero;
                _floorTilemap = floorObj.AddComponent<Tilemap>();
                var floorRenderer = floorObj.AddComponent<TilemapRenderer>();
                floorRenderer.sortingOrder = 0;
            }

            // Create Wall Tilemap (rendered on top)
            if (_wallTilemap == null)
            {
                GameObject wallObj = new GameObject("WallTilemap");
                wallObj.transform.SetParent(_grid.transform);
                wallObj.transform.localPosition = Vector3.zero;
                _wallTilemap = wallObj.AddComponent<Tilemap>();
                var wallRenderer = wallObj.AddComponent<TilemapRenderer>();
                wallRenderer.sortingOrder = 1;
            }
        }

        /// <summary>
        /// Creates procedural tile sprites
        /// </summary>
        private void CreateTiles()
        {
            // Create wall tile
            Sprite wallSprite = ProceduralSpriteGenerator.CreateWallTile(_tilePixelSize, _gameConstants.WallColor);
            _wallTile = ScriptableObject.CreateInstance<Tile>();
            _wallTile.sprite = wallSprite;
            _wallTile.color = Color.white;

            // Create floor tile (black background)
            Sprite floorSprite = ProceduralSpriteGenerator.CreateWallTile(_tilePixelSize, _gameConstants.BackgroundColor);
            _floorTile = ScriptableObject.CreateInstance<Tile>();
            _floorTile.sprite = floorSprite;
            _floorTile.color = Color.white;
        }

        /// <summary>
        /// Populates the tilemaps based on maze data
        /// </summary>
        private void PopulateTilemaps()
        {
            _mazeData.Reinitialize();

            for (int y = 0; y < _mazeData.Height; y++)
            {
                for (int x = 0; x < _mazeData.Width; x++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    
                    // Always place floor
                    _floorTilemap.SetTile(tilePos, _floorTile);

                    // Place wall if needed
                    if (_mazeData.IsWall(x, y))
                    {
                        _wallTilemap.SetTile(tilePos, _wallTile);
                    }
                }
            }

            // Refresh tilemaps
            _floorTilemap.RefreshAllTiles();
            _wallTilemap.RefreshAllTiles();
        }

        /// <summary>
        /// Sets up collision for walls
        /// </summary>
        private void SetupCollision()
        {
            // Add TilemapCollider2D
            var tilemapCollider = _wallTilemap.gameObject.GetComponent<TilemapCollider2D>();
            if (tilemapCollider == null)
            {
                tilemapCollider = _wallTilemap.gameObject.AddComponent<TilemapCollider2D>();
            }
            tilemapCollider.usedByComposite = true;

            // Add Rigidbody2D (static)
            var rb = _wallTilemap.gameObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = _wallTilemap.gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static;

            // Add CompositeCollider2D for optimized collision
            var compositeCollider = _wallTilemap.gameObject.GetComponent<CompositeCollider2D>();
            if (compositeCollider == null)
            {
                compositeCollider = _wallTilemap.gameObject.AddComponent<CompositeCollider2D>();
            }
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;

            // Set layer
            _wallTilemap.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        /// <summary>
        /// Gets world position for a tile coordinate
        /// </summary>
        public Vector3 GetWorldPosition(int x, int y)
        {
            return _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, 0.5f, 0);
        }

        /// <summary>
        /// Gets world position for a tile coordinate (float version for half-tiles)
        /// </summary>
        public Vector3 GetWorldPosition(float x, float y)
        {
            return new Vector3(x * _gameConstants.TileSize + 0.5f, y * _gameConstants.TileSize + 0.5f, 0);
        }

        /// <summary>
        /// Gets tile coordinate from world position
        /// </summary>
        public Vector2Int GetTilePosition(Vector3 worldPos)
        {
            Vector3Int cellPos = _grid.WorldToCell(worldPos);
            return new Vector2Int(cellPos.x, cellPos.y);
        }

        /// <summary>
        /// Checks if a position can be walked to
        /// </summary>
        public bool CanMoveTo(Vector2Int from, Vector2Int direction)
        {
            Vector2Int target = from + direction;
            
            // Handle tunnel wrap
            if (target.x < 0) target.x = _mazeData.Width - 1;
            else if (target.x >= _mazeData.Width) target.x = 0;

            return _mazeData.IsWalkable(target.x, target.y);
        }

        /// <summary>
        /// Gets valid movement directions from a position
        /// </summary>
        public Vector2Int[] GetValidDirections(Vector2Int position)
        {
            var directions = new System.Collections.Generic.List<Vector2Int>();
            
            Vector2Int[] allDirections = {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (var dir in allDirections)
            {
                if (CanMoveTo(position, dir))
                {
                    directions.Add(dir);
                }
            }

            return directions.ToArray();
        }

        /// <summary>
        /// Handles tunnel wrap for world position
        /// </summary>
        public Vector3 WrapPosition(Vector3 worldPos)
        {
            float leftBound = -0.5f;
            float rightBound = (_mazeData.Width - 0.5f) * _gameConstants.TileSize;

            if (worldPos.x < leftBound)
            {
                worldPos.x = rightBound - 0.1f;
            }
            else if (worldPos.x > rightBound)
            {
                worldPos.x = leftBound + 0.1f;
            }

            return worldPos;
        }

        /// <summary>
        /// Checks if position is in tunnel area
        /// </summary>
        public bool IsInTunnel(Vector3 worldPos)
        {
            Vector2Int tilePos = GetTilePosition(worldPos);
            return tilePos.x < 5 || tilePos.x >= _mazeData.Width - 5;
        }

        /// <summary>
        /// Clears the generated maze
        /// </summary>
        public void ClearMaze()
        {
            if (_wallTilemap != null) _wallTilemap.ClearAllTiles();
            if (_floorTilemap != null) _floorTilemap.ClearAllTiles();
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Maze")]
        private void EditorGenerateMaze()
        {
            GenerateMaze();
        }

        [ContextMenu("Clear Maze")]
        private void EditorClearMaze()
        {
            ClearMaze();
        }
#endif
    }
}
