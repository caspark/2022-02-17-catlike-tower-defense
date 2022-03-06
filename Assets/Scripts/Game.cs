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
    [SerializeField] private EnemyFactory enemyFactory = default;
    [SerializeField, Range(0.0f, 10f)] private float spawnSpeed = 1f;

    static Game instance;

    Ray TouchRay => Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    float spawnProgress;
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
        else if (keyboard.eKey.wasPressedThisFrame) {
            spawnProgress += 1;
        }

        if (keyboard.digit1Key.wasPressedThisFrame) {
            selectedTowerType = TowerType.Laser;
            Debug.Log("Selected tower type: " + selectedTowerType);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame) {
            selectedTowerType = TowerType.Mortar;
            Debug.Log("Selected tower type: " + selectedTowerType);
        }

        spawnProgress += spawnSpeed * Time.deltaTime;
        while (spawnProgress >= 1f) {
            spawnProgress -= 1f;
            SpawnEnemy();
        }
        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }

    private void SpawnEnemy() {
        GameTile spawnPoint = board.GetSpawnPoint(Random.Range(0, board.SpawnPointCount));
        Enemy enemy = enemyFactory.Get((EnemyType)Random.Range(0, 3));
        enemies.Add(enemy);
        enemy.spawnOn(spawnPoint);
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
