using System.Collections;
using UnityEngine;

/// <summary>
/// Exit door. When the KEY collides with it (OnCollisionEnter) the door
/// slides open and the game is won.
/// </summary>
public class Door : MonoBehaviour
{
    public float openHeight = 3.2f;
    public float openDuration = 1.5f;

    bool opened;

    void OnCollisionEnter(Collision collision)
    {
        if (opened) return;

        if (collision.collider.GetComponentInParent<KeyObject>() != null)
        {
            opened = true;
            StartCoroutine(OpenRoutine());
            if (GameManager.Instance != null) GameManager.Instance.Win();
        }
        else if (collision.collider.GetComponentInParent<PlayerController>() != null)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ShowHint("Locked! Bring the golden key to the door.");
        }
    }

    IEnumerator OpenRoutine()
    {
        Vector3 from = transform.position;
        Vector3 to = from + Vector3.up * openHeight;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
}
