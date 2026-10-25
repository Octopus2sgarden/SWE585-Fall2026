using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBooster : MonoBehaviour
{
    [SerializeField]
    private Vector3 direction = new Vector3(-5, 0, 0);
    private Vector3 startPos;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    // FixedUpdate is synchronized with Unity's physics timestep.
    void FixedUpdate()
    {
        rb.AddForce(direction);
    }

    void OnCollisionEnter(Collision collision)
    {
    if (collision.gameObject.CompareTag("Barrier"))
    {
        transform.localScale = transform.localScale * 1.5f;
    }
    }

}
