using System.Collections;
using UnityEngine;

/// <summary>
/// Periodic spike trap driven by a coroutine:
/// safe -> warning (plate flashes) -> spikes up (deadly) -> spikes down -> repeat.
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    [Header("Parts")]
    public Transform spikes;
    public Renderer warningPlate;

    [Header("Timing (seconds)")]
    public float startDelay = 0f;
    public float safeDuration = 2.5f;
    public float warningDuration = 0.8f;
    public float activeDuration = 1.8f;
    public float moveDuration = 0.15f;

    [Header("Look")]
    public float hiddenOffset = -1.1f;
    public Color safeColor = new Color(0.25f, 0.25f, 0.25f);
    public Color warningColor = new Color(1f, 0.6f, 0f);
    public Color dangerColor = new Color(0.9f, 0.1f, 0.1f);

    public bool IsDeadly { get; private set; }

    Vector3 upPos;
    Vector3 downPos;

    void Awake()
    {
        upPos = spikes.localPosition;
        downPos = upPos + Vector3.up * hiddenOffset;
        spikes.localPosition = downPos;
    }

    IEnumerator Start()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // SAFE
            IsDeadly = false;
            SetColor(safeColor);
            yield return new WaitForSeconds(safeDuration);

            // WARNING
            float t = 0f;
            while (t < warningDuration)
            {
                t += Time.deltaTime;
                SetColor(Mathf.PingPong(t * 8f, 1f) > 0.5f ? warningColor : safeColor);
                yield return null;
            }

            // ACTIVE
            SetColor(dangerColor);
            IsDeadly = true;
            yield return Move(downPos, upPos);
            yield return new WaitForSeconds(activeDuration);

            // RETRACT
            IsDeadly = false;
            yield return Move(upPos, downPos);
        }
    }

    IEnumerator Move(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            spikes.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }

    void SetColor(Color c)
    {
        if (warningPlate != null) warningPlate.material.color = c;
    }
}
