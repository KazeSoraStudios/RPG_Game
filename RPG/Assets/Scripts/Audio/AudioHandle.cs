using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandle 
{
    public string AudioName { get; private set; }
    public AudioClip clip;

    public float volume = 1.0f;
    public float fadeDuration = 1.0f;
    public float Delay = 0;

    public bool ShouldFadeOut = false;
    public bool ShouldFadeIn = false;
    public bool isValid = true;
    public bool IsFading { get; private set; }

    private Coroutine fadeInCoroutineHandle = null;
    private Coroutine fadeOutCoroutineHandle = null;

    public void Init()
	{
        AudioName = clip.name;
	}
        
    public IEnumerator fadeIn()
    {
        IsFading = true;
        volume = 0;
        var source = ServiceManager.Get<AudioManager>().GetAudioSourceOfHandle(AudioName);
        while (volume < 1.0f)
        {
            
            volume += (1.0f / fadeDuration) * Time.fixedDeltaTime;
            source.volume = volume;
            yield return null;
        }

        IsFading = false;
        volume = 1.0f;
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
        fadeOutCoroutineHandle = null;
    }

    public void Reset()
    {
        volume = 1.0f;
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