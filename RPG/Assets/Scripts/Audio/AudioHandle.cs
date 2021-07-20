using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandle 
{
    public string AudioName { 
        get => clip.name; }
    public AudioClip clip;

    public float volume;

    public float fadeDuration;
    public float delay;

    public bool ShouldFadeOut;
    public bool ShouldFadeIn;
    public bool isValid;
    public bool IsFading { get; private set; }

    public Action OnCompleted;

    private float origVolume;
    private Coroutine fadeInCoroutineHandle;
    private Coroutine fadeOutCoroutineHandle;
    public AudioHandle()
	{
        origVolume = 1.0f;
        fadeInCoroutineHandle = null;
        fadeOutCoroutineHandle = null;
        volume = 1;
        fadeDuration = 1;
        delay = 0;
        ShouldFadeIn = false;
        ShouldFadeOut = false; 
        fadeDuration = 1;
        isValid = true;
    }

    public void Init()
    {
        origVolume = volume;
    }

	public IEnumerator fadeIn()
    {
        IsFading = true;
        volume = 0;
        var source = ServiceManager.Get<AudioManager>().GetAudioSourceOfHandle(AudioName);
        while (volume < origVolume)
        {
            
            volume += (1.0f / fadeDuration) * Time.fixedDeltaTime;
            source.volume = volume;
            yield return null;
        }

        IsFading = false;
        volume = origVolume;
        source.volume = volume;
        fadeInCoroutineHandle = null;
    }

    public IEnumerator fadeOut()
    {
        IsFading = true;
        var source = ServiceManager.Get<AudioManager>().GetAudioSourceOfHandle(AudioName);
        while (volume > 0.0f)
        {
            volume -= (1.0f / fadeDuration) * Time.fixedDeltaTime;
            source.volume = volume;
            yield return null;
        }

        IsFading = false;
        volume = 0.0f;
        source.volume = volume;
        source.Stop();
        source.clip = null;
        fadeOutCoroutineHandle = null;
    }

    public void Reset()
    {
        volume = 1.0f;
        origVolume = 1.0f;
        ServiceManager.Get<AudioManager>().GetAudioSourceOfHandle(AudioName).volume = volume;
        fadeDuration = 1.0f;
        if (fadeInCoroutineHandle != null)
        {
            fadeInCoroutineHandle = null;
        }

        if (fadeOutCoroutineHandle != null)
        {
            fadeOutCoroutineHandle = null;
        }
    }
    

    
}
