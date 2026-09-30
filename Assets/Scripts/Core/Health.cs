using System;
using UnityEngine;

/// <summary>Plain health container. Everything else (UI, VFX, win check) listens to its events.</summary>
public class Health : MonoBehaviour
{
    [SerializeField] float maxHealth = 200f;

    public float Current { get; private set; }
    public float Max => maxHealth;
    public bool IsDead => Current <= 0f;

    public event Action<float, float> Changed; // current, max
    public event Action<float> Damaged;        // amount
    public event Action<Health> Died;

    void Awake() => Current = maxHealth;

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        Current = Mathf.Max(0f, Current - amount);
        Damaged?.Invoke(amount);
        Changed?.Invoke(Current, maxHealth);
        if (IsDead) Died?.Invoke(this);
    }
}
