using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermutableSoundPlayer : MonoBehaviour
{
    //TODO: this is a temporary solution, should be replaced with a more generic system
    public static event System.Action GlobalVolumeUpdate;
    public static void UpdateGlobalVolume() => GlobalVolumeUpdate?.Invoke();
    
    [SerializeField] CustomSound[] sounds;

    private Dictionary<string, AudioSource> sourceMap = new ();
    private string selectedName;
    private AudioSource selectedSource;

    private bool _isPlaying = false;
    
    void Awake()
    {
        foreach (var sound in sounds)
            if (!sourceMap.ContainsKey(sound.name))
            {
                GameObject go = new GameObject(sound.name + "Source");
                go.transform.parent = transform;
                go.transform.localPosition = Vector3.zero;
                AudioSource source = go.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.loop = true;
                source.mute = true;
                source.time = 0f;

                sourceMap.TryAdd(sound.name, source);
            }
        
        UpdateVolume();
        GlobalVolumeUpdate += UpdateVolume;
    }
    
    void OnDestroy() => GlobalVolumeUpdate -= UpdateVolume;

    private void UpdateVolume ()
    {
        var volume = 1f;
        if (PlayerPrefs.HasKey("SFXVolume"))
            volume = PlayerPrefs.GetFloat("SFXVolume");
        foreach (var source in sourceMap.Values)
            source.volume = volume;
    }

    public void SelectClip (string name)
    {
        if (selectedName == name)
            return;

        bool found = sourceMap.TryGetValue(name, out AudioSource source);
        if (!found)
            throw new KeyNotFoundException($"{gameObject.name}'s PermutableSoundPlayer does not have an audio clip with the given name: {name}");
        
        UnselectClip();
        
        selectedName = name;
        selectedSource = source;
        selectedSource.mute = false;
    }

    public void UnselectClip ()
    {
        if (selectedSource != null)
        {
            selectedSource.mute = true;
            selectedSource = null;
            selectedName = "";
        }
    }

    public void PlayAll()
    {
        if (_isPlaying)
            return;
        _isPlaying = true;
        
        foreach (var source in sourceMap.Values) source.Play();
    }

    public void Stop()
    {
        if (!_isPlaying)
            return;
        _isPlaying = false;
        
        foreach (var source in sourceMap.Values)
        {
            source.Stop();
            source.time = 0f;

            selectedSource = null;
        }
    }
}
