using UnityEngine;
using Pacman.Maze;
using Pacman.Player;
using Pacman.Collectibles;
using Pacman.Ghosts;
using Pacman.UI;
using Pacman.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pacman.Core
{
    /// <summary>
    /// Main game bootstrapper that sets up all game systems.
    /// Auto-creates ScriptableObjects if not assigned.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("=== CONFIGURATION ===")]
        [SerializeField] private GameConstants _gameConstants;
        [SerializeField] private MazeData _mazeData;

        [Header("=== MANAGERS (Auto-created if null) ===")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private MazeGenerator _mazeGenerator;
        [SerializeField] private DotManager _dotManager;
        [SerializeField] private GhostManager _ghostManager;
        [SerializeField] private UIManager _uiManager;

        [Header("=== CHARACTERS (Auto-created if null) ===")]
        [SerializeField] private PacmanController _pacman;
        [SerializeField] private Blinky _blinky;
        [SerializeField] private Pinky _pinky;
        [SerializeField] private Inky _inky;
        [SerializeField] private Clyde _clyde;

        private void Awake()
        {
            // Auto-create ScriptableObjects if not assigned
            EnsureScriptableObjects();

            // Setup Physics2D layers
            SetupPhysicsLayers();

            // Create managers
            CreateManagers();

            // Create characters
            CreateCharacters();

            // Wire up references
            WireReferences();
        }

        /// <summary>
        /// Ensures ScriptableObjects exist, creates them if not
        /// </summary>
        private void EnsureScriptableObjects()
        {
            // Create GameConstants if not assigned
            if (_gameConstants == null)
            {
                _gameConstants = ScriptableObject.CreateInstance<GameConstants>();
                Debug.Log("GameBootstrapper: Created GameConstants at runtime");
                
#if UNITY_EDITOR
                // Try to save as asset in editor
                if (!Application.isPlaying)
                {
                    EnsureFolderExists("Assets/ScriptableObjects/GameConfig");
                    AssetDatabase.CreateAsset(_gameConstants, "Assets/ScriptableObjects/GameConfig/DefaultGameConstants.asset");
                    AssetDatabase.SaveAssets();
                }
#endif
            }

            // Create MazeData if not assigned
            if (_mazeData == null)
            {
                _mazeData = ScriptableObject.CreateInstance<MazeData>();
                Debug.Log("GameBootstrapper: Created MazeData at runtime");
                
#if UNITY_EDITOR
                // Try to save as asset in editor
                if (!Application.isPlaying)
                {
                    EnsureFolderExists("Assets/ScriptableObjects/MazeData");
                    AssetDatabase.CreateAsset(_mazeData, "Assets/ScriptableObjects/MazeData/ClassicMaze.asset");
                    AssetDatabase.SaveAssets();
                }
#endif
            }
        }

#if UNITY_EDITOR
        private void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
#endif

        /// <summary>
        /// Sets up Physics2D collision layers
        /// </summary>
        private void SetupPhysicsLayers()
        {
            // Layer setup is done in Project Settings
            // This is just a reminder of expected layers
        }

        /// <summary>
        /// Creates all manager objects
        /// </summary>
        private void CreateManagers()
        {
            // Create MazeGenerator
            if (_mazeGenerator == null)
            {
                GameObject mazeObj = new GameObject("MazeGenerator");
                mazeObj.transform.SetParent(transform);
                _mazeGenerator = mazeObj.AddComponent<MazeGenerator>();
            }

            // Create DotManager
            if (_dotManager == null)
            {
                GameObject dotObj = new GameObject("DotManager");
                dotObj.transform.SetParent(transform);
                _dotManager = dotObj.AddComponent<DotManager>();
            }

            // Create GhostManager
            if (_ghostManager == null)
            {
                GameObject ghostMgrObj = new GameObject("GhostManager");
                ghostMgrObj.transform.SetParent(transform);
                _ghostManager = ghostMgrObj.AddComponent<GhostManager>();
            }

            // Create UIManager
            if (_uiManager == null)
            {
                GameObject uiObj = new GameObject("UIManager");
                uiObj.transform.SetParent(transform);
                _uiManager = uiObj.AddComponent<UIManager>();
            }

            // Create GameManager (should be last as it depends on others)
            if (_gameManager == null)
            {
                GameObject gameObj = new GameObject("GameManager");
                gameObj.transform.SetParent(transform);
                _gameManager = gameObj.AddComponent<GameManager>();
            }
        }

        /// <summary>
        /// Creates all character objects
        /// NOTE: Add SpriteRenderer BEFORE the controller so Awake() can find it
        /// </summary>
        private void CreateCharacters()
        {
            // Create Pacman (SpriteRenderer FIRST, then Controller)
            if (_pacman == null)
            {
                GameObject pacmanObj = new GameObject("Pacman");
                pacmanObj.AddComponent<SpriteRenderer>();
                pacmanObj.AddComponent<Rigidbody2D>();
                pacmanObj.AddComponent<CircleCollider2D>();
                _pacman = pacmanObj.AddComponent<PacmanController>();
            }

            // Create Blinky (Red)
            if (_blinky == null)
            {
                GameObject blinkyObj = new GameObject("Blinky");
                blinkyObj.AddComponent<SpriteRenderer>();
                blinkyObj.AddComponent<Rigidbody2D>();
                blinkyObj.AddComponent<CircleCollider2D>();
                _blinky = blinkyObj.AddComponent<Blinky>();
            }

            // Create Pinky (Pink)
            if (_pinky == null)
            {
                GameObject pinkyObj = new GameObject("Pinky");
                pinkyObj.AddComponent<SpriteRenderer>();
                pinkyObj.AddComponent<Rigidbody2D>();
                pinkyObj.AddComponent<CircleCollider2D>();
                _pinky = pinkyObj.AddComponent<Pinky>();
            }

            // Create Inky (Cyan)
            if (_inky == null)
            {
                GameObject inkyObj = new GameObject("Inky");
                inkyObj.AddComponent<SpriteRenderer>();
                inkyObj.AddComponent<Rigidbody2D>();
                inkyObj.AddComponent<CircleCollider2D>();
                _inky = inkyObj.AddComponent<Inky>();
            }

            // Create Clyde (Orange)
            if (_clyde == null)
            {
                GameObject clydeObj = new GameObject("Clyde");
                clydeObj.AddComponent<SpriteRenderer>();
                clydeObj.AddComponent<Rigidbody2D>();
                clydeObj.AddComponent<CircleCollider2D>();
                _clyde = clydeObj.AddComponent<Clyde>();
            }
        }

        /// <summary>
        /// Wires up all references between components
        /// </summary>
        private void WireReferences()
        {
            // MazeGenerator
            SetPrivateField(_mazeGenerator, "_gameConstants", _gameConstants);
            SetPrivateField(_mazeGenerator, "_mazeData", _mazeData);

            // DotManager
            SetPrivateField(_dotManager, "_gameConstants", _gameConstants);
            SetPrivateField(_dotManager, "_mazeGenerator", _mazeGenerator);

            // GameManager
            SetPrivateField(_gameManager, "_gameConstants", _gameConstants);
            SetPrivateField(_gameManager, "_mazeGenerator", _mazeGenerator);
            SetPrivateField(_gameManager, "_dotManager", _dotManager);
            SetPrivateField(_gameManager, "_pacman", _pacman);
            SetPrivateField(_gameManager, "_ghostManager", _ghostManager);

            // UIManager
            SetPrivateField(_uiManager, "_gameConstants", _gameConstants);

            // Pacman
            SetPrivateField(_pacman, "_gameConstants", _gameConstants);
            SetPrivateField(_pacman, "_mazeGenerator", _mazeGenerator);

            // GhostManager
            SetPrivateField(_ghostManager, "_gameConstants", _gameConstants);
            SetPrivateField(_ghostManager, "_mazeGenerator", _mazeGenerator);
            SetPrivateField(_ghostManager, "_blinky", _blinky);
            SetPrivateField(_ghostManager, "_pinky", _pinky);
            SetPrivateField(_ghostManager, "_inky", _inky);
            SetPrivateField(_ghostManager, "_clyde", _clyde);

            // All Ghosts
            GhostBase[] ghosts = { _blinky, _pinky, _inky, _clyde };
            foreach (var ghost in ghosts)
            {
                SetPrivateField(ghost, "_gameConstants", _gameConstants);
                SetPrivateField(ghost, "_mazeGenerator", _mazeGenerator);
                SetPrivateField(ghost, "_pacman", _pacman);
            }

            // Inky needs Blinky reference
            _inky.SetBlinkyReference(_blinky);
        }

        /// <summary>
        /// Helper to set private serialized fields via reflection
        /// </summary>
        private void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                // Try base class
                var baseType = target.GetType().BaseType;
                while (baseType != null)
                {
                    field = baseType.GetField(fieldName,
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(target, value);
                        return;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Force Setup Scene")]
        private void EditorSetupScene()
        {
            EnsureScriptableObjects();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
