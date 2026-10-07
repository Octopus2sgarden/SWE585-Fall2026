using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public float force = 5f;

    void Start()
    {
        
    }

    void Update()
    {
        Vector2 input = Keyboard.current.wKey.isPressed
            ? Vector2.up
            : Keyboard.current.sKey.isPressed
            ? Vector2.down
            : Vector2.zero;

        if (Keyboard.current.aKey.isPressed)
            input.x = -1;

        if (Keyboard.current.dKey.isPressed)
            input.x = 1;

        Vector3 movement = Camera.main.transform.right * input.x
                  + Camera.main.transform.forward * input.y;

        movement.y = 0;

        rb.AddForce(movement * force);

    }
}