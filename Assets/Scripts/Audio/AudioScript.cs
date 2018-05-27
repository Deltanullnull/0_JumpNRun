using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour {

    AudioSource[] audioSources;

    public static AudioScript Instance;

	// Use this for initialization
	void Start () {

        if (Instance == null)
            Instance = this;

        audioSources = GetComponents<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlaySound(int idx)
    {
        audioSources[idx].Play();
    }
}
