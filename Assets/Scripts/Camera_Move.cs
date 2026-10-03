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

        float mouseX = (mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (mousePosition.y / Screen.height) * 2f - 1f;

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