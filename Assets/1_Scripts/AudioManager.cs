using System;
using System.Collections;
using System.Collections.Generic;
using _1_Scripts;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private Sound[] musicSounds, sfxSounds;
    [SerializeField] private AudioSource musicSource, sfxSource;


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
}
