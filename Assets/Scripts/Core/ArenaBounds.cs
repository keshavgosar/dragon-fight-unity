using UnityEngine;

/// <summary>Rectangular play area. Physical wall colliders stop the dragons; this clamp is a safety net
/// (knockback, flying) so nothing can ever leave the arena.</summary>
public class ArenaBounds : MonoBehaviour
{
    static ArenaBounds instance;

    [SerializeField] Vector2 halfSize = new Vector2(12f, 8f); // x, z
    [SerializeField] float margin = 1f;

    void Awake() => instance = this;

    public static Vector3 Clamp(Vector3 p)
    {
        if (instance == null) return p;
        Vector3 c = instance.transform.position;
        float hx = instance.halfSize.x - instance.margin;
        float hz = instance.halfSize.y - instance.margin;
        p.x = Mathf.Clamp(p.x, c.x - hx, c.x + hx);
        p.z = Mathf.Clamp(p.z, c.z - hz, c.z + hz);
        return p;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(halfSize.x * 2f, 1f, halfSize.y * 2f));
    }
}
