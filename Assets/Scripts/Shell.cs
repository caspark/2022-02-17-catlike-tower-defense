using UnityEngine;

public class Shell : WarEntity {
    private Vector3 launchPoint;
    private Vector3 targetPoint;
    private Vector3 launchVelocity;
    private float age;
    private float blastRadius;
    private float damage;

    public void Initialize(Vector3 launchPoint, Vector3 targetPoint, Vector3 launchVelocity, float blastRadius, float damage) {
        this.launchPoint = launchPoint;
        this.targetPoint = targetPoint;
        this.launchVelocity = launchVelocity;
        this.blastRadius = blastRadius;
        this.damage = damage;
    }

    public override bool GameUpdate() {
        age += Time.deltaTime;
        Vector3 p = launchPoint + launchVelocity * age;
        p.y -= 0.5f * 9.81f * age * age;

        if (p.y <= 0f) {
            TargetPoint.FillBuffer(targetPoint, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++) {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
            OriginFactory.Reclaim(this);
            return false;
        }

        transform.localPosition = p;

        Vector3 d = launchVelocity;
        d.y -= 9.81f * age;
        transform.localRotation = Quaternion.LookRotation(d);
        return true;

    }
}
