using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct EnemyAnimator {
    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    public enum Clip { Move, Intro, Outro }

    public Clip CurrentClip { get; private set; }

    public bool IsDone => GetPlayable(CurrentClip).IsDone();

    public void Configure(Animator animator, EnemyAnimationConfig config) {
        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, 3);

        var clip = AnimationClipPlayable.Create(graph, config.Move);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Move, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Intro);
        clip.SetDuration(config.Intro.length);
        mixer.ConnectInput((int)Clip.Intro, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Outro);
        clip.SetDuration(config.Outro.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Outro, clip, 0);

        var output = AnimationPlayableOutput.Create(graph, "Enemy", animator);
        output.SetSourcePlayable(mixer);
    }

    public void PlayIntro() {
        SetWeight(Clip.Intro, 1f);
        CurrentClip = Clip.Intro;
        graph.Play();
    }

    public void PlayMove(float speed) {
        SetWeight(CurrentClip, 0f);
        SetWeight(Clip.Move, 1f);
        Playable clip = GetPlayable(Clip.Move);
        clip.SetSpeed(speed);
        clip.Play();
        CurrentClip = Clip.Move;
    }

    public void PlayOutro() {
        SetWeight(CurrentClip, 0f);
        SetWeight(Clip.Outro, 1f);
        GetPlayable(Clip.Outro).Play();
        CurrentClip = Clip.Outro;
    }

    private Playable GetPlayable(Clip clip) {
        return mixer.GetInput((int)clip);
    }

    private void SetWeight(Clip clip, float weight) {
        mixer.SetInputWeight((int)clip, weight);
    }

    public void Stop() {
        graph.Stop();
    }

    public void Destroy() {
        graph.Destroy();
    }
}
