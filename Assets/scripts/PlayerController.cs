using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public float force = 5f;

    void Start()
    {
        
    }

    void Update()
    {
        rb.AddForce(Vector3.forward * force);
    }
}