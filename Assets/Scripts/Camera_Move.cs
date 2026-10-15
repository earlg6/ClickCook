using UnityEngine;
using UnityEngine.InputSystem;

public class CameraCursorFollow : MonoBehaviour
{
    [Header("Maximum Rotation")]
    [SerializeField] private float maxHorizontalAngle = 5f;
    [SerializeField] private float maxVerticalAngle = 3f;

    [Header("Smoothness")]
    [SerializeField] private float smoothTime = 0.15f;

    private Quaternion initialRotation;
    private Vector3 currentRotation;
    private Vector3 rotationVelocity;

    public Quaternion CursorRotation => Quaternion.Euler(currentRotation);

    private void Start()
    {
        initialRotation = transform.localRotation;
        currentRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Keep the counter readable when the pointer leaves the Game View/window.
        float mouseX = Mathf.Clamp((mousePosition.x / Mathf.Max(1, Screen.width)) * 2f - 1f, -1f, 1f);
        float mouseY = Mathf.Clamp((mousePosition.y / Mathf.Max(1, Screen.height)) * 2f - 1f, -1f, 1f);

        float targetX = -mouseY * maxVerticalAngle;
        float targetY = mouseX * maxHorizontalAngle;

        Vector3 targetRotation = initialRotation.eulerAngles;
        targetRotation.x += targetX;
        targetRotation.y += targetY;

        currentRotation = Vector3.SmoothDamp(
            currentRotation,
            targetRotation,
            ref rotationVelocity,
            smoothTime
        );

        transform.localRotation = Quaternion.Euler(currentRotation);
    }
}
