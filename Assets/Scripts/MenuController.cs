using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class MenuController : MonoBehaviour {
    [SerializeField, Required]
    private UIDocument uIDocument = default;

    [SerializeField, Required]
    private GameScenario[] scenarios = default;

    [SerializeField, Required]
    private String gameSceneName = default;


    // Lax menu only things can be present for a short time with the game running
    [SerializeField]
    private GameObject[] menuOnlyLax;

    // Strict menu only things must not ever be present when the game is running
    [SerializeField]
    private GameObject[] menuOnlyStrict;

    private bool scenarioFinished = false;

    private void Awake() {
        PopulateUI();
    }

    private void PopulateUI() {
        VisualElement scenarioSelect = uIDocument.rootVisualElement.Q<VisualElement>("ScenarioSelect");
        scenarioSelect.hierarchy.Clear();
        foreach (GameScenario scenario in scenarios) {
            Button button = new Button(() => {
                StartCoroutine(LoadScenario(scenario));
            });
            button.text = scenario.scenarioName;
            scenarioSelect.Add(button);
        }
    }

    private IEnumerator LoadScenario(GameScenario scenario) {
        Debug.Log("Player selected scenario " + scenario.scenarioName);

        foreach (GameObject menuOnly in this.menuOnlyStrict) {
            menuOnly.SetActive(false);
        }
        yield return SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);
        foreach (GameObject menuOnly in this.menuOnlyLax) {
            menuOnly.SetActive(false);
        }

        Game game = FindObjectOfType<Game>();
        game.scenario = scenario;
        game.GameOverHandler += RecordScenarioFinished;
    }

    private void RecordScenarioFinished() {
        scenarioFinished = true;
    }

    private void Update() {
        if (scenarioFinished) {
            scenarioFinished = false;
            StartCoroutine(UnloadScenario());
        }
    }

    private IEnumerator UnloadScenario() {
        Game game = FindObjectOfType<Game>();
        game.TearDownGame();

        foreach (GameObject menuOnly in this.menuOnlyLax) {
            menuOnly.SetActive(true);
        }
        yield return SceneManager.UnloadSceneAsync(gameSceneName);
        foreach (GameObject menuOnly in this.menuOnlyStrict) {
            menuOnly.SetActive(true);
        }
        PopulateUI();
    }
}
