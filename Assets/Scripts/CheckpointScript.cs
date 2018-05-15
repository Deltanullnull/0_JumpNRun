using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointScript : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void Activate()
    {
        GameObject flag = gameObject.transform.GetChild(0).gameObject;

        flag.SetActive(true);

        flag.transform.GetChild(0).GetComponent<FlagBehaviour>().Activate();

        //flag.transform.parent = gameObject.transform;

        //flag.transform.position = Vector3.zero;
    }
}
