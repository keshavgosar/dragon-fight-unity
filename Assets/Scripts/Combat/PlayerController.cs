using UnityEngine;

/// <summary>WASD movement + 1/2/3 abilities. Includes a short input buffer so presses during a
/// recovery window still fire the moment the dragon is free (feels more responsive).</summary>
[RequireComponent(typeof(DragonMotor), typeof(DragonCombat))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float bufferTime = 0.3f;

    DragonMotor motor;
    DragonCombat combat;
    int buffered = -1;
    float bufferExpires;

    void Awake()
    {
        motor = GetComponent<DragonMotor>();
        combat = GetComponent<DragonCombat>();
    }

    void Update()
    {
        if (GameManager.IsOver || combat.Health.IsDead) { motor.Move(Vector3.zero); return; }

        motor.Move(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")));

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Queue(DragonCombat.Fire);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Queue(DragonCombat.Tail);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Queue(DragonCombat.Fly);

        if (buffered >= 0)
        {
            if (Time.time > bufferExpires) buffered = -1;
            else if (combat.TryUse(buffered)) buffered = -1;
        }
    }

    void Queue(int index)
    {
        buffered = index;
        bufferExpires = Time.time + bufferTime;
    }
}
