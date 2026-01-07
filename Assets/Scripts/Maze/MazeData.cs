using UnityEngine;

namespace Pacman.Maze
{
    /// <summary>
    /// ScriptableObject containing the maze layout data.
    /// Defines wall positions, dot positions, and special zones.
    /// </summary>
    [CreateAssetMenu(fileName = "MazeData", menuName = "Pacman/Maze Data")]
    public class MazeData : ScriptableObject
    {
        [Header("=== MAZE DIMENSIONS ===")]
        public int Width = 28;
        public int Height = 31;

        [Header("=== MAZE LAYOUT ===")]
        [Tooltip("Maze layout string. Each character represents a cell type.")]
        [TextArea(35, 35)]
        public string MazeLayout = @"
############################
#............##............#
#.####.#####.##.#####.####.#
#O####.#####.##.#####.####O#
#.####.#####.##.#####.####.#
#..........................#
#.####.##.########.##.####.#
#.####.##.########.##.####.#
#......##....##....##......#
######.##### ## #####.######
     #.##### ## #####.#     
     #.##          ##.#     
     #.## ###--### ##.#     
######.## #      # ##.######
T     .   #      #   .     T
######.## #      # ##.######
     #.## ######## ##.#     
     #.##          ##.#     
     #.## ######## ##.#     
######.## ######## ##.######
#............##............#
#.####.#####.##.#####.####.#
#.####.#####.##.#####.####.#
#O..##.......  .......##..O#
###.##.##.########.##.##.###
###.##.##.########.##.##.###
#......##....##....##......#
#.##########.##.##########.#
#.##########.##.##########.#
#..........................#
############################";

        // Cell type definitions
        public const char WALL = '#';
        public const char DOT = '.';
        public const char POWER_PELLET = 'O';
        public const char EMPTY = ' ';
        public const char GHOST_HOUSE = '-';
        public const char TUNNEL = 'T';

        // Cached maze array
        private char[,] _mazeArray;
        private bool _isInitialized;

        /// <summary>
        /// Gets the cell type at the given position
        /// </summary>
        public char GetCell(int x, int y)
        {
            InitializeMazeArray();
            
            // Flip Y coordinate (maze string is top-to-bottom, Unity is bottom-to-top)
            int flippedY = Height - 1 - y;
            
            if (x < 0 || x >= Width || flippedY < 0 || flippedY >= Height)
                return WALL;
                
            return _mazeArray[x, flippedY];
        }

        /// <summary>
        /// Checks if a cell is walkable (not a wall)
        /// </summary>
        public bool IsWalkable(int x, int y)
        {
            char cell = GetCell(x, y);
            return cell != WALL;
        }

        /// <summary>
        /// Checks if a cell is a wall
        /// </summary>
        public bool IsWall(int x, int y)
        {
            return GetCell(x, y) == WALL;
        }

        /// <summary>
        /// Checks if a cell has a dot
        /// </summary>
        public bool HasDot(int x, int y)
        {
            return GetCell(x, y) == DOT;
        }

        /// <summary>
        /// Checks if a cell has a power pellet
        /// </summary>
        public bool HasPowerPellet(int x, int y)
        {
            return GetCell(x, y) == POWER_PELLET;
        }

        /// <summary>
        /// Checks if a cell is part of a tunnel
        /// </summary>
        public bool IsTunnel(int x, int y)
        {
            return GetCell(x, y) == TUNNEL;
        }

        /// <summary>
        /// Checks if a cell is part of the ghost house
        /// </summary>
        public bool IsGhostHouse(int x, int y)
        {
            return GetCell(x, y) == GHOST_HOUSE;
        }

        /// <summary>
        /// Gets the total number of dots (including power pellets)
        /// </summary>
        public int GetTotalDotCount()
        {
            InitializeMazeArray();
            int count = 0;
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    char cell = _mazeArray[x, y];
                    if (cell == DOT || cell == POWER_PELLET)
                        count++;
                }
            }
            
            return count;
        }

        /// <summary>
        /// Gets count of small dots only
        /// </summary>
        public int GetSmallDotCount()
        {
            InitializeMazeArray();
            int count = 0;
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (_mazeArray[x, y] == DOT)
                        count++;
                }
            }
            
            return count;
        }

        /// <summary>
        /// Gets count of power pellets
        /// </summary>
        public int GetPowerPelletCount()
        {
            InitializeMazeArray();
            int count = 0;
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (_mazeArray[x, y] == POWER_PELLET)
                        count++;
                }
            }
            
            return count;
        }

        /// <summary>
        /// Initialize the maze array from the string
        /// </summary>
        private void InitializeMazeArray()
        {
            if (_isInitialized && _mazeArray != null)
                return;

            _mazeArray = new char[Width, Height];
            
            // Clean up the layout string
            string cleanLayout = MazeLayout.Trim().Replace("\r\n", "\n");
            string[] rows = cleanLayout.Split('\n');

            for (int y = 0; y < Height && y < rows.Length; y++)
            {
                string row = rows[y];
                for (int x = 0; x < Width; x++)
                {
                    if (x < row.Length)
                        _mazeArray[x, y] = row[x];
                    else
                        _mazeArray[x, y] = EMPTY;
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Force re-initialization (useful after modifying layout in editor)
        /// </summary>
        public void Reinitialize()
        {
            _isInitialized = false;
            _mazeArray = null;
            InitializeMazeArray();
        }

        private void OnValidate()
        {
            _isInitialized = false;
        }

        /// <summary>
        /// Gets all positions of a specific cell type
        /// </summary>
        public System.Collections.Generic.List<Vector2Int> GetPositionsOfType(char cellType)
        {
            InitializeMazeArray();
            var positions = new System.Collections.Generic.List<Vector2Int>();
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Convert from storage coordinates to Unity coordinates
                    int unityY = Height - 1 - y;
                    if (_mazeArray[x, y] == cellType)
                    {
                        positions.Add(new Vector2Int(x, unityY));
                    }
                }
            }
            
            return positions;
        }

        /// <summary>
        /// Check if position is in ghost house area (for pathfinding exclusion)
        /// </summary>
        public bool IsInGhostHouseArea(int x, int y)
        {
            // Ghost house is roughly at center of maze
            // X: 10-17, Y: 12-16 (in Unity coordinates, Y is flipped)
            int ghostHouseLeft = 10;
            int ghostHouseRight = 17;
            int ghostHouseBottom = 13;
            int ghostHouseTop = 16;
            
            return x >= ghostHouseLeft && x <= ghostHouseRight && 
                   y >= ghostHouseBottom && y <= ghostHouseTop;
        }

        /// <summary>
        /// Gets the ghost house door position
        /// </summary>
        public Vector2Int GetGhostHouseDoorPosition()
        {
            return new Vector2Int(13, 17); // Above the ghost house
        }

        /// <summary>
        /// Gets tunnel exit positions
        /// </summary>
        public Vector2Int[] GetTunnelExits()
        {
            return new Vector2Int[]
            {
                new Vector2Int(0, 14),  // Left tunnel
                new Vector2Int(27, 14)  // Right tunnel
            };
        }
    }
}
