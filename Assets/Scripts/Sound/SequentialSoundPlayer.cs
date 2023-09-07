using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialSoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public struct SequenceSound
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Min(0f)] public float crossFadeTime;
    }

    [SerializeField] SequenceSound[] soundSequence;
    [SerializeField] bool loopLast;
    [SerializeField] bool playOnStart;
    [SerializeField] float fadeOutTime;

    private AudioSource currentSource;
    private AudioSource nextSource;

    public bool stopped { get; private set; } = false;
    private bool playRoutineRunning = false;
    private bool initialized = false;

    void Start()
    {
        if (!initialized)
            Init();
    }

    private void Init()
    {
        GameObject go = new GameObject("SequentialSoundSource");
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;

        currentSource = go.AddComponent<AudioSource>();
        nextSource = go.AddComponent<AudioSource>();

        initialized = true;

        if (playOnStart)
            StartPlaying();
    }

    public void StartPlaying()
    {
        if (!playRoutineRunning && soundSequence.Length > 0)
            StartCoroutine(PlayingRoutine());
    }

    private IEnumerator PlayingRoutine ()
    {
        if (!initialized)
            Init();

        playRoutineRunning = true;
        stopped = false;

        int currentIndex = 0;
        int nextIndex = 0;

        PlaySoundFromSequence(currentSource, currentIndex);

        while (!stopped && nextIndex != -1)
        {
            //define next index
            if (currentIndex + 1 < soundSequence.Length)
                nextIndex = currentIndex + 1;
            else if (loopLast && currentIndex + 1 >= soundSequence.Length)
                nextIndex = currentIndex;
            else
                nextIndex = -1;

            //wait for cross-fade time and start playing next track
            if (currentSource.clip.length - currentSource.time > soundSequence[nextIndex].crossFadeTime)
                yield return new WaitWhile(() => currentSource.clip.length - currentSource.time > soundSequence[nextIndex].crossFadeTime);
            PlaySoundFromSequence(nextSource, nextIndex);
            
            //wait for current track to stop playing
            //then prepare for the next iteration by moving values
            if (currentSource.isPlaying)
                yield return new WaitWhile(() => currentSource.isPlaying);
            currentSource.clip = null;
            currentIndex = nextIndex;
            (nextSource, currentSource) = (currentSource, nextSource);
        }

        playRoutineRunning = false;
    }

    private void PlaySoundFromSequence (AudioSource source, int index)
    {
        source.clip = soundSequence[index].clip;
        source.volume = soundSequence[index].volume;
        source.time = 0;
        source.loop = false;

        source.Play();
    }

    public IEnumerator StopPlaying ()
    {
        //fade out
        float initTime = Time.time;
        float initCurrentVolume = currentSource.volume;
        float initNextVolume = nextSource.volume;
        float t = 1f;
        while (t > 0 && fadeOutTime != 0)
        {
            t = 1f - (Time.time - initTime) / fadeOutTime;
            currentSource.volume = initCurrentVolume * t;
            nextSource.volume = initNextVolume * t;
            yield return new WaitForEndOfFrame();
        }

        //stop
        currentSource.Stop();
        nextSource.Stop();
        stopped = true;
    }
}
