using System;
using UnityEngine;

public class Tower : GameTileContent {
    [SerializeField, Range(1.5f, 10.5f)]
    float targetingRange = 1.5f;

    [SerializeField]
    Transform turret = default, laserBeam = default;

    TargetPoint target;

    const int enemyLayerMask = 1 << 9;

    static Collider[] targetsBuffer = new Collider[1];

    Vector3 laserBeamScale;

    private void Awake() {
        laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate() {
        if (TrackTarget() || AcquireTarget()) {
            Shoot();
        }
        else {
            laserBeam.localScale = Vector3.zero;
        }
    }

    bool TrackTarget() {
        if (target == null) {
            return false;
        }
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float y = a.z - b.z;
        float r = targetingRange + 0.125f * target.Enemy.Scale;
        if (x * x + y * y > r * r) {
            target = null;
            return false;
        }
        return true;
    }

    private bool AcquireTarget() {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 3f;
        int hits = Physics.OverlapCapsuleNonAlloc(a, b, targetingRange, targetsBuffer, enemyLayerMask);
        if (hits > 0) {
            target = targetsBuffer[0].GetComponent<TargetPoint>();
            Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
            return true;
        }
        target = null;
        return false;
    }

    private void Shoot() {
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f; // make sure it's clearly above the ground
        Gizmos.DrawWireSphere(position, targetingRange);
        if (target != null) {
            Gizmos.DrawLine(position, target.Position);
        }
    }
}
