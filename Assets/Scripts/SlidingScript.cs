using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingScript : MonoBehaviour {

    ParticleSystem boulderParticles;
    ParticleSystem.EmissionModule emissionModule;
	// Use this for initialization
	void Awake()
    {
        boulderParticles = transform.GetChild(3).GetComponent<ParticleSystem>();
        
        //boulderParticles.enableEmission = true;

        if (!boulderParticles.isPlaying)
        {
            boulderParticles.Play();
        }

        emissionModule = boulderParticles.emission;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetParticleVisible(bool visible)
    {
        if (visible)
        {
            emissionModule.enabled = true;
            //boulderParticles.Play();
        }
        else if (!visible )
        {
            emissionModule.enabled = false;
            //boulderParticles.Clear();
            //boulderParticles.Stop();
        }
            
    }
}
