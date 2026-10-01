using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Runs the three abilities. Used by BOTH the player and the AI, so cooldowns are identical.</summary>
[RequireComponent(typeof(DragonMotor), typeof(Health))]
public class DragonCombat : MonoBehaviour
{
    public const int Fire = 0, Tail = 1, Fly = 2;

    [Header("References")]
    public Transform opponent;
    [SerializeField] Transform model;      // visual child (moved up/down while flying)
    [SerializeField] Animator animator;

    [Header("Abilities")]
    public Ability fire = new Ability { name = "Fire Breath", damage = 36f, cooldown = 5f, range = 8f, coneAngle = 50f, windup = 0.35f, activeTime = 1.2f, recovery = 0.3f, animTrigger = "Fire" };
    public Ability tail = new Ability { name = "Tail Whip", damage = 22f, cooldown = 2.5f, range = 3.5f, coneAngle = 140f, windup = 0.35f, activeTime = 0.1f, recovery = 0.3f, knockback = 12f, animTrigger = "Tail" };
    public Ability fly = new Ability { name = "Sky Strike", damage = 40f, cooldown = 9f, range = 14f, windup = 0.5f, recovery = 0.5f, knockback = 16f, animTrigger = "Fly" };

    [Header("Fire tuning")]
    [SerializeField] int fireTicks = 6;

    [Header("Fly tuning")]
    [SerializeField] float flyHeight = 4f;
    [SerializeField] float glideTime = 0.7f;
    [SerializeField] float landTime = 0.25f;
    [SerializeField] float slamRadius = 4f;
    [SerializeField] float landOffset = 1.5f;
    [SerializeField] float slamShake = 0.4f;

    [Header("Tail whip (procedural spin, no tail clip needed)")]
    [SerializeField] bool spinForTail = true;
    [SerializeField] float spinDuration = 0.5f;

    DragonMotor motor;
    Health health;
    Ability[] abilities;
    readonly HashSet<string> triggers = new HashSet<string>();
    float modelBaseY;
    Quaternion modelBaseRot = Quaternion.identity;
    bool busy;

    public Ability[] Abilities => abilities ??= new[] { fire, tail, fly };
    public bool IsBusy => busy;
    public Health Health => health;

    void Awake()
    {
        motor = GetComponent<DragonMotor>();
        health = GetComponent<Health>();

        if (model != null)
        {
            modelBaseY = model.localPosition.y; modelBaseRot = model.localRotation;
        }

        if (animator != null)
            foreach (var p in animator.parameters)
                if (p.type == AnimatorControllerParameterType.Trigger) triggers.Add(p.name);

        foreach (var ab in Abilities)
            if (ab.vfx != null && !ab.vfx.gameObject.scene.IsValid())
                Debug.LogWarning($"{name}: '{ab.name}' VFX is a prefab asset, not a scene object. Drag an instance from the Hierarchy into the slot instead.", this);
        health.Died += OnDied;
    }

    void OnDestroy() { if (health != null) health.Died -= OnDied; }

    public bool TryUse(int index)
    {
        if (busy || health.IsDead || GameManager.IsOver || opponent == null) return false;
        Ability a = Abilities[index];
        if (!a.IsReady) return false;

        switch (index)
        {
            case Fire: StartCoroutine(FireRoutine(a)); break;
            case Tail: StartCoroutine(TailRoutine(a)); break;
            default: StartCoroutine(FlyRoutine(a)); break;
        }
        return true;
    }

    // ---------- Abilities ----------

    IEnumerator FireRoutine(Ability a)
    {
        Begin(a);
        yield return Wait(a.windup);
        if (a.vfx) a.vfx.Play();
        Sfx.Play(a.sfx);

        int ticks = Mathf.Max(1, fireTicks);
        float perTick = a.damage / ticks;
        for (int i = 0; i < ticks; i++)
        {
            FaceOpponent();
            HitCone(perTick, a.range, a.coneAngle, a.knockback);
            yield return new WaitForSeconds(a.activeTime / ticks);
        }
        if (a.vfx) a.vfx.Stop();
        yield return Wait(a.recovery);
        End();
    }

    IEnumerator TailRoutine(Ability a)
    {
        Begin(a);
        yield return Wait(a.windup);
        if (a.vfx) a.vfx.Play();
        Sfx.Play(a.sfx);

        if (spinForTail && model != null)
        {
            float t = 0f;
            bool hit = false;
            while (t < spinDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / spinDuration);
                model.localRotation = Quaternion.AngleAxis(360f * Mathf.SmoothStep(0f, 1f, k), Vector3.up) * modelBaseRot;
                if (!hit && k >= 0.5f)
                {
                    hit = true;
                    HitCone(a.damage, a.range, 360f, a.knockback);
                }
                yield return null;
            }
            model.localRotation = modelBaseRot;
        }
        else
        {
            HitCone(a.damage, a.range, a.coneAngle, a.knockback);
        }

        if (a.vfx) a.vfx.Stop();
        yield return Wait(a.activeTime + a.recovery);
        End();
    }

    IEnumerator FlyRoutine(Ability a)
    {
        Begin(a);
        motor.SetKinematic(true);

        // 1. take off
        yield return MoveModelY(modelBaseY, modelBaseY + flyHeight, a.windup);

        // 2. glide over the opponent
        Sfx.Play(a.sfx);
        if (a.vfx) a.vfx.Play();
        Vector3 start = transform.position;
        Vector3 toTarget = opponent.position - start; toTarget.y = 0f;
        Vector3 end = ArenaBounds.Clamp(opponent.position - toTarget.normalized * landOffset);
        end.y = start.y;

        float t = 0f;
        while (t < glideTime)
        {
            t += Time.fixedDeltaTime;
            motor.Body.MovePosition(Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / glideTime)));
            FaceOpponent();
            yield return new WaitForFixedUpdate();
        }
        if (a.vfx) a.vfx.Stop();

        // 3. land + slam
        if (animator != null && triggers.Contains("Land")) animator.SetTrigger("Land");
        yield return MoveModelY(modelBaseY + flyHeight, modelBaseY, landTime);
        motor.SetKinematic(false);
        HitRadius(a.damage, slamRadius, a.knockback);
        if (CameraRig.Instance) CameraRig.Instance.Shake(0.2f, slamShake);

        yield return Wait(a.recovery);
        End();
    }

    // ---------- Damage helpers ----------

    void HitCone(float damage, float range, float coneAngle, float knockback)
    {
        var done = new HashSet<Health>();
        foreach (var col in Physics.OverlapSphere(transform.position, range))
        {
            var h = col.GetComponentInParent<Health>();
            if (h == null || h == health || h.IsDead || !done.Add(h)) continue;

            Vector3 to = h.transform.position - transform.position; to.y = 0f;
            if (Vector3.Angle(transform.forward, to) > coneAngle * 0.5f) continue;
            Apply(h, damage, to.normalized * knockback);
        }
    }

    void HitRadius(float damage, float radius, float knockback)
    {
        var done = new HashSet<Health>();
        foreach (var col in Physics.OverlapSphere(transform.position, radius))
        {
            var h = col.GetComponentInParent<Health>();
            if (h == null || h == health || h.IsDead || !done.Add(h)) continue;
            Vector3 away = h.transform.position - transform.position; away.y = 0f;
            Apply(h, damage, away.normalized * knockback);
        }
    }

    static void Apply(Health target, float damage, Vector3 knockback)
    {
        target.TakeDamage(damage);
        if (knockback.sqrMagnitude > 0.01f && target.TryGetComponent(out DragonMotor m)) m.Knockback(knockback);
    }

    // ---------- Utilities ----------

    void Begin(Ability a)
    {
        busy = true;
        a.readyTime = Time.time + a.cooldown;
        motor.Locked = true;
        FaceOpponent();
        if (animator != null && triggers.Contains(a.animTrigger)) animator.SetTrigger(a.animTrigger);
    }

    void End()
    {
        busy = false;
        motor.Locked = false;
    }

    void FaceOpponent()
    {
        if (opponent == null) return;
        Vector3 d = opponent.position - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d);
    }

    static IEnumerator Wait(float s) { if (s > 0f) yield return new WaitForSeconds(s); }

    IEnumerator MoveModelY(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetModelY(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration)));
            yield return null;
        }
        SetModelY(to);
    }

    void SetModelY(float y)
    {
        if (model == null) return;
        Vector3 p = model.localPosition; p.y = y; model.localPosition = p;
    }

    void OnDied(Health _)
    {
        StopAllCoroutines();
        busy = false;
        motor.SetKinematic(false);
        motor.Locked = true;
        motor.Move(Vector3.zero);
        SetModelY(modelBaseY);

        if (model != null) model.localRotation = modelBaseRot;

        foreach (var a in Abilities) if (a.vfx) a.vfx.Stop();
        if (animator != null && triggers.Contains("Die")) animator.SetTrigger("Die");
    }
}
