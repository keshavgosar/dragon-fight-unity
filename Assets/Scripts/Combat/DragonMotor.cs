using UnityEngine;

/// <summary>Movement + knockback for a dragon. Controllers (player / AI) only call Move().</summary>
[RequireComponent(typeof(Rigidbody))]
public class DragonMotor : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float turnSpeed = 720f;
    [SerializeField] float knockbackDecay = 25f;
    [SerializeField] Animator animator;

    Rigidbody rb;
    Vector3 desired;
    Vector3 knock;
    bool hasSpeedParam;
    static readonly int SpeedHash = Animator.StringToHash("Speed");

    public bool Locked { get; set; }   // true while casting / dead
    public Rigidbody Body => rb;

    Vector3 Velocity
    {
#if UNITY_6000_0_OR_NEWER
        get => rb.linearVelocity;
        set => rb.linearVelocity = value;
#else
        get => rb.velocity;
        set => rb.velocity = value;
#endif
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (animator != null)
            foreach (var p in animator.parameters)
                if (p.nameHash == SpeedHash) hasSpeedParam = true;
    }

    public void Move(Vector3 dir) => desired = Vector3.ClampMagnitude(new Vector3(dir.x, 0f, dir.z), 1f);

    public void Knockback(Vector3 impulse) => knock = new Vector3(impulse.x, 0f, impulse.z);

    public void SetKinematic(bool value)
    {
        if (!value) rb.isKinematic = false;
        if (!value) Velocity = Vector3.zero;
        else rb.isKinematic = true;
    }

    void Update()
    {
        if (hasSpeedParam)
            animator.SetFloat(SpeedHash, Locked ? 0f : desired.magnitude, 0.1f, Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return; // flying: DragonCombat drives the body directly

        Vector3 v = Locked ? Vector3.zero : desired * moveSpeed;
        v += knock;
        knock = Vector3.MoveTowards(knock, Vector3.zero, knockbackDecay * Time.fixedDeltaTime);
        v.y = Velocity.y; // keep gravity
        Velocity = v;

        if (!Locked && desired.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(desired);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.fixedDeltaTime);
        }

        rb.position = ArenaBounds.Clamp(rb.position);
    }
}
