using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{

    public static GameManagerScript Instance = null;

    GameObject lastCheckpoint;

	// Use this for initialization
	void Awake() {
        if (Instance == null)
            Instance = this;
	}

    public void PassCheckpoint(GameObject checkPoint)
    {
        if (lastCheckpoint != checkPoint)
        {
            Debug.Log("Checkpoint passed");
            lastCheckpoint = checkPoint;
        }
    }

    public void LoadAtCheckpoint()
    {
        // TODO respawn player at last checkpoint


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
