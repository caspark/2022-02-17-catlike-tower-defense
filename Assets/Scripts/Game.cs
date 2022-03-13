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
    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField] private GameBoard board = default;

    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private WarFactory warFactory = default;
    [SerializeField] private GameScenario scenario = default;
    [SerializeField, Range(0, 100)] private int startingPlayerHealth = 10;
    [SerializeField] private float playSpeed = 1f;
    [SerializeField] private UIDocument uiDocument = default;

    const float pausedTimeScale = 0.01f;

    static Game instance;
    GameScenario.State activeScenario;
    Ray TouchRay => Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    GameBehaviorCollection enemies = new GameBehaviorCollection();
    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    [ShowInInspector] TowerType selectedTowerType;
    [ShowInInspector] int playerHealth;

    VisualElement uiTowerSelectContainer;
    private int killCount;

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
        activeScenario = scenario.Begin();
        playerHealth = startingPlayerHealth;

        // Init UI
        Debug.Assert(uiDocument != null, "UI Document must be set!");
        VisualElement uiRoot = uiDocument.rootVisualElement;
        uiRoot.Query<Button>(className: "buildButton")
            .Build()
            .ForEachWithIndex((button, i) => {
                button.clickable.clicked += () => {
                    SelectTowerType((TowerType)i);
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
        UpdateAllUI();
        SelectTowerType(TowerType.Laser);
    }

    void BeginNewGame() {
        Debug.Log("Beginning new game");
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
        activeScenario = scenario.Begin();
        playerHealth = startingPlayerHealth;
        killCount = 0;

        UpdateAllUI();
    }

    private void Update() {
        Mouse mouse = Mouse.current;
        if (mouse == null) {
            Debug.Log("Mouse not detected!");
            return;
        }
        if (mouse.leftButton.wasPressedThisFrame) {
            HandleTouch();
        }
        else if (mouse.rightButton.wasPressedThisFrame) {
            HandleAlternativeTouch();
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
            SelectTowerType(TowerType.Laser);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame) {
            SelectTowerType(TowerType.Mortar);
        }

        if (keyboard.bKey.wasPressedThisFrame) {
            BeginNewGame();
        }

        if (playerHealth <= 0 && startingPlayerHealth > 0) {
            Debug.Log("Defeat!");
            BeginNewGame();
        }

        if (!activeScenario.Progress() && enemies.isEmpty) {
            Debug.Log("Victory!");
            BeginNewGame();
            activeScenario.Progress();
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }

    private void SelectTowerType(TowerType type) {
        selectedTowerType = type;
        Debug.Log("Selected tower type: " + selectedTowerType);

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
    }

    public static void EnemyReachedDestination() {
        instance.playerHealth -= 1;
        instance.UpdatePlayerHealthUI();

        Label lives = instance.uiDocument.rootVisualElement.Q<Label>("Lives");
        lives.RemoveFromClassList("life-inc-out");
        lives.AddToClassList("life-inc-in");
    }

    private void UpdateAllUI() {
        UpdatePlayerHealthUI();
        UpdateWaveUI();
        UpdateKillsUI();
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
                board.ToggleTower(tile, selectedTowerType);
            }
            else {
                board.ToggleWall(tile);
            }
        }
    }

    private void HandleAlternativeTouch() {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null) {
            if (Keyboard.current.leftShiftKey.isPressed) {
                board.ToggleDestination(tile);
            }
            else {
                board.ToggleSpawnPoint(tile);
            }
        }
    }

}
