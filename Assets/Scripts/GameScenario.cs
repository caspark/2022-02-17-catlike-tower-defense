using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject {

    [SerializeField]
    EnemyWave[] waves = { };

    public State Begin() => new State(this);

    [System.Serializable]
    public struct State {

        GameScenario scenario;

        int index;

        EnemyWave.State wave;

        public State(GameScenario scenario) {
            this.scenario = scenario;
            index = 0;
            Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");
            wave = scenario.waves[0].Begin();
        }

        public bool Progress() {
            float deltaTime = wave.Progress(Time.deltaTime);
            while (deltaTime >= 0f) {
                index += 1;
                if (index >= scenario.waves.Length) {
                    return false;
                }
                wave = scenario.waves[index].Begin();
                deltaTime = wave.Progress(deltaTime);
            }
            return true;
        }
    }
}
