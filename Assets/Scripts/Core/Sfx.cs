using UnityEngine;

/// <summary>Tiny 2D sound helper so attack/hit sounds are audible from the high camera.</summary>
public static class Sfx
{
    static AudioSource source;

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (source == null)
        {
            var go = new GameObject("SfxPlayer");
            Object.DontDestroyOnLoad(go);
            source = go.AddComponent<AudioSource>();
            source.spatialBlend = 0f;
        }
        source.PlayOneShot(clip, volume);
    }
}
