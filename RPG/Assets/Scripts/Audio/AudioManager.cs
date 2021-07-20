using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> AvailableSources = new List<AudioSource>();
    public AudioSource BackgroundAudio;
    public Dictionary<string, AudioHandle> Sounds = new Dictionary<string, AudioHandle>();
    public AudioSource[] AllSources = new AudioSource[8];
    private bool BackgroundPaused = false;
	void Awake()
	{
		ServiceManager.Register(this);
		BackgroundAudio = gameObject.AddComponent<AudioSource>();
		for (int i = 0; i < 8; i++)
		{
			AllSources[i] = gameObject.AddComponent<AudioSource>();
			AvailableSources.Add(AllSources[i]);
		}	
	}


	void OnDestroy()    
    {
		foreach(var source in AllSources)
		{
			source.Stop();
		}
		BackgroundAudio.Stop();
		AvailableSources.Clear();
		Array.Clear(AllSources, 0, AllSources.Length);
		Sounds.Clear();
        ServiceManager.Unregister(this);
    }

    private void Update()
    {
        foreach (AudioSource source in AllSources)
        {
			if (source.isPlaying && AvailableSources.Contains(source))
				AvailableSources.Remove(source);
			else if (!source.isPlaying && !AvailableSources.Contains(source))
			{
				Sounds[source.clip.name].OnCompleted?.Invoke();
				AvailableSources.Add(source);
			}
			
			if(!AvailableSources.Contains(source))
			{	
				if((source.clip.length - source.time ) <= Sounds[source.clip.name].fadeDuration && !Sounds[source.clip.name].IsFading && Sounds[source.clip.name].ShouldFadeOut)
				{
					StartCoroutine(Sounds[source.clip.name].fadeOut());
				}
			}
        }

        if (!BackgroundPaused && !BackgroundAudio.isPlaying && BackgroundAudio.clip != null)
        {
            if (BackgroundAudio.clip != null)
                BackgroundAudio.Play();
        }

    }

	/// <summary>
	/// To get the handles clip use AudioManager.LoadAudioFromResources("AudioName")
	/// </summary>
	/// <param name="handle"></param>
	public void AddAudio(AudioHandle handle)
	{
		if (handle.clip != null)
		{
			if (!Sounds.ContainsValue(handle))
			{
				Sounds.Add(handle.clip.name, handle);
				handle.Init();
			}
			else
			{
				LogManager.LogError("Sounds list already contains specified sound!");
			}
		}
		else
			LogManager.LogError("AudioHandle's clip is null!");
	}

	public void SetBackgroundAudio(string SoundName)
	{
		if (Sounds.ContainsKey(SoundName))
		{
			if (Sounds[SoundName].isValid)
			{
				BackgroundAudio.clip = Sounds[SoundName].clip;
				BackgroundAudio.volume = Sounds[SoundName].volume;
				BackgroundAudio.Play();
				if (Sounds[SoundName].ShouldFadeIn && Sounds[SoundName].fadeDuration > 0 && !Sounds[SoundName].IsFading)
					StartCoroutine(Sounds[SoundName].fadeIn());
			}
			else
				LogManager.LogWarn($"{SoundName} is currently not valid.");
		}
		else if (SoundName == null)
			BackgroundAudio.clip = null;
		else
			LogManager.LogError("Could not play specified sound because it does not exist in the sounds array.");

	}

	public void PauseBackground()
	{
		BackgroundPaused = true;
		BackgroundAudio.Pause();
	}

	public void UnPauseBackground(bool fadeIn = false)
	{
		BackgroundAudio.UnPause();
		if (fadeIn)
			Sounds[BackgroundAudio.clip.name].fadeIn();
		BackgroundPaused = false;
	}

	

	public AudioHandle PlaySound(string SoundName)
	{
		if (Sounds[SoundName].isValid)
		{
			if (Sounds.ContainsValue(Sounds[SoundName]))
			{
				
					AvailableSources[0].clip = Sounds[SoundName].clip;
					AvailableSources[0].volume = Sounds[SoundName].volume;
					AvailableSources[0].PlayDelayed(Sounds[SoundName].delay);
					
				if (Sounds[SoundName].ShouldFadeIn && Sounds[SoundName].fadeDuration > 0 && !Sounds[SoundName].IsFading)
						StartCoroutine(Sounds[SoundName].fadeIn());
				return Sounds[SoundName];
			}
			else
				LogManager.LogError("Could not play specified sound because it does not exist in the sounds array.");
		}
		else
			LogManager.LogWarn($"{SoundName} is currently not valid.");
		return null;
	}

	public void ForceFadeOut(string SoundName)
	{
		if (Sounds[SoundName].isValid)
		{
			if (Sounds.ContainsKey(SoundName))
				StartCoroutine(Sounds[SoundName].fadeOut());
			else
				LogManager.LogError("Could not play specified sound because it does not exist in the sounds array.");
		}
		else
			LogManager.LogWarn($"{SoundName} is currently not valid.");
	}

	public AudioSource GetAudioSourceOfHandle(string AudioName)
	{
		foreach(AudioSource source in AllSources)
		{
			if(source.clip == Sounds[AudioName].clip)
			{
				return source;
			}
		}
		if (BackgroundAudio.clip == Sounds[AudioName].clip)
			return BackgroundAudio;

		return null;
	}
	
}