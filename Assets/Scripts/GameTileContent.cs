using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class GameTileContent : MonoBehaviour {
    [SerializeField] private GameTileContentType type = default;

    public GameTileContentType Type => type;

    public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

    GameTileContentFactory originFactory;

    public GameTileContentFactory OriginFactory {
        get => originFactory;
        set {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public void Recycle() {
        originFactory.Reclaim(this);
    }

    public virtual void GameUpdate() { }
}
