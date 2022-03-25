using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAnimationConfig")]
public class EnemyAnimationConfig : ScriptableObject {
    [SerializeField]
    AnimationClip move = default;

    public AnimationClip Move => move;
}
