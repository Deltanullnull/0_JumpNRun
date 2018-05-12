using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagBehaviour : MonoBehaviour {

    Cloth clothComponent;
    SkinnedMeshRenderer skin;

    public float windStrength = 10f;

	// Use this for initialization
	void Awake () {
        clothComponent = GetComponent<Cloth>();
        skin = GetComponent<SkinnedMeshRenderer>();

        clothComponent.randomAcceleration = new Vector3(0, 10, 0);

        clothComponent.externalAcceleration = new Vector3(windStrength, 0, 0);
         
        
    }
	
	// Update is called once per frame
	void Update () {
        
        
	}
}
