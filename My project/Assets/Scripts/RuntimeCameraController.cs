using UnityEngine;
using UnityEngine.InputSystem;

public class RuntimeCameraController : MonoBehaviour
{
    [SerializeField, Min(0.1f)] float moveSpeed = 20f;
    [SerializeField, Min(1f)] float fastMoveMultiplier = 3f;
    [SerializeField, Min(0.01f)] float lookSensitivity = 0.12f;

    float yaw;
    float pitch;
    bool isLooking;
    CursorLockMode previousCursorLockState;
    bool previousCursorVisible;

    void Awake()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        bool lookHeld = mouse != null && mouse.rightButton.isPressed;
        if (lookHeld && !isLooking)
        {
            BeginLooking();
        }
        else if (!lookHeld && isLooking)
        {
            EndLooking();
        }

        if (!isLooking || mouse == null)
        {
            return;
        }

        Vector2 lookDelta = mouse.delta.ReadValue();
        yaw += lookDelta.x * lookSensitivity;
        pitch = Mathf.Clamp(pitch - lookDelta.y * lookSensitivity, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        Vector3 movement = Vector3.zero;
        if (keyboard.wKey.isPressed) movement += transform.forward;
        if (keyboard.sKey.isPressed) movement -= transform.forward;
        if (keyboard.dKey.isPressed) movement += transform.right;
        if (keyboard.aKey.isPressed) movement -= transform.right;
        if (keyboard.eKey.isPressed) movement += Vector3.up;
        if (keyboard.qKey.isPressed) movement -= Vector3.up;

        if (movement.sqrMagnitude > 0f)
        {
            float speed = moveSpeed;
            if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
            {
                speed *= fastMoveMultiplier;
            }

            transform.position += movement.normalized * speed * Time.deltaTime;
        }
    }

    void OnDisable()
    {
        if (isLooking)
        {
            EndLooking();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isLooking)
        {
            EndLooking();
        }
    }

    void BeginLooking()
    {
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isLooking = true;
    }

    void EndLooking()
    {
        isLooking = false;
        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
    }
}