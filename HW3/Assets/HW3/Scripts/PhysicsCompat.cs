using UnityEngine;

/// <summary>
/// Helpers that hide API renames between Unity 2022 (velocity/drag/PhysicMaterial)
/// and Unity 6 (linearVelocity/linearDamping/PhysicsMaterial).
/// </summary>
public static class PhysicsCompat
{
    public static Vector3 GetVelocity(Rigidbody rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    public static void SetVelocity(Rigidbody rb, Vector3 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }

    public static void SetDamping(Rigidbody rb, float linear, float angular)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = linear;
        rb.angularDamping = angular;
#else
        rb.drag = linear;
        rb.angularDrag = angular;
#endif
    }

    public static void MakeFrictionless(Collider c)
    {
#if UNITY_6000_0_OR_NEWER
        var m = new PhysicsMaterial("Frictionless")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
#else
        var m = new PhysicMaterial("Frictionless")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicMaterialCombine.Minimum
        };
#endif
        c.material = m;
    }
}
