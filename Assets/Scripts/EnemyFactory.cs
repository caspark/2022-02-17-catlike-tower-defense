using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory {

    [SerializeField] private Enemy prefab = default;

    public Enemy Get() {
        Enemy instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(Enemy enemy) {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }
}
