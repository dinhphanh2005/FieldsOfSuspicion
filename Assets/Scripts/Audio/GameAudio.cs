using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudio : MonoBehaviour
{
    static GameAudio instance;
    AudioSource musicA, musicB, sfx;
    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    string currentMusic;
    Coroutine fading;

    public const float MusicVolume = 0.45f;
    public const float SfxVolume = 0.8f;

    public static void Ensure(GameObject host)
    {
        if (instance) return;
        instance = host.AddComponent<GameAudio>();
        instance.musicA = instance.NewSource(true);
        instance.musicB = instance.NewSource(true);
        instance.sfx = instance.NewSource(false);
    }

    AudioSource NewSource(bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0;
        return s;
    }

    AudioClip Clip(string name)
    {
        if (!clips.TryGetValue(name, out var c))
        {
            c = Resources.Load<AudioClip>("Audio/" + name);
            if (!c) Debug.LogWarning("Thiếu âm thanh Audio/" + name);
            clips[name] = c;
        }
        return c;
    }

    public static void Sfx(string name, float volume = 1f)
    {
        if (!instance) return;
        var c = instance.Clip("sfx_" + name);
        if (c) instance.sfx.PlayOneShot(c, SfxVolume * volume);
    }

    public static void Music(string name, float fade = 1.2f)
    {
        if (!instance || name == instance.currentMusic) return;
        instance.currentMusic = name;
        if (instance.fading != null) instance.StopCoroutine(instance.fading);
        instance.fading = instance.StartCoroutine(instance.Crossfade(name == null ? null : instance.Clip("music_" + name), fade));
    }

    IEnumerator Crossfade(AudioClip next, float time)
    {
        var from = musicA.isPlaying ? musicA : musicB;
        var to = from == musicA ? musicB : musicA;
        if (next)
        {
            to.clip = next;
            to.volume = 0;
            to.Play();
        }
        float startFrom = from.volume;
        for (float t = 0; t < time; t += Time.unscaledDeltaTime)
        {
            float k = t / time;
            from.volume = Mathf.Lerp(startFrom, 0, k);
            if (next) to.volume = Mathf.Lerp(0, MusicVolume, k);
            yield return null;
        }
        from.Stop();
        from.volume = 0;
        if (next) to.volume = MusicVolume;
        fading = null;
    }
}
