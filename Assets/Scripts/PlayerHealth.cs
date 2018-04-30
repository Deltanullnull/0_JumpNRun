using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {

    [SerializeField]
    private int health;

	// Use this for initialization
	void Awake () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void KillEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().Die();
    }
}
