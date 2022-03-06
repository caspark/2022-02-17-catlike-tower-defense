using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField] private GameBoard board = default;

    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private WarFactory warFactory = default;
    [SerializeField] private GameScenario scenario = default;
    GameScenario.State activeScenario;

    static Game instance;

    Ray TouchRay => Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    GameBehaviorCollection enemies = new GameBehaviorCollection();
    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    TowerType selectedTowerType;

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

        if (keyboard.digit1Key.wasPressedThisFrame) {
            selectedTowerType = TowerType.Laser;
            Debug.Log("Selected tower type: " + selectedTowerType);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame) {
            selectedTowerType = TowerType.Mortar;
            Debug.Log("Selected tower type: " + selectedTowerType);
        }
        if (keyboard.bKey.wasPressedThisFrame) {
            BeginNewGame();
        }

        activeScenario.Progress();

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }

    public static void SpawnEnemy(EnemyFactory factory, EnemyType type) {
        GameTile spawnPoint = instance.board.GetSpawnPoint(
            Random.Range(0, instance.board.SpawnPointCount)
        );
        Enemy enemy = factory.Get(type);
        enemy.spawnOn(spawnPoint);
        instance.enemies.Add(enemy);
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

    void BeginNewGame() {
        Debug.Log("Beginning new game");
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
        activeScenario = scenario.Begin();
    }
}
