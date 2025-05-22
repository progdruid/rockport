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
    //TODO: this is a temporary solution, should be replaced with a more generic system
    public static event System.Action GlobalVolumeUpdate;
    public static void UpdateGlobalVolume() => GlobalVolumeUpdate?.Invoke();

    
    [SerializeField] string[] startSounds;
    [SerializeField] CustomSound[] sounds;
    [SerializeField] bool mute;
    [SerializeField] [Range(1, 10)] int emitPeriodInCalls;

    public bool isPlaying => numSources > 0;

    private Dictionary<string, CustomSound> soundMap = new ();
    private int numSources = 0;
    private int numCalls = 0;

    private float _emitterVolume = 1f;
    
    void Awake()
    {
        foreach (var sound in sounds)
            soundMap.TryAdd(sound.name, sound);

        if (emitPeriodInCalls < 1)
            emitPeriodInCalls = 1;
        
        GlobalVolumeUpdate += UpdateVolume;
    }

    void Start()
    {
        UpdateVolume();
        foreach (var sound in startSounds)
            EmitSound(sound);
    }
    
    void OnDestroy() => GlobalVolumeUpdate -= UpdateVolume;

    private void UpdateVolume ()
    {
        _emitterVolume = 1f;
        if (PlayerPrefs.HasKey("SFXVolume"))
            _emitterVolume = PlayerPrefs.GetFloat("SFXVolume");
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
        source.volume = sound.volume * _emitterVolume;
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
