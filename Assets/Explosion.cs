using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : WarEntity {
    static int colorPropertyID = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock propertyBlock;

    [SerializeField, Range(0f, 1f)]
    float duration = 0.5f;

    [SerializeField]
    AnimationCurve opacityCurve = default;
    [SerializeField]
    AnimationCurve scaleCurve = default;

    float age;
    float scale;
    MeshRenderer meshRenderer;

    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(meshRenderer != null, "Explosion needs a mesh renderer!", this);
    }

    public void Initialize(Vector3 position, float blastRadius, float damage = 0f) {
        if (damage > 0) {
            TargetPoint.FillBuffer(position, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++) {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
        }
        transform.localPosition = position;
        scale = 2f * blastRadius;
    }

    public override bool GameUpdate() {
        age += Time.deltaTime;
        if (age >= duration) {
            OriginFactory.Reclaim(this);
            return false;
        }

        if (propertyBlock == null) {
            propertyBlock = new MaterialPropertyBlock();
        }
        float t = age / duration;
        Color c = Color.clear;
        c.a = opacityCurve.Evaluate(t);
        propertyBlock.SetColor(colorPropertyID, c);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scaleCurve.Evaluate(t) * scale);

        return true;
    }
}
