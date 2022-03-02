using System;
using UnityEngine;

public class Enemy : MonoBehaviour {

    private EnemyFactory originFactory;

    public EnemyFactory OriginFactory {
        get => originFactory;
        set {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    private GameTile tileFrom, tileTo;
    private Vector3 positionFrom, positionTo;
    private float progress;

    public bool GameUpdate() {
        progress += Time.deltaTime;
        while (progress >= 1f) {
            tileFrom = tileTo;
            tileTo = tileTo.NextTileOnPath;
            if (tileTo == null) {
                OriginFactory.Reclaim(this);
                return false;
            }
            positionFrom = positionTo;
            positionTo = tileTo.ExitPoint;
            transform.localRotation = tileFrom.PathDirection.GetRotation();
            progress -= 1f;
        }
        transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        return true;
    }

    internal void spawnOn(GameTile tile) {
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        positionFrom = tileFrom.transform.localPosition;
        positionTo = tileTo.ExitPoint;
        transform.localRotation = tileFrom.PathDirection.GetRotation();
        progress = 0f;
    }
}
