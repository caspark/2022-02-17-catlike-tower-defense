using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {
    [SerializeField] private Transform ground = default;

    [SerializeField] private GameTile tilePrefab = default;

    Vector2Int size;

    GameTile[] tiles;

    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    public void Initialize(Vector2Int size) {
        this.size = size;
        ground.localScale = new Vector3(size.x, size.y, 1);

        Vector2 offset = new Vector2((size.x - 1) * .5f, (size.y - 1) * .5f);
        tiles = new GameTile[size.x * size.y];
        int i = 0;
        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {
                GameTile tile = Instantiate(tilePrefab, ground);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0, y - offset.y);

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

        FindPaths();
    }

    private void FindPaths() {
        foreach (GameTile tile in tiles) {
            tile.ClearPath();
        }
        tiles[0].BecomeDestination();
        searchFrontier.Enqueue(tiles[0]);

        while (searchFrontier.Count > 0) {
            GameTile tile = searchFrontier.Dequeue();
            if (tile != null) {
                searchFrontier.Enqueue(tile.GrowPathNorth());
                searchFrontier.Enqueue(tile.GrowPathEast());
                searchFrontier.Enqueue(tile.GrowPathSouth());
                searchFrontier.Enqueue(tile.GrowPathWest());
            }
        }

        foreach (GameTile tile in tiles) {
            tile.ShowPath();
        }
    }


}
