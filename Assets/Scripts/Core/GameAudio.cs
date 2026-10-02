using System.Collections;
using UnityEngine;

/// <summary>
/// Background music (looping) plus win / lose stingers. Listens to both dragons' Died events, so it needs
/// no changes to GameManager. Put it on any active object in the scene (e.g. an empty "AudioManager").
/// </summary>
public class GameAudio : MonoBehaviour
{
    [Header("Who is who")]
    [SerializeField] Health player;   // the dragon with PlayerController
    [SerializeField] Health enemy;    // the dragon with EnemyAI

    [Header("Clips")]
    [SerializeField] AudioClip backgroundMusic;
    [SerializeField] AudioClip winClip;
    [SerializeField] AudioClip loseClip;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] float musicVolume = 0.35f;
    [Range(0f, 1f)] [SerializeField] float resultVolume = 1f;
    [SerializeField] float musicFadeOut = 0.6f;

    AudioSource music;
    AudioSource result;
    bool finished;

    void Awake()
    {
        music = CreateSource(loop: true);
        result = CreateSource(loop: false);

        music.clip = backgroundMusic;
        music.volume = musicVolume;
        if (backgroundMusic != null) music.Play();

        player.Died += OnDied;
        enemy.Died += OnDied;
    }

    void OnDestroy()
    {
        if (player) player.Died -= OnDied;
        if (enemy) enemy.Died -= OnDied;
    }

    AudioSource CreateSource(bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0f; // 2D: same volume everywhere
        return s;
    }

    void OnDied(Health loser)
    {
        if (finished) return;
        finished = true;

        AudioClip clip = loser == enemy ? winClip : loseClip; // enemy died = player won
        StartCoroutine(FadeOutMusic());

        if (clip != null)
        {
            result.clip = clip;
            result.volume = resultVolume;
            result.Play();
        }
    }

    // Unscaled time so the fade still works during the slow-motion on the killing blow.
    IEnumerator FadeOutMusic()
    {
        float start = music.volume;
        float t = 0f;
        while (t < musicFadeOut)
        {
            t += Time.unscaledDeltaTime;
            music.volume = Mathf.Lerp(start, 0f, t / musicFadeOut);
            yield return null;
        }
        music.Stop();
    }
}
