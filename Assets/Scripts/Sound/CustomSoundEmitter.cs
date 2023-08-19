using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CustomSound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
}

public class CustomSoundEmitter : MonoBehaviour
{
    [SerializeField] CustomSound[] sounds;
    [SerializeField] bool mute;
    [SerializeField] [Range(1, 10)] int emitPeriodInCalls;

    public bool isPlaying => numSources > 0;

    private Dictionary<string, CustomSound> soundMap = new ();
    private int numSources = 0;
    private int numCalls = 0;

    void Awake()
    {
        foreach (var sound in sounds)
            soundMap.TryAdd(sound.name, sound);
        if (emitPeriodInCalls == 0)
            emitPeriodInCalls = 1;
    }

    public void EmitSound (string clipName)
    {
        numCalls++;

        if (mute || numCalls % emitPeriodInCalls != 0)
            return;

        bool found = soundMap.TryGetValue(clipName, out CustomSound sound);
        if (!found)
            throw new KeyNotFoundException($"{gameObject.name}'s CustomSoundEmitter does not have an audio clip with the given name: {clipName}");
        
        numSources++;
        numCalls = 0;
        
        GameObject go = new GameObject($"{clipName}Source");
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        var source = go.AddComponent<AudioSource>();
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.Play();
        if (source != null && gameObject.activeSelf)
            StartCoroutine(WaitForEndAndDestroy(source));
    }

    private IEnumerator WaitForEndAndDestroy (AudioSource source)
    {
        yield return new WaitUntil(() => source == null || !source.isPlaying);
        Destroy(source.gameObject);
        numSources--;
    }
}
