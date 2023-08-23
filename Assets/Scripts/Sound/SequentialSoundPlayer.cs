using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialSoundPlayer : MonoBehaviour
{
    [SerializeField] CustomSound[] soundSequence;
    [SerializeField] bool loopLast;
    [SerializeField] bool playOnStart;
    [SerializeField] float fadeOutTime;

    private AudioSource source;

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

        source = go.AddComponent<AudioSource>();

        if (playOnStart)
            StartPlaying();

        initialized = true;
    }

    public void StartPlaying()
    {
        if (!playRoutineRunning)
            StartCoroutine(StartPlayingRoutine());
    }

    private IEnumerator StartPlayingRoutine ()
    {
        if (!initialized)
            Init();

        playRoutineRunning = true;
        stopped = false;

        int nextClipIndex = 0;
        source.loop = false;

        while (nextClipIndex < soundSequence.Length - 1 && !stopped)
        {
            PlaySoundFromSequence(nextClipIndex);
            nextClipIndex++;
            yield return new WaitWhile(() => source.isPlaying && !stopped);
        }

        if (!stopped)
        {
            source.loop = true;
            PlaySoundFromSequence(nextClipIndex);
        }

        playRoutineRunning = false;
    }

    private void PlaySoundFromSequence (int index)
    {
        source.clip = soundSequence[index].clip;
        source.time = 0;
        source.volume = soundSequence[index].volume;

        source.Play();
    }

    public IEnumerator StopPlaying ()
    {
        //fade out
        float initTime = Time.time;
        float initVolume = source.volume;
        while (source.volume != 0 && fadeOutTime != 0)
        {
            float t = 1f - (Time.time - initTime) / fadeOutTime;
            source.volume = initVolume * t;
            yield return new WaitForEndOfFrame();
        }

        //stop
        source.Stop();
        source.loop = false;
        stopped = true;
    }
}
