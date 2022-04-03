using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu, InlineEditor(InlineEditorModes.GUIAndHeader)]
public class GameScenario : ScriptableObject {

    [SerializeField]
    public string scenarioName = default;

    [SerializeField]
    EnemyWave[] waves = { };

    [SerializeField, Range(0, 10)]
    int cycles = 1;

    [SerializeField, Range(0f, 1f)]
    float cycleSpeedUp = 0.5f;

    public State Begin() => new State(this);

    [System.Serializable]
    public struct State {

        GameScenario scenario;
        int cycle;
        int index;
        float timeScale;
        EnemyWave.State wave;

        public State(GameScenario scenario) {
            this.scenario = scenario;
            cycle = 0;
            index = 0;
            timeScale = 1f;
            Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");
            wave = scenario.waves[0].Begin();
        }

        public bool Progress() {
            float deltaTime = wave.Progress(timeScale * Time.deltaTime);
            while (deltaTime >= 0f) {
                index += 1;
                if (index >= scenario.waves.Length) {
                    cycle += 1;
                    if (cycle >= scenario.cycles) {
                        // scenario finished
                        return false;
                    }
                    // player has moved on to the next cycle in scenario
                    index = 0;
                    timeScale += scenario.cycleSpeedUp;
                }
                wave = scenario.waves[index].Begin();
                deltaTime = wave.Progress(deltaTime);
            }
            return true;
        }

        public string GetProgressString() {
            StringBuilder sb = new StringBuilder();

            wave.AddProgressString(sb);
            if (scenario.cycles == -1) {
                sb.Append(" (Endless: ");
                sb.Append(timeScale.ToString("p0"));
                sb.Append(" spawn speed)");
            }
            else if (scenario.cycles != 1) {
                sb.Append(" (Cycle ");
                sb.Append(cycle + 1);
                sb.Append(" / ");
                sb.Append(scenario.cycles);
                sb.Append(": ");
                sb.Append(timeScale.ToString("p0"));
                sb.Append(" spawn speed)");
            }
            return sb.ToString();
        }
    }
}
