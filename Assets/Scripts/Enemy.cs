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

    internal void spawnOn(GameTile tile) {
        transform.localPosition = tile.transform.localPosition;
    }
}
