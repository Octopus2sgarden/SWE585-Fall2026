using System.Collections;
using Mirror;
using UnityEngine;

// Bonus A + Bonus B: server-authoritative health, defeat and respawn.
// Attach to the NetworkPlayer prefab (next to NetworkPlayerController).
public sealed class NetworkPlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float respawnDelay = 2f;

    [SyncVar]
    [SerializeField] private int health = 3;

    [SyncVar]
    private bool isDefeated;

    public int Health => health;
    public bool IsDefeated => isDefeated;

    public override void OnStartServer()
    {
        health = maxHealth;
        isDefeated = false;
    }

    // Only the server may change health.
    [Server]
    public void TakeDamage(int amount)
    {
        // Ignore hits while defeated, so one defeat can't trigger twice.
        if (isDefeated || health <= 0)
            return;

        health = Mathf.Max(0, health - amount);

        if (health == 0)
        {
            isDefeated = true;
            StartCoroutine(RespawnAfterDelay());
        }
    }

    [Server]
    private IEnumerator RespawnAfterDelay()
    {
        RpcSetVisible(false);

        yield return new WaitForSeconds(respawnDelay);

        Transform start = NetworkManager.singleton.GetStartPosition();
        Vector3 position = start != null ? start.position : Vector3.up;
        Quaternion rotation = start != null ? start.rotation : Quaternion.identity;

        // Server sets its own copy first...
        transform.SetPositionAndRotation(position, rotation);
        GetComponent<NetworkTransformReliable>().ResetState();

        // ...then corrects the owning client, because the transform is Client To Server.
        TargetApplyRespawn(connectionToClient, position, rotation);

        health = maxHealth;
        isDefeated = false;
        RpcSetVisible(true);
    }

    [TargetRpc]
    private void TargetApplyRespawn(NetworkConnectionToClient target,
                                    Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        GetComponent<NetworkTransformReliable>().ResetState();
    }

    // Hide/show the player and disable/enable its colliders on every instance.
    [ClientRpc]
    private void RpcSetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = visible;

        // Remote copies: clear old interpolation so they don't slide from the death spot.
        if (visible && !isOwned)
            GetComponent<NetworkTransformReliable>().ResetState();
    }

    // Simple health display for the local player (top-right, away from the HUD).
    private void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        string text = isDefeated ? "DEFEATED - respawning..." : $"Health: {health} / {maxHealth}";
        GUI.Label(new Rect(Screen.width - 210, 10, 200, 30), text);
    }
}
