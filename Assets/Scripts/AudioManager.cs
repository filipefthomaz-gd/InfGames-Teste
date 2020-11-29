using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource music;

    private static bool started = false;

    void Awake()
    {   
        DontDestroyOnLoad(this.gameObject);
        if (started)
            Destroy(gameObject);
    }

    void Start()
    {
        music = GetComponent<AudioSource>();
        started = true;
        music.Play();

    }
}
