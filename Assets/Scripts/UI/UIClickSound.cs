using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a sound when this Button is clicked. Uses the Sfx helper, whose AudioSource survives scene loads,
/// so the click is still heard when the button reloads the scene (e.g. Restart).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [Range(0f, 1f)] [SerializeField] float volume = 1f;

    void Awake() => GetComponent<Button>().onClick.AddListener(PlayClick);

    void PlayClick() => Sfx.Play(clip, volume);
}
