using UnityEngine;

/// <summary>
/// The key: a physics cube (forces, collisions, gravity).
/// - Walk into it to PUSH it.
/// - Press E near it to PICK IT UP; it floats in front of the player but stays a
///   dynamic Rigidbody, so its collision with the door still fires OnCollisionEnter.
/// If it falls off the map it respawns at its start position.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class KeyObject : MonoBehaviour
{
    public float respawnBelowY = -5f;
    public float holdDistance = 1.1f;
    public float holdHeight = 0.2f;
    public float followStrength = 15f;
    public float maxHoldGap = 3f;      // stuck behind a wall -> auto drop

    Rigidbody rb;
    Collider myCol;
    Vector3 startPos;
    Quaternion startRot;

    Transform holder;
    Collider holderCol;

    public bool IsHeld => holder != null;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();
        rb.mass = 1f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        PhysicsCompat.SetDamping(rb, 0.5f, 0.5f);
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void PickUp(Transform who)
    {
        holder = who;
        holderCol = who.GetComponent<Collider>();
        if (holderCol != null && myCol != null) Physics.IgnoreCollision(myCol, holderCol, true);
        rb.useGravity = false;
    }

    public void Drop()
    {
        if (holderCol != null && myCol != null) Physics.IgnoreCollision(myCol, holderCol, false);
        holder = null;
        holderCol = null;
        rb.useGravity = true;
    }

    void FixedUpdate()
    {
        if (holder == null) return;

        Vector3 target = holder.position + holder.forward * holdDistance + Vector3.up * holdHeight;
        Vector3 delta = target - rb.position;
        if (delta.magnitude > maxHoldGap)
        {
            Drop();
            return;
        }
        PhysicsCompat.SetVelocity(rb, Vector3.ClampMagnitude(delta * followStrength, 20f));
        rb.angularVelocity = Vector3.zero;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, holder.rotation, 10f * Time.fixedDeltaTime));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsHeld || GameManager.Instance == null || GameManager.IsGameOver) return;
        if (collision.collider.GetComponentInParent<PlayerController>() != null)
            GameManager.Instance.ShowHint("Press E to pick up the key", 2f);
    }

    void Update()
    {
        if (transform.position.y < respawnBelowY) Respawn();
    }

    public void Respawn()
    {
        Drop();
        PhysicsCompat.SetVelocity(rb, Vector3.zero);
        rb.angularVelocity = Vector3.zero;
        rb.position = startPos;
        rb.rotation = startRot;
        transform.SetPositionAndRotation(startPos, startRot);
        if (GameManager.Instance != null)
            GameManager.Instance.ShowHint("The key fell and returned to its start!");
    }
}
