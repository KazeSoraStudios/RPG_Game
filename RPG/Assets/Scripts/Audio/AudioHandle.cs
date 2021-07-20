using System;
using System.Collections;
using UnityEngine;

namespace RPG_Audio
{
    public class AudioHandle
    {
        public string Id;
        public AudioClip clip;

        public float volume;

        public float FadeDuration;
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
            FadeDuration = 1;
            delay = 0;
            ShouldFadeIn = false;
            ShouldFadeOut = false; 
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
            var source = ServiceManager.Get<AudioManager>().GetAudioSourceOfHandle(Id);
            while (volume < origVolume)
            {
                volume += (1.0f / FadeDuration) * Time.fixedDeltaTime;
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
            var source = ServiceManager.Get<AudioManager>().GetAudioSourceOfHandle(Id);
            while (volume > 0.0f)
            {
                volume -= (1.0f / FadeDuration) * Time.fixedDeltaTime;
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
            ServiceManager.Get<AudioManager>().GetAudioSourceOfHandle(Id).volume = volume;
            FadeDuration = 1.0f;
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
}