using TMPro;
using UnityEngine;

/// <summary>Floating damage number (world-space TextMeshPro that faces the camera).</summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] TextMeshPro text;
    [SerializeField] float life = 0.8f;
    [SerializeField] Vector3 velocity = new Vector3(0f, 2.5f, 0f);

    float t;
    Color baseColor;
    Camera cam;

    public static void Spawn(DamagePopup prefab, Vector3 position, float amount)
    {
        if (prefab == null) return;
        var p = Instantiate(prefab, position + Random.insideUnitSphere * 0.3f, Quaternion.identity);
        p.text.text = Mathf.RoundToInt(amount).ToString();
    }

    void Awake()
    {
        cam = Camera.main;
        baseColor = text.color;
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (cam) transform.rotation = cam.transform.rotation;

        float k = t / life;
        transform.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, Mathf.Clamp01(k * 4f)); // pop-in
        text.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - k * k);
        if (t >= life) Destroy(gameObject);
    }
}
