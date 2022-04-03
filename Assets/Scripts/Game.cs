using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
    enum GameState {
        Setup,
        Playing,
        GameOver,
    }

    enum BuildTool {
        // things that should be shown in the UI
        Wall, LaserTower, MortarTower,
        // hidden things that can be built
        SpawnPoint, DestinationPoint,
    }

    public delegate void OnGameOver();

    public event OnGameOver GameOverHandler;

    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField] private GameBoard board = default;

    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private WarFactory warFactory = default;
    [SerializeField] public GameScenario scenario = default;
    [SerializeField, Range(0, 100)] private int startingPlayerHealth = 10;
    [SerializeField] private float playSpeed = 1f;
    [SerializeField] private UIDocument uiDocument = default;

    const float pausedTimeScale = 0.01f;

    static Game instance;
    GameScenario.State activeScenario;
    Ray TouchRay => Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    GameBehaviorCollection enemies = new GameBehaviorCollection();
    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    [ShowInInspector] BuildTool selectedBuildTool;
    [ShowInInspector] int playerHealth;
    private AudioSource audioSource;
    VisualElement uiTowerSelectContainer;
    private int killCount;

    [SerializeField]
    private AudioClip loseLife;

    [SerializeField]
    private AudioClip[] enemyDeath;

    [SerializeField, Required]
    private ParticleSystem victoryParticles = default;

    [SerializeField, Required]
    private AudioClip victorySound = default;

    [SerializeField, Required]
    private GameObject defeatModel = default;

    [SerializeField, Required]
    private AudioClip defeatSound = default;

    GameState gameState = GameState.Setup;

    Label gameOverLabel = default;

    Label GameOverLabel {
        get {
            if (gameOverLabel == null) {
                gameOverLabel = uiDocument.rootVisualElement.Q<Label>("GameOver");
                Debug.Assert(gameOverLabel != null, "GameOver label not found!");
            }
            return gameOverLabel;
        }
    }

    public static Shell SpawnShell() {
        Shell shell = instance.warFactory.Shell;
        instance.nonEnemies.Add(shell);
        return shell;
    }

    public static Explosion SpawnExplosion() {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    private void OnEnable() {
        instance = this;
    }

    private void OnValidate() {
        if (boardSize.x < 2) {
            boardSize.x = 2;
        }
        if (boardSize.y < 2) {
            boardSize.y = 2;
        }
    }

    private void Awake() {
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;

        audioSource = GetComponent<AudioSource>();
        Debug.Assert(audioSource != null, "No audio source found!");

        // Init UI
        Debug.Assert(uiDocument != null, "UI Document must be set!");
        VisualElement uiRoot = uiDocument.rootVisualElement;
        uiRoot.Query<Button>(className: "buildButton")
            .Build()
            .ForEachWithIndex((button, i) => {
                button.clickable.clicked += () => {
                    SelectBuildTool((BuildTool)i);
                };
            });
        uiTowerSelectContainer = uiRoot.Q<VisualElement>("TowerSelectContainer");
        Label kills = uiRoot.Query<Label>("Kills");
        kills.RegisterCallback<TransitionEndEvent>(e => {
            kills.RemoveFromClassList("kill-inc-in");
            kills.AddToClassList("kill-inc-out");
        });
        Label lives = uiRoot.Query<Label>("Lives");
        lives.RegisterCallback<TransitionEndEvent>(e => {
            lives.RemoveFromClassList("life-inc-in");
            lives.AddToClassList("life-inc-out");
        });

        // Init UI state
        SelectBuildTool(BuildTool.Wall);
    }

    public void TearDownGame() {
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
    }

    void BeginNewGame() {
        Debug.Log("Beginning new game");
        TearDownGame();

        board.ToggleDestination(board.GetTile(boardSize.x * boardSize.y / 2));
        board.ToggleSpawnPoint(board.GetTile(0));
        board.ToggleTower(board.GetTile(boardSize.x * 2 + 3), TowerType.Mortar);
        board.ToggleTower(board.GetTile(boardSize.x * 2 + 4), TowerType.Laser);

        activeScenario = scenario.Begin();
        playerHealth = startingPlayerHealth;
        killCount = 0;

        UpdateAllUI();

        gameState = GameState.Playing;
    }

    private void Update() {
        if (gameState == GameState.Setup) {
            BeginNewGame();
        }
        else if (gameState == GameState.GameOver) {
            // wait for gameover state to be cleared
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) {
            Debug.Log("Mouse not detected!");
            return;
        }
        if (mouse.leftButton.wasPressedThisFrame) {
            HandleTouch();
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) {
            Debug.Log("Keyboard not detected!");
            return;
        }

        if (keyboard.pKey.wasPressedThisFrame) {
            board.ShowPaths = !board.ShowPaths;
        }
        else if (keyboard.gKey.wasPressedThisFrame) {
            board.ShowGrid = !board.ShowGrid;
        }

        if (keyboard.spaceKey.wasPressedThisFrame) {
            Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : 1f;
        }
        else if (Time.timeScale > pausedTimeScale) {
            Time.timeScale = playSpeed;
        }

        if (keyboard.digit1Key.wasPressedThisFrame) {
            SelectBuildTool(BuildTool.Wall);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame) {
            SelectBuildTool(BuildTool.LaserTower);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame) {
            SelectBuildTool(BuildTool.MortarTower);
        }
        else if (keyboard.digit4Key.wasPressedThisFrame) {
            SelectBuildTool(BuildTool.SpawnPoint);
        }
        else if (keyboard.digit5Key.wasPressedThisFrame) {
            SelectBuildTool(BuildTool.DestinationPoint);
        }

        if (keyboard.bKey.wasPressedThisFrame) {
            BeginNewGame();
        }

        if (playerHealth <= 0 && startingPlayerHealth > 0) {
            gameState = GameState.GameOver;
            StartCoroutine(HandleDefeat());
            return;
        }

        if (!activeScenario.Progress() && enemies.isEmpty) {
            gameState = GameState.GameOver;
            StartCoroutine(HandleVictory());
            return;
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }

    private IEnumerator HandleDefeat() {
        Debug.Log("Defeat!");
        audioSource.PlayOneShot(defeatSound);
        defeatModel.SetActive(true);
        defeatModel.GetComponent<Animator>().SetTrigger("DoDance");
        GameOverLabel.text = "Defeat :(";
        GameOverLabel.RemoveFromClassList("game-over-out");
        GameOverLabel.AddToClassList("game-over-in");

        yield return new WaitForSecondsRealtime(6);

        audioSource.Stop();
        defeatModel.SetActive(false);

        EmitGameOverAndPossiblyRestart();
    }


    private IEnumerator HandleVictory() {
        Debug.Log("Victory!");
        audioSource.PlayOneShot(victorySound);
        victoryParticles.Play();
        GameOverLabel.text = "Victory!!";
        GameOverLabel.RemoveFromClassList("game-over-out");
        GameOverLabel.AddToClassList("game-over-in");

        yield return new WaitForSecondsRealtime(5);

        audioSource.Stop();
        victoryParticles.Stop();

        EmitGameOverAndPossiblyRestart();
    }

    private void EmitGameOverAndPossiblyRestart() {
        if (GameOverHandler == null) {
            BeginNewGame();
        }
        else {
            GameOverHandler();
        }
    }

    private void SelectBuildTool(BuildTool type) {
        selectedBuildTool = type;
        Debug.Log("Selected build tool: " + selectedBuildTool);

        uiTowerSelectContainer.Query<Button>().Build().ForEachWithIndex((child, i) => {
            if (i == (int)type) {
                child.AddToClassList("selected");
            }
            else {
                child.RemoveFromClassList("selected");
            }
        });
    }

    public static void SpawnEnemy(EnemyFactory factory, EnemyType type) {
        GameTile spawnPoint = instance.board.GetSpawnPoint(
            Random.Range(0, instance.board.SpawnPointCount)
        );
        Enemy enemy = factory.Get(type);
        enemy.spawnOn(spawnPoint);
        instance.enemies.Add(enemy);

        instance.UpdateWaveUI();
    }

    public static void EnemyDied(Enemy enemy) {
        Debug.Log("Enemy died!", enemy);
        instance.killCount += 1;
        instance.UpdateKillsUI();

        Label kills = instance.uiDocument.rootVisualElement.Q<Label>("Kills");
        kills.RemoveFromClassList("kill-inc-out");
        kills.AddToClassList("kill-inc-in");

        instance.audioSource.PlayOneShot(instance.enemyDeath[Random.Range(0, instance.enemyDeath.Length)]);
    }

    public static void EnemyReachedDestination() {
        instance.playerHealth -= 1;
        instance.UpdatePlayerHealthUI();

        Label lives = instance.uiDocument.rootVisualElement.Q<Label>("Lives");
        lives.RemoveFromClassList("life-inc-out");
        lives.AddToClassList("life-inc-in");

        instance.audioSource.PlayOneShot(instance.loseLife);
    }

    private void UpdateAllUI() {
        UpdatePlayerHealthUI();
        UpdateWaveUI();
        UpdateKillsUI();

        GameOverLabel.RemoveFromClassList("game-over-in");
        GameOverLabel.AddToClassList("game-over-out");
    }

    private void UpdatePlayerHealthUI() {
        uiDocument.rootVisualElement.Q<Label>("Lives").text = $"{playerHealth} / {startingPlayerHealth} lives";
    }

    private void UpdateWaveUI() {
        uiDocument.rootVisualElement.Q<Label>("Wave").text = activeScenario.GetProgressString();
    }

    private void UpdateKillsUI() {
        uiDocument.rootVisualElement.Q<Label>("Kills").text = killCount != 1 ? $"{killCount} kills" : "1 kill";
    }

    private void HandleTouch() {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null) {
            if (Keyboard.current.leftShiftKey.isPressed) {
                board.ToggleSpawnPoint(tile);
            }
            else {
                if (selectedBuildTool == BuildTool.Wall) {
                    board.ToggleWall(tile);
                }
                else if (selectedBuildTool == BuildTool.LaserTower) {
                    board.ToggleTower(tile, TowerType.Laser);
                }
                else if (selectedBuildTool == BuildTool.MortarTower) {
                    board.ToggleTower(tile, TowerType.Mortar);
                }
                else if (selectedBuildTool == BuildTool.SpawnPoint) {
                    board.ToggleSpawnPoint(tile);
                }
                else if (selectedBuildTool == BuildTool.DestinationPoint) {
                    board.ToggleDestination(tile);
                }
            }
        }
    }

}
