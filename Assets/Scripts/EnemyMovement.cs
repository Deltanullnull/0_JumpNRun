using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    SphereCollider headCollider;

    BoxCollider bodyCollider;

    Rigidbody rigidBody;

    public bool IsAlive;

    public int Value;


    // Use this for initialization
    void Awake () {

        bodyCollider = GetComponent<BoxCollider>();
        headCollider = GetComponent<SphereCollider>();

        rigidBody = GetComponent<Rigidbody>();

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

        //rigidBody.isKinematic = false;

        //bodyCollider.enabled = headCollider.enabled = false;

        ScoreManager.instance.IncreaseScore(Value);

        // TODO start coroutine where enemy is being "flattened"

        StartCoroutine("Squeeze");
    }

    IEnumerator Squeeze()
    {
        while (transform.localScale.y >= 0.1f)
        {
            Vector3 localScale = transform.localScale;

            localScale.y *= 0.8f;

            transform.localScale = localScale;

            yield return null;
        }

        DestroyImmediate(gameObject);

        yield return null;
    }
}
