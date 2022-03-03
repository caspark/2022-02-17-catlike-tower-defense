using UnityEngine;

public class TargetPoint : MonoBehaviour {

    public Enemy Enemy { get; private set; }

    public Vector3 Position => transform.position;

    private void Awake() {
        Enemy = transform.root.GetComponent<Enemy>();
        Debug.Assert(Enemy != null, "TargetPoint is not child of Enemy!", this);
        Debug.Assert(GetComponent<SphereCollider>() != null, "TargetPoint needs a sphere collider!", this);
        Debug.Assert(gameObject.layer == 9, "TargetPoint should be in layer 9!", this);
    }
}
