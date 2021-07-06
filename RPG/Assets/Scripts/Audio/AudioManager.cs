using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private static Dictionary<string, AudioSource> Sounds = new Dictionary<string, AudioSource>();

    private AudioSource _audioSource;
    private static GameObject _gameObject;

    void Awake()
    {

        ServiceManager.Register(this);
        _gameObject = gameObject;
    }

    void OnDestroy()    
    {
        ServiceManager.Unregister(this);
    }
    
    /// <summary>
    /// use like AddAudio("AudioName").clip = Resources.Load<AudioClip>("Sounds/audioFile");
    /// </summary>
    public static AudioSource AddAudio(string AudioName)
    {
        var source = _gameObject.AddComponent<AudioSource>();
        Sounds.Add(AudioName, source);
        return source;       
    }

    public void Play(string SoundName)
    {
       Sounds[SoundName].Play();
    }

    public void Stop(string SoundName)
    {
        Sounds[SoundName].Stop();
    }

    public void Pause(string SoundName)
    {
        Sounds[SoundName].Pause();
    }

    public void UnPause(string SoundName)
    {
        Sounds[SoundName].UnPause();
    }
}