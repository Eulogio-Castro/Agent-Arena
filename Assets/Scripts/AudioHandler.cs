using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class AudioHandler : MonoBehaviour
{

    public Sound[] sounds;
    // Start is called before the first frame update

    private void Awake()
    {
        foreach(Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.pitch = sound.pitch + UnityEngine.Random.Range(-sound.pitchVariance, sound.pitchVariance);
            sound.source.loop = sound.loop;
            sound.source.volume = sound.volume + UnityEngine.Random.Range(-sound.volumeVariance, sound.volumeVariance);
            sound.source.spatialBlend = 1;
            sound.source.outputAudioMixerGroup = sound.mixerGroup;
        }
    }
    void Start()
    {
        if (Array.Find(sounds, sound => sound.name == SceneManager.GetActiveScene().name) != null)
        {
            Play(SceneManager.GetActiveScene().name);
        }
        
    }

    public void Play(string name)
    {
        //find sound where sound.name == name
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public AudioClip getClip(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s.source.clip;
    }

}
