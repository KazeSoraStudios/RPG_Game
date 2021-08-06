using System.Collections.Generic;
using UnityEngine;
using RPG_GameData;

namespace RPG_Audio
{
	public class AudioLibrary
	{
		private Dictionary<string, AudioHandle> audio = new Dictionary<string, AudioHandle>();

		public AudioLibrary(Dictionary<string, AudioData> audioData)
		{
			LoadAudio(audioData);
		}

		public AudioHandle GetHandleForSound(string sound)
		{
			if (!audio.ContainsKey(sound))
			{
				LogManager.LogWarn($"Sound [{sound}] not found in AudioLibrary.");
				return null;
			}
			return audio[sound];
		}

		public bool HasSound(string sound)
		{
			return audio.ContainsKey(sound);
		}

		public bool HasHandle(AudioHandle handle)
		{
			return audio.ContainsValue(handle);
		}

		public void ClearLibrary()
		{
			audio.Clear();
			audio = null;
		}

		private void LoadAudio(Dictionary<string, AudioData> audioData)
		{
			audio.Clear();
			if (audioData == null || audioData.Count < 1)
			{
				LogManager.LogError("No audio passed to AudioLibrary!");
				return;
			}
			foreach (var entry in audioData)
			{
				if (audio.ContainsKey(entry.Key))
				{
					LogManager.LogError($"Duplicate Key [{entry.Key}] in AudioData.");
					continue;
				}
				var data = entry.Value;
				var clip = ServiceManager.Get<AssetManager>().Load<AudioClip>(Constants.SOUND_FOLDER + data.SoundName);
				if (clip == null)
					continue;
				var handle = new AudioHandle
				{
					clip = clip,
					ShouldFadeIn = data.FadeIn,
					ShouldFadeOut = data.FadeOut,
					FadeDuration = data.FadeDuration,
					delay = data.delay,
					Id = data.Id, 
					volume = 1
				};
				audio[entry.Key] = handle;
			}
		}
	}
}
