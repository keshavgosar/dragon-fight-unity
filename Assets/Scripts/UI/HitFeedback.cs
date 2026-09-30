using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Reacts to Health.Damaged: colour flash, number popup, hit VFX, sound, camera shake.</summary>
[RequireComponent(typeof(Health))]
public class HitFeedback : MonoBehaviour
{
    [SerializeField] DamagePopup popupPrefab;
    [SerializeField] ParticleSystem hitVfxPrefab;
    [SerializeField] AudioClip hitSfx;
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashTime = 0.15f;
    [SerializeField] Vector3 hitPointOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] float shakeMagnitude = 0.12f;

    Health health;
    Renderer[] renderers;
    Color[] originals;
    int[] propertyIds;
    MaterialPropertyBlock block;
    Coroutine flashRoutine;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP
    static readonly int ColorId = Shader.PropertyToID("_Color");         // Built-in

    void Awake()
    {
        health = GetComponent<Health>();
        block = new MaterialPropertyBlock();

        var list = new List<Renderer>();
        foreach (var r in GetComponentsInChildren<Renderer>())
            if (r is MeshRenderer || r is SkinnedMeshRenderer) list.Add(r);
        renderers = list.ToArray();

        originals = new Color[renderers.Length];
        propertyIds = new int[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            var m = renderers[i].sharedMaterial;
            if (m != null && m.HasProperty(BaseColorId)) { propertyIds[i] = BaseColorId; originals[i] = m.GetColor(BaseColorId); }
            else if (m != null && m.HasProperty(ColorId)) { propertyIds[i] = ColorId; originals[i] = m.GetColor(ColorId); }
            else propertyIds[i] = -1;
        }

        health.Damaged += OnDamaged;
    }

    void OnDestroy() { if (health) health.Damaged -= OnDamaged; }

    void OnDamaged(float amount)
    {
        Vector3 point = transform.position + hitPointOffset;
        DamagePopup.Spawn(popupPrefab, point, amount);

        if (hitVfxPrefab)
        {
            var vfx = Instantiate(hitVfxPrefab, point, Quaternion.identity);
            Destroy(vfx.gameObject, 2f);
        }
        Sfx.Play(hitSfx);
        if (CameraRig.Instance && amount >= 10f) CameraRig.Instance.Shake(0.12f, shakeMagnitude);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float t = 0f;
        while (t < flashTime)
        {
            Apply(1f - t / flashTime);
            t += Time.deltaTime;
            yield return null;
        }
        Apply(0f);
    }

    void Apply(float k)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (propertyIds[i] < 0) continue;
            renderers[i].GetPropertyBlock(block);
            block.SetColor(propertyIds[i], Color.Lerp(originals[i], flashColor, k));
            renderers[i].SetPropertyBlock(block);
        }
    }
}
