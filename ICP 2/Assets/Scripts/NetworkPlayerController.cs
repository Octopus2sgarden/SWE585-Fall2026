using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public sealed class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 120f;

    // Optional: only used if NetworkPlayerHealth (bonus) is on the prefab.
    private NetworkPlayerHealth health;

    private void Awake()
    {
        TryGetComponent(out health);
    }

    private void Update()
    {
        if (!isLocalPlayer || Keyboard.current == null)
            return;

        // Bonus B: no moving or firing while defeated.
        if (health != null && health.IsDefeated)
            return;

        Keyboard keyboard = Keyboard.current;
        float horizontal = Axis(keyboard.aKey, keyboard.dKey)
                         + Axis(keyboard.leftArrowKey, keyboard.rightArrowKey);
        float vertical = Axis(keyboard.sKey, keyboard.wKey)
                       + Axis(keyboard.downArrowKey, keyboard.upArrowKey);
        float turn = Axis(keyboard.qKey, keyboard.eKey);

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);

        if (keyboard.spaceKey.wasPressedThisFrame)
            CmdFire(transform.position + transform.forward * 1.1f
                    + Vector3.up * 0.5f, transform.rotation);
    }

    private static float Axis(KeyControl negative, KeyControl positive)
    {
        return (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
    }

    [Command]
    private void CmdFire(Vector3 position, Quaternion rotation)
    {
        // Server-side check too: a defeated player can't fire.
        if (health != null && health.IsDefeated)
            return;

        GameObject projectile = Instantiate(projectilePrefab, position, rotation);
        NetworkServer.Spawn(projectile);
    }
}
