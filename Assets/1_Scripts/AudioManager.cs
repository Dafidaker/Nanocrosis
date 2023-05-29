using System;
using System.Collections;
using System.Collections.Generic;
using _1_Scripts;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] public AudioMixer audioMixer;
    [SerializeField] private Sound[] musicSounds, sfxSounds;
    [SerializeField] private AudioSource musicSource, sfxSource;


    private const string MasterVolume = "MasterVolume";
    private const string MusicVolume = "MusicVolume";
    private const string EffectsVolume = "SFXVolume";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        PlayMusic("MainMenu");
    }

    public void PlayMusic(string name, Vector3 point = default)
    {
        Sound s = Array.Find(musicSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Sound Not Found");
            return;
        }

        if (point != default)
        {
            AudioSource.PlayClipAtPoint(s.clip, point);
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    
    public void PlaySFX(string name, Vector3 point = default)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Sound Not Found");
            return;
        }

        if (point != default)
        {
            AudioSource.PlayClipAtPoint(s.clip, point);
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat(MasterVolume, Mathf.Log10(volume) * 20);
    }
    
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(MusicVolume, Mathf.Log10(volume) * 20);
    }
    
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(EffectsVolume, Mathf.Log10(volume) * 20);
    }
}
