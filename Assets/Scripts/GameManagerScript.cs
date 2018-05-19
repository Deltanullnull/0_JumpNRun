using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{

    public static GameManagerScript Instance = null;

    public bool playerDied = false;

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

            lastCheckpoint.GetComponent<CheckpointScript>().Activate();
        }
    }

    public void LoadAtCheckpoint()
    {
        // TODO respawn player at last checkpoint

        GameObject player = Instantiate(Resources.Load("Player")) as GameObject;

        if (lastCheckpoint != null)
        {
            player.transform.position = lastCheckpoint.transform.position;
        }
        

        Camera.main.GetComponent<CameraMovement>().playerObject = player;

    }
	
	// Update is called once per frame
	void Update () {
		if (playerDied)
        {
            playerDied = false;
            
            StartCoroutine("RespawnPlayer");
        }
	}

    IEnumerator RespawnPlayer()
    {
        // wait 2sec before respawn
        yield return new WaitForSeconds(2f);

        LoadAtCheckpoint();
    }
}
