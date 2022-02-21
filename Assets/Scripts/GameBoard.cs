using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {
    [SerializeField] private Transform ground = default;

    [SerializeField] private GameTile tilePrefab = default;

    GameTileContentFactory contentFactory;

    Vector2Int size;

    GameTile[] tiles;

    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory) {
        this.size = size;
        this.contentFactory = contentFactory;
        ground.localScale = new Vector3(size.x, size.y, 1);

        Vector2 offset = new Vector2((size.x - 1) * .5f, (size.y - 1) * .5f);
        tiles = new GameTile[size.x * size.y];
        int i = 0;
        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {
                GameTile tile = Instantiate(tilePrefab, ground);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0, y - offset.y);
                tile.IsAlternative = (i & 1) == 0;
                tile.Content = contentFactory.Get(GameTileContentType.Empty);

                if (x > 0) {
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }
                if (y > 0) {
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                }

                tiles[i] = tile;
                i++;
            }
        }

        ToggleDestination(tiles[tiles.Length / 2]);
    }

    private bool FindPaths() {
        foreach (GameTile tile in tiles) {
            if (tile.Content.Type == GameTileContentType.Destination) {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
            }
            else {
                tile.ClearPath();
            }
        }
        if (searchFrontier.Count == 0) {
            return false;
        }

        while (searchFrontier.Count > 0) {
            GameTile tile = searchFrontier.Dequeue();
            if (tile != null) {
                if (tile.IsAlternative) {
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathWest());
                }
                else {
                    searchFrontier.Enqueue(tile.GrowPathWest());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                }
            }
        }

        foreach (GameTile tile in tiles) {
            tile.ShowPath();
        }

        return true;
    }

    public void ToggleDestination(GameTile tile) {
        if (tile.Content.Type == GameTileContentType.Destination) {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths()) {
                tile.Content = contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public GameTile GetTile(Ray ray) {
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);
            if (x >= 0 && x < size.x && y >= 0 && y < size.y) {
                return tiles[x + y * size.x];
            }
        }
        return null;
    }

}
