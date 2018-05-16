using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    SphereCollider headCollider;

    BoxCollider bodyCollider;

    Rigidbody rigidBody;

    Animator animator;

    public bool IsAlive;

    public int Value;

    float moveDirection = -1f;


    // Use this for initialization
    void Awake () {

        bodyCollider = GetComponent<BoxCollider>();
        headCollider = GetComponent<SphereCollider>();

        rigidBody = GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();

        IsAlive = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (transform.position.y < -2)
            DestroyImmediate(gameObject);

        // check, if ridge is near, turn direction
        // Raycast from offset downwards

        Ray rayDown = new Ray(rigidBody.transform.position + new Vector3(moveDirection, 0, 0) * Time.deltaTime, Vector3.down);
        RaycastHit hitInfo;

        if (Physics.Raycast(rayDown, out hitInfo, 5f, LayerMask.GetMask("Platform")))
        {
            if (hitInfo.distance > bodyCollider.size.y / 2)
                moveDirection *= -1; 
        }
        else
        {
            moveDirection *= -1;
        }
        

    }

    private void FixedUpdate()
    {
        if (!IsAlive)
            return;

        // TODO move back and forth
        rigidBody.MovePosition(rigidBody.transform.position + new Vector3(moveDirection, 0, 0) * Time.deltaTime);
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
