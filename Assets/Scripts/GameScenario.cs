using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
class LdTkLevel {
    public LdtkLayer[] layerInstances;
}

[System.Serializable]
class LdtkLayer {
    public int __cHei, __cWid;
    public int[] intGridCsv;
}

[CreateAssetMenu, InlineEditor(InlineEditorModes.GUIAndHeader)]
public class GameScenario : ScriptableObject {

    public enum LevelEntity {
        Empty, Wall, Destination, Spawn, LaserTower, MortarTower
    }
    public struct LevelData {
        public Vector2Int size;

        public LevelEntity[] entities;
    }

    [SerializeField]
    public string scenarioName = default;

    [SerializeField]
    TextAsset scenarioLevel = default;

    [SerializeField]
    EnemyWave[] waves = { };

    [SerializeField, Range(0, 10)]
    int cycles = 1;

    [SerializeField, Range(0f, 1f)]
    float cycleSpeedUp = 0.5f;

    public State Begin() => new State(this);

    public LevelData LoadLevelData() {
        LevelData data = new LevelData();
        if (scenarioLevel == null) {
            Debug.Log("No level data found for scenario ", this);
            data.size = new Vector2Int(10, 10);

            data.entities = new LevelEntity[data.size.x * data.size.y];
            for (int i = 0; i < data.entities.Length; i++) {
                data.entities[i] = LevelEntity.Empty;
            }

            data.entities[0] = LevelEntity.Spawn;
            int dest = data.size.x * data.size.y / 2;
            data.entities[dest] = LevelEntity.Destination;
            data.entities[dest / 2 - 1] = LevelEntity.Wall;
            data.entities[dest / 2] = LevelEntity.LaserTower;
            data.entities[dest / 2 + 1] = LevelEntity.MortarTower;
            Debug.Log("Generated level data for scenario " + string.Join(", ", data.entities));
        }
        else {
            Debug.Log("Loading level data for scenario from " + scenarioLevel.name, this);
            LdTkLevel level = JsonUtility.FromJson<LdTkLevel>(scenarioLevel.text);
            LdtkLayer layer = level.layerInstances[0];
            data.size = new Vector2Int(layer.__cWid, layer.__cHei);
            data.entities = new LevelEntity[layer.intGridCsv.Length];
            for (int i = 0; i < layer.intGridCsv.Length; i++) {
                int val = layer.intGridCsv[i];
                LevelEntity entity = (LevelEntity)val;
                data.entities[i] = entity;
            }
        }
        return data;
    }

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
