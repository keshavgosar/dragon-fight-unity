using System;
using UnityEngine;

/// <summary>Data for one ability. Player and AI share this, so they obey identical damage/cooldowns.</summary>
[Serializable]
public class Ability
{
    public string name = "Ability";
    public Sprite icon;
    [Min(0f)] public float damage = 20f;   // total damage (fire: spread over its ticks)
    [Min(0f)] public float cooldown = 3f;
    public float range = 3f;               // reach (fly: max distance it can be launched from)
    [Range(10f, 360f)] public float coneAngle = 90f;
    public float windup = 0.3f;
    public float activeTime = 0.2f;
    public float recovery = 0.3f;
    public float knockback = 0f;
    public string animTrigger;
    public ParticleSystem vfx;
    public AudioClip sfx;

    [NonSerialized] public float readyTime;

    public float Remaining => Mathf.Max(0f, readyTime - Time.time);
    public float Normalized => cooldown > 0f ? Remaining / cooldown : 0f; // 1 = just used, 0 = ready
    public bool IsReady => Time.time >= readyTime;
}
