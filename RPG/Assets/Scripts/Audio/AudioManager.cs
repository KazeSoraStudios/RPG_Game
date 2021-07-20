using UnityEngine;
using System;
using System.Collections.Generic;

namespace RPG_Audio
{
	public class AudioManager : MonoBehaviour
	{
		public List<AudioSource> AvailableSources = new List<AudioSource>();
		public AudioSource BackgroundAudio;
		public AudioSource[] AllSources = new AudioSource[8];

		private AudioLibrary Library;
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
			Library.ClearLibrary();
			ServiceManager.Unregister(this);
		}

		public void LoadLibrary(Dictionary<string, RPG_GameData.AudioData> audioData)
		{
			Library = new AudioLibrary(audioData);
		}

		private void Update()
		{
			foreach (AudioSource source in AllSources)
			{
				if (source.isPlaying && AvailableSources.Contains(source))
					AvailableSources.Remove(source);
				else if (!source.isPlaying && !AvailableSources.Contains(source))
				{
					Library.GetHandleForSound(source.clip.name).OnCompleted?.Invoke();
					AvailableSources.Add(source);
				}
				
				if(!AvailableSources.Contains(source))
				{	
					if((source.clip.length - source.time ) <= Library.GetHandleForSound(source.clip.name).FadeDuration && !Library.GetHandleForSound(source.clip.name).IsFading && Library.GetHandleForSound(source.clip.name).ShouldFadeOut)
					{
						StartCoroutine(Library.GetHandleForSound(source.clip.name).fadeOut());
					}
				}
			}

			if (!BackgroundPaused && !BackgroundAudio.isPlaying && BackgroundAudio.clip != null)
			{
				if (BackgroundAudio.clip != null)
					BackgroundAudio.Play();
			}

		}

		// /// <summary>
		// /// To get the handles clip use AudioManager.LoadAudioFromResources("AudioName")
		// /// </summary>
		// /// <param name="handle"></param>
		// public void AddAudio(AudioHandle handle)
		// {
		// 	if (handle.clip != null)
		// 	{
		// 		if (!Sounds.HasHandle(handle))
		// 		{
		// 			Sounds..Add(handle.clip.name, handle);
		// 			handle.Init();
		// 		}
		// 		else
		// 		{
		// 			LogManager.LogError("Sounds list already contains specified sound!");
		// 		}
		// 	}
		// 	else
		// 		LogManager.LogError("AudioHandle's clip is null!");
		// }

		public void SetBackgroundAudio(string SoundName)
		{
			if (Library.HasSound(SoundName))
			{
				var handle = Library.GetHandleForSound(SoundName);
				if (handle.isValid)
				{
					BackgroundAudio.clip = handle.clip;
					BackgroundAudio.volume = handle.volume;
					BackgroundAudio.PlayDelayed(handle.delay);
					if (handle.ShouldFadeIn && handle.FadeDuration > 0 && !handle.IsFading)
						StartCoroutine(handle.fadeIn());
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
				Library.GetHandleForSound(BackgroundAudio.clip.name).fadeIn();
			BackgroundPaused = false;
		}

		

		public AudioHandle PlaySound(string SoundName)
		{
			var handle = Library.GetHandleForSound(SoundName);
			if (handle.isValid)
			{
				AvailableSources[0].clip = handle.clip;
				AvailableSources[0].volume = handle.volume;
				AvailableSources[0].PlayDelayed(handle.delay);
					
				if (handle.ShouldFadeIn && handle.FadeDuration > 0 && !handle.IsFading)
						StartCoroutine(handle.fadeIn());
				return handle;
			}
			else
				LogManager.LogWarn($"{SoundName} is currently not valid.");
			return null;
		}

		public void ForceFadeOut(string SoundName)
		{
			var handle = Library.GetHandleForSound(SoundName);
			if (handle.isValid)
			{
				StartCoroutine(handle.fadeOut());
			}
			else
				LogManager.LogWarn($"{SoundName} is currently not valid.");
		}

		public AudioSource GetAudioSourceOfHandle(string AudioName)
		{
			foreach (AudioSource source in AllSources)
			{
				if(source.clip == Library.GetHandleForSound(AudioName).clip)
				{
					return source;
				}
			}
			if (BackgroundAudio.clip == Library.GetHandleForSound(AudioName).clip)
				return BackgroundAudio;

			return null;
		}
	}
}
