using UnityEngine;

/// <summary>
/// Keeps this object (e.g. the FirePoint) on a bone such as the head, so effects follow the animation,
/// while keeping the dragon's facing so the flames always shoot the way the damage cone points.
/// </summary>
public class FollowBone : MonoBehaviour
{
    [SerializeField] Transform bone;          // e.g. the Head bone
    [SerializeField] Transform owner;         // the dragon root (Red / Blue)
    [SerializeField] Vector3 localOffset = new Vector3(0f, 0f, 1f); // offset from the bone, in the dragon's space

    void LateUpdate()
    {
        if (bone == null || owner == null) return;
        transform.position = bone.position + owner.TransformVector(localOffset);
        transform.rotation = owner.rotation;
    }
}
