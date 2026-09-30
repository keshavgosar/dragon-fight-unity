using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Health bar with a trailing "damage" bar. Optionally follows a dragon (overhead bar).</summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Image fill;          // Image Type = Filled, Horizontal
    [SerializeField] Image delayedFill;   // optional trailing bar behind the fill
    [SerializeField] TMP_Text label;      // optional "120 / 200"
    [SerializeField] float trailSpeed = 0.5f;
    [Header("Optional: follow the dragon in screen space")]
    [SerializeField] Transform follow;
    [SerializeField] Vector3 worldOffset = new Vector3(0f, 4f, 0f);

    float target = 1f;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        health.Changed += OnChanged;
        OnChanged(health.Current, health.Max);
        if (delayedFill) delayedFill.fillAmount = target;
    }

    void OnDestroy() { if (health) health.Changed -= OnChanged; }

    void OnChanged(float current, float max)
    {
        target = current / max;
        fill.fillAmount = target;
        if (label) label.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }

    void Update()
    {
        if (delayedFill)
            delayedFill.fillAmount = Mathf.MoveTowards(delayedFill.fillAmount, target, trailSpeed * Time.unscaledDeltaTime);
        if (follow && cam)
            transform.position = cam.WorldToScreenPoint(follow.position + worldOffset);
    }
}
