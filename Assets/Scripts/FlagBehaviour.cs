using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagBehaviour : MonoBehaviour {

    Cloth clothComponent;

    public float windStrength = 10f;

	// Use this for initialization
	void Awake () {
        clothComponent = GetComponent<Cloth>();
	}
	
	// Update is called once per frame
	void Update () {
        clothComponent.externalAcceleration = new Vector3(windStrength, 10, 0);
	}
}
