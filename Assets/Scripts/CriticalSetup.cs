using UnityEngine;
using UnityEngine.EventSystems;

public class CriticalSetup : MonoBehaviour {
    [SerializeField] EventSystem uiEventSystemPrefab;

    private void Awake() {
        if (FindObjectOfType<EventSystem>() == null) {
            Debug.Log("CriticalSetup: Creating EventSystem");
            Instantiate(uiEventSystemPrefab);
        }
    }
}
