using UnityEngine;

/// <summary>
/// Trigger volume over the trap. Kills the player while the trap is deadly.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrapKillZone : MonoBehaviour
{
    public SpikeTrap trap;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (trap == null || !trap.IsDeadly) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;
        if (GameManager.Instance != null)
            GameManager.Instance.Lose("You were impaled by the spike trap!");
    }
}
