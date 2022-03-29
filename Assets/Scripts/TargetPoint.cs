using UnityEngine;

public class TargetPoint : MonoBehaviour {
    const int enemyLayerMask = 1 << 9;

    static Collider[] buffer = new Collider[100];

    public static int BufferedCount { get; private set; }

    public static bool FillBuffer(Vector3 position, float range) {
        Vector3 top = position;
        top.y += 3f;
        BufferedCount = Physics.OverlapCapsuleNonAlloc(position, top, range, buffer, enemyLayerMask);
        return BufferedCount > 0;
    }

    public static TargetPoint GetBuffered(int index) {
        var target = buffer[index].GetComponent<TargetPoint>();
        Debug.Assert(target != null, "Targeted non-enemy!", buffer[0]);
        return target;
    }

    public static TargetPoint RandomBuffered => GetBuffered(Random.Range(0, BufferedCount));

    public Enemy Enemy { get; private set; }

    public Vector3 Position => transform.position;

    private void Awake() {
        Enemy = transform.root.GetComponent<Enemy>();
        Debug.Assert(Enemy != null, "TargetPoint is not child of Enemy!", this);
        Debug.Assert(GetComponent<SphereCollider>() != null, "TargetPoint needs a sphere collider!", this);
        Debug.Assert(gameObject.layer == 9, "TargetPoint should be in layer 9!", this);

        Enemy.TargetPointCollider = GetComponent<Collider>();
    }
}
