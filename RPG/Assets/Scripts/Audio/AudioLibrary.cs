using UnityEngine;

namespace RPG_Audio
{
	public class AudioLibrary : MonoBehaviour
	{
		public AudioHandle HeroVillageTheme;

		private void Awake()
		{
			ServiceManager.Register(this);
		}
		void OnDestroy()
		{
			ServiceManager.Unregister(this);
		}

		void Start()
		{
			HeroVillageTheme = new AudioHandle
			{
				clip = ServiceManager.Get<AssetManager>().Load<AudioClip>("Sounds/HeroVillageTheme"),
				ShouldFadeIn = true,
				fadeDuration = 100,
			};
			ServiceManager.Get<AudioManager>().AddAudio(HeroVillageTheme);

		}
	}
}
