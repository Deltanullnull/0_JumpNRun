using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    SphereCollider headCollider;

    BoxCollider bodyCollider;

    public bool IsAlive;

	// Use this for initialization
	void Awake () {

        bodyCollider = GetComponent<BoxCollider>();
        headCollider = GetComponent<SphereCollider>();

        IsAlive = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (transform.position.y < -2)
            DestroyImmediate(gameObject);
    }

    public void Die()
    {
        IsAlive = false;

        bodyCollider.enabled = headCollider.enabled = false;
    }
}
