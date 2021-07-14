using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> AvailableSources = new List<AudioSource>();
    public AudioSource BackgroundAudio;
    public Dictionary<string, AudioClip> Sounds = new Dictionary<string, AudioClip>();
    public AudioSource[] AllSources = new AudioSource[8];

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
        ServiceManager.Unregister(this);
    }

}