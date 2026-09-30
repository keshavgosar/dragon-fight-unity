using UnityEngine;

/// <summary>Readable 3-state AI: Idle -> Chase -> Attack. Uses the same DragonCombat (and cooldowns) as the player.</summary>
[RequireComponent(typeof(DragonMotor), typeof(DragonCombat))]
public class EnemyAI : MonoBehaviour
{
    enum State { Idle, Chase, Attack }

    [SerializeField] Transform target;
    [SerializeField] float startDelay = 1.5f;
    [SerializeField] float thinkInterval = 0.3f;
    [Range(0f, 1f)] [SerializeField] float useAbilityChance = 0.85f;
    [SerializeField] float flyMinDistance = 6f;

    DragonMotor motor;
    DragonCombat combat;
    State state = State.Idle;
    float nextThink;
    int strafeDir = 1;
    float nextStrafeFlip;

    void Awake()
    {
        motor = GetComponent<DragonMotor>();
        combat = GetComponent<DragonCombat>();
    }

    void Update()
    {
        if (GameManager.IsOver || combat.Health.IsDead) { motor.Move(Vector3.zero); return; }

        float dist = FlatDistance();
        float engage = combat.fire.range * 0.9f;

        switch (state)
        {
            case State.Idle:
                motor.Move(Vector3.zero);
                if (Time.timeSinceLevelLoad >= startDelay) state = State.Chase;
                break;

            case State.Chase:
                motor.Move(DirToTarget());
                if (dist <= engage) state = State.Attack;
                break;

            case State.Attack:
                AttackState(dist, engage);
                break;
        }
    }

    void AttackState(float dist, float engage)
    {
        if (combat.IsBusy) return;
        if (dist > engage * 1.3f) { state = State.Chase; return; }

        if (Time.time >= nextThink)
        {
            nextThink = Time.time + thinkInterval;
            int pick = PickAbility(dist);
            if (pick >= 0 && Random.value <= useAbilityChance && combat.TryUse(pick)) return;
        }

        // Nothing ready: circle the player and hold a mid distance so it never stands still.
        if (Time.time >= nextStrafeFlip)
        {
            strafeDir = Random.value > 0.5f ? 1 : -1;
            nextStrafeFlip = Time.time + Random.Range(1.5f, 3f);
        }
        Vector3 toTarget = DirToTarget();
        Vector3 tangent = Vector3.Cross(Vector3.up, toTarget) * strafeDir;
        float preferred = (combat.tail.range + combat.fire.range) * 0.5f;
        float radial = Mathf.Clamp((dist - preferred) / 2f, -1f, 1f);
        motor.Move(tangent * 0.8f + toTarget * radial);
    }

    /// <summary>Distance-based choice: tail when close, fire at mid range, fly to close the gap.</summary>
    int PickAbility(float dist)
    {
        var a = combat.Abilities;
        if (dist <= a[DragonCombat.Tail].range && a[DragonCombat.Tail].IsReady) return DragonCombat.Tail;
        if (dist > a[DragonCombat.Tail].range && dist <= a[DragonCombat.Fire].range && a[DragonCombat.Fire].IsReady) return DragonCombat.Fire;
        if (dist >= flyMinDistance && dist <= a[DragonCombat.Fly].range && a[DragonCombat.Fly].IsReady) return DragonCombat.Fly;
        return -1;
    }

    Vector3 DirToTarget()
    {
        Vector3 d = target.position - transform.position; d.y = 0f;
        return d.normalized;
    }

    float FlatDistance()
    {
        Vector3 d = target.position - transform.position; d.y = 0f;
        return d.magnitude;
    }
}
