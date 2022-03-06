using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : WarEntity {
    [SerializeField, Range(0f, 1f)]
    float duration = 0.5f;

    float age;

    public void Initialize(Vector3 position, float blastRadius, float damage) {
        TargetPoint.FillBuffer(position, blastRadius);
        for (int i = 0; i < TargetPoint.BufferedCount; i++) {
            TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
        }
        transform.localPosition = position;
        transform.localScale = Vector3.one * 2f * blastRadius;
    }

    public override bool GameUpdate() {
        age += Time.deltaTime;
        if (age >= duration) {
            OriginFactory.Reclaim(this);
            return false;
        }
        return true;
    }
}
