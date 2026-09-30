using System.Collections;
using UnityEngine;

/// <summary>Angled top-down (TFT / Underlords style) camera that keeps both dragons framed.</summary>
public class CameraRig : MonoBehaviour
{
    public static CameraRig Instance { get; private set; }

    [SerializeField] Transform a;
    [SerializeField] Transform b;
    [SerializeField] float pitch = 55f;
    [SerializeField] float minDistance = 14f;
    [SerializeField] float maxDistance = 28f;
    [SerializeField] float zoomPerUnit = 0.7f;
    [SerializeField] float smoothTime = 0.25f;

    Vector3 velocity;
    Vector3 shakeOffset;

    void Awake()
    {
        Instance = this;
        transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void LateUpdate()
    {
        Vector3 mid = (a.position + b.position) * 0.5f;
        float separation = Vector3.Distance(a.position, b.position);
        float dist = Mathf.Clamp(minDistance + separation * zoomPerUnit, minDistance, maxDistance);
        Vector3 target = mid - transform.forward * dist;
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime) + shakeOffset;
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.25f)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            shakeOffset = Random.insideUnitSphere * magnitude * (1f - t / duration);
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
