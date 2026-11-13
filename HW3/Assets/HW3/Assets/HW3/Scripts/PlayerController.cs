using UnityEngine;

/// <summary>
/// Third-person, camera-relative Rigidbody movement.
/// Because the player is a physics body, walking into the Key pushes it.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpVelocity = 5.5f;
    public float turnSpeed = 12f;

    [Header("References")]
    public Transform cameraTransform;

    Rigidbody rb;
    Vector3 moveDir;
    bool jumpQueued;
    bool dead;
    KeyObject heldKey;

    public bool IsDead => dead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 3f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.sleepThreshold = 0f; // keep trigger events firing while standing still

        var col = GetComponent<Collider>();
        if (col != null) PhysicsCompat.MakeFrictionless(col); // don't stick to walls
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (dead || GameManager.IsGameOver)
        {
            moveDir = Vector3.zero;
            return;
        }

        Vector2 input = GameInput.Move();

        Vector3 fwd = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
        fwd.y = 0f; right.y = 0f;
        fwd.Normalize(); right.Normalize();

        moveDir = Vector3.ClampMagnitude(fwd * input.y + right * input.x, 1f);

        if (GameInput.JumpPressed() && IsGrounded())
            jumpQueued = true;

        if (heldKey != null && !heldKey.IsHeld) heldKey = null; // auto-dropped
        if (GameInput.InteractPressed()) ToggleCarry();
    }

    void ToggleCarry()
    {
        if (heldKey != null)
        {
            heldKey.Drop();
            heldKey = null;
            return;
        }
        foreach (var c in Physics.OverlapSphere(transform.position, 2f, ~0, QueryTriggerInteraction.Ignore))
        {
            var k = c.GetComponentInParent<KeyObject>();
            if (k != null)
            {
                k.PickUp(transform);
                heldKey = k;
                if (GameManager.Instance != null) GameManager.Instance.ShowHint("Got the key! Carry it into the green door (E to drop).", 3f);
                return;
            }
        }
    }

    void FixedUpdate()
    {
        if (dead) return;

        float speed = GameInput.RunHeld() ? runSpeed : walkSpeed;
        Vector3 v = PhysicsCompat.GetVelocity(rb);
        Vector3 target = moveDir * speed;
        v.x = target.x;
        v.z = target.z;

        if (jumpQueued)
        {
            v.y = jumpVelocity;
            jumpQueued = false;
        }
        PhysicsCompat.SetVelocity(rb, v);

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(moveDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, turnSpeed * Time.fixedDeltaTime));
        }
    }

    bool IsGrounded()
    {
        // Player is on the "Ignore Raycast" layer, so the ray never hits the player itself.
        return Physics.Raycast(transform.position, Vector3.down, 1.15f,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    /// <summary>Called by GameManager when the player is caught / killed.</summary>
    public void Die()
    {
        if (dead) return;
        dead = true;
        if (heldKey != null) { heldKey.Drop(); heldKey = null; }
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddTorque(transform.right * 6f, ForceMode.VelocityChange); // fall over
    }
}
