using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {


    [SerializeField]
    GameObject playerObject;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        // TODO Follow player (only within level boundaries

        if (playerObject != null)
        {
            Vector3 camPosition = new Vector3
            {
                x = Mathf.Max(0, playerObject.transform.position.x),
                y = Mathf.Max(1, playerObject.transform.position.y),
                z = transform.position.z
            };

            transform.SetPositionAndRotation(camPosition, transform.rotation);
        }
        
	}
}
