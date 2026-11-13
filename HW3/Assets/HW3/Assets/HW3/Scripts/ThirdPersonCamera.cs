using UnityEngine;

/// <summary>
/// Mouse-orbit third person camera with wall collision.
/// Esc frees the cursor, left click locks it again.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 7f;
    public float height = 1.2f;
    public float sensitivity = 2.5f;
    public float minPitch = -5f;
    public float maxPitch = 65f;
    public float zoomSmooth = 10f;

    float yaw;
    float pitch = 25f;
    float currentDistance;

    void Start()
    {
        if (target != null) yaw = target.eulerAngles.y;
        currentDistance = distance;
        LockCursor(true);
    }

    public static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (!GameManager.IsGameOver)
        {
            if (GameInput.UnlockPressed()) LockCursor(false);
            else if (GameInput.ClickPressed() && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 d = GameInput.Look();
                yaw += d.x * sensitivity;
                pitch -= d.y * sensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
        }

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * height;
        Vector3 dir = rot * Vector3.back;

        float wanted = distance;
        if (Physics.SphereCast(focus, 0.3f, dir, out RaycastHit hit, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            wanted = Mathf.Max(0.6f, hit.distance - 0.1f);
        }

        // snap in instantly when blocked, ease out smoothly
        currentDistance = wanted < currentDistance
            ? wanted
            : Mathf.Lerp(currentDistance, wanted, Time.deltaTime * zoomSmooth);

        transform.position = focus + dir * currentDistance;
        transform.rotation = rot;
    }
}
