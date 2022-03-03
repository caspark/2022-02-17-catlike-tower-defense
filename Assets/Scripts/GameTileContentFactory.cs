using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "GameTileContentFactory", menuName = "Catlike TD/GameTileContentFactory", order = 0)]
public class GameTileContentFactory : GameObjectFactory {
    [SerializeField] private GameTileContent destinationPrefab = default;
    [SerializeField] private GameTileContent emptyPrefab = default;
    [SerializeField] private GameTileContent wallPrefab = default;
    [SerializeField] private GameTileContent spawnPointPrefab = default;
    [SerializeField] private Tower towerPrefab = default;

    public void Reclaim(GameTileContent content) {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }

    public GameTileContent Get(GameTileContentType type) {
        switch (type) {
            case GameTileContentType.Empty: return Get(emptyPrefab);
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.Wall: return Get(wallPrefab);
            case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
            case GameTileContentType.Tower: return Get(towerPrefab);
        }
        Debug.Assert(false, $"Unsupported type: {type}");
        return null;
    }

    private GameTileContent Get(GameTileContent prefab) {
        GameTileContent instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }
}
