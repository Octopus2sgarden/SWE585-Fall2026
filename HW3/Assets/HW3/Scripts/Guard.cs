using System.Collections;
using UnityEngine;

/// <summary>
/// Guard that patrols between two points forever (coroutine).
/// If the player is in front of it (inside the view cone, not behind a wall)
/// and close enough, or touches it, the player loses.
/// </summary>
public class Guard : MonoBehaviour
{
    [Header("Patrol")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2.5f;
    public float turnSpeed = 240f;
    public float waitAtEnds = 1.2f;

    [Header("Detection")]
    public Transform player;
    [Tooltip("Tick = the guard kills ONLY when it touches the player.")]
    public bool touchOnly = false;
    [Tooltip("Player directly in front AND closer than this = caught (PDF: 'passes in front and is too close').")]
    public float viewDistance = 2.5f;
    public float viewAngle = 60f;
    public float catchDistance = 1.15f;   // capsules touching
    public float eyeHeight = 0.6f;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (pointA != null && pointB != null) StartCoroutine(Patrol());
    }

    IEnumerator Patrol()
    {
        transform.position = Flat(pointA.position);
        while (true)
        {
            yield return TurnTowards(pointB.position);
            yield return MoveTo(pointB.position);
            yield return new WaitForSeconds(waitAtEnds);
            yield return TurnTowards(pointA.position);
            yield return MoveTo(pointA.position);
            yield return new WaitForSeconds(waitAtEnds);
        }
    }

    Vector3 Flat(Vector3 p)
    {
        p.y = transform.position.y;
        return p;
    }

    IEnumerator TurnTowards(Vector3 target)
    {
        Vector3 dir = Flat(target) - transform.position;
        if (dir.sqrMagnitude < 0.001f) yield break;
        Quaternion goal = Quaternion.LookRotation(dir, Vector3.up);
        while (Quaternion.Angle(transform.rotation, goal) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = goal;
    }

    IEnumerator MoveTo(Vector3 target)
    {
        target = Flat(target);
        while ((transform.position - target).sqrMagnitude > 0.0004f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    void Update()
    {
        if (player == null || GameManager.Instance == null || GameManager.IsGameOver) return;
        if (CanSeePlayer())
            GameManager.Instance.Lose(Vector3.Distance(player.position, transform.position) < catchDistance + 0.3f ? "You touched the guard!" : "The guard saw you right in front of him!");
    }

    bool CanSeePlayer()
    {
        Vector3 to = player.position - transform.position;
        to.y = 0f;
        float d = to.magnitude;

        if (d < catchDistance) return true;              // touching
        if (touchOnly) return false;
        if (d > viewDistance) return false;              // too far
        if (Vector3.Angle(transform.forward, to) > viewAngle * 0.5f) return false; // not in front

        // Wall in between? (player is on Ignore Raycast layer, so any hit = obstacle)
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 ray = (player.position + Vector3.up * 0.5f) - eye;
        if (Physics.Raycast(eye, ray.normalized, ray.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null || GameManager.IsGameOver) return;
        if (collision.collider.GetComponentInParent<PlayerController>() != null)
            GameManager.Instance.Lose("You touched the guard!");
    }

    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
        Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}
