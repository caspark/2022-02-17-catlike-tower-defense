using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct EnemyAnimator {
    PlayableGraph graph;

    public void Configure(Animator animator, EnemyAnimationConfig config) {
        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var clip = AnimationClipPlayable.Create(graph, config.Move);

        var output = AnimationPlayableOutput.Create(graph, "Enemy", animator);
        output.SetSourcePlayable(clip);
    }

    public void Play(float speed) {
        graph.GetOutput(0).GetSourcePlayable().SetSpeed(speed);
        graph.Play();
    }

    public void Stop() {
        graph.Destroy();
    }
}
