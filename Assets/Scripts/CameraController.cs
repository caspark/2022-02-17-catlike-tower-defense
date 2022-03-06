using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {

    [SerializeField]
    float cameraMoveSpeed = 10f;
    [SerializeField]
    float cameraRotateSpeed = 10f;
    [SerializeField]
    float cameraZoomSpeed = 0.5f;
    [SerializeField, FloatRangeSlider(1f, 12f)]
    FloatRange cameraZoomRange = new FloatRange(1f, 12f);

    [SerializeField]
    CinemachineVirtualCamera primaryCamera;

    [SerializeField]
    bool drawDebugGizmo = false;

    Vector2 mouseDragStartPosition = Vector2.zero;
    Cinemachine3rdPersonFollow primaryCamera3P;

    private void Awake() {
        Debug.Assert(primaryCamera != null, "Primary camera is not set");
        primaryCamera3P = primaryCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    void Update() {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) {
            return;
        }
        Mouse mouse = Mouse.current;
        if (mouse == null) {
            return;
        }

        var dir = new Vector3(
            keyboard.dKey.ReadValue() - keyboard.aKey.ReadValue(),
            0,
            keyboard.wKey.ReadValue() - keyboard.sKey.ReadValue());
        if (dir.sqrMagnitude > 0.01f) {
            transform.Translate(dir.normalized * cameraMoveSpeed * Time.deltaTime);
        }

        if (mouse.middleButton.wasPressedThisFrame) {
            mouseDragStartPosition = mouse.position.ReadValue();
        }
        else if (mouse.middleButton.wasReleasedThisFrame) {
            mouseDragStartPosition = Vector2.zero;
        }
        if (mouseDragStartPosition != Vector2.zero) {
            var diff = (mouse.position.ReadValue() - mouseDragStartPosition) * Time.deltaTime;

            transform.localRotation = Quaternion.Euler(
                transform.localRotation.eulerAngles.x,
                transform.localRotation.eulerAngles.y + diff.x * cameraRotateSpeed,
                transform.localRotation.eulerAngles.z
            );
            mouseDragStartPosition = mouse.position.ReadValue();
        }
        if (mouse.scroll.ReadValue().y != 0) {
            primaryCamera3P.VerticalArmLength = cameraZoomRange.Clamp(primaryCamera3P.VerticalArmLength - mouse.scroll.ReadValue().y * Time.deltaTime * cameraZoomSpeed);
        }
    }

    private void OnDrawGizmos() {
        if (drawDebugGizmo) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
    }
}
