using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandle : MonoBehaviour
{
    AudioManager manager;
	private bool BackgroundPaused = false;
	private void Awake()
	{
        ServiceManager.Register(this);
	}

	void Start()
    {
        manager = ServiceManager.Get<AudioManager>();   
    }

	private void Update()
	{
		foreach (AudioSource source in manager.AllSources)
		{
			if (source.isPlaying && manager.AvailableSources.Contains(source))
				manager.AvailableSources.Remove(source);
			else if (!source.isPlaying && !manager.AvailableSources.Contains(source))
				manager.AvailableSources.Add(source);
		}

		if (!BackgroundPaused && !manager.BackgroundAudio.isPlaying && manager.BackgroundAudio.clip != null)
		{
			if(manager.BackgroundAudio.clip != null)
				manager.BackgroundAudio.Play();
		}
		
	}

	public void PauseBackground()
	{
		BackgroundPaused = true;
		manager.BackgroundAudio.Pause();		
	}

	public void UnPauseBackground()
	{
		manager.BackgroundAudio.UnPause();
		BackgroundPaused = false;
	}

	public AudioClip AddAudio(string clip)
    {

        if (!manager.Sounds.ContainsKey(clip))
        {
			if (Resources.Load<AudioClip>("Sounds/" + clip) != null)
			{
				manager.Sounds.Add(clip, Resources.Load<AudioClip>("Sounds/" + clip));
				return Resources.Load<AudioClip>("Sounds/" + clip);
			}
			else
				LogManager.LogError("Sound not found in sounds directory");
        }
        else
        {
            LogManager.LogError("Sounds list already contains specified sound!");
        }
		return null;
    }

	public void SetBackgroundAudio(string SoundName)
	{
		if (manager.Sounds.ContainsKey(SoundName))
		{
			manager.BackgroundAudio.clip = manager.Sounds[SoundName];
			manager.BackgroundAudio.Play();
		}
		else
		{
			if (Resources.Load<AudioClip>("Sounds/" + SoundName) != null)
			{
				AddAudio(SoundName);
				manager.AvailableSources[0].clip = manager.Sounds[SoundName];
				manager.BackgroundAudio.Play();
			}
			else
				LogManager.LogError("Could not play specified sound because it does not exist in the sounds array, nor could it be found in the Resources/Sounds directory.");
		}
	}

    public void PlaySound(string SoundName, float volume = 1.0f, float delay = 0)
	{
		if (manager.Sounds.ContainsKey(SoundName))
		{
			manager.AvailableSources[0].clip = manager.Sounds[SoundName];
			manager.AvailableSources[0].volume = volume;
			manager.AvailableSources[0].PlayDelayed(delay);
			
		}
		else
		{
			if (Resources.Load<AudioClip>("Sounds/" + SoundName) != null)
			{
				AddAudio(SoundName);
				manager.AvailableSources[0].clip = manager.Sounds[SoundName];
				manager.AvailableSources[0].volume = volume;
				manager.AvailableSources[0].PlayDelayed(delay);
			}
			else
				LogManager.LogError("Could not play specified sound because it does not exist in the sounds array, nor could it be found in the Resources/Sounds directory.");
		}
	}

}
