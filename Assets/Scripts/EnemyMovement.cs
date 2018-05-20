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
        headCollider = transform.GetChild(1).GetComponent<SphereCollider>();

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
        Ray rayForward = new Ray(rigidBody.transform.position + Vector3.up * bodyCollider.size.y / 2 , -transform.right);

        Debug.DrawRay(rayForward.origin, rayForward.direction);

        RaycastHit hitInfo;

        bool turnAround = false;

        if (Physics.Raycast(rayDown, out hitInfo, 5f, LayerMask.GetMask("Platform")))
        {
            if (hitInfo.distance > bodyCollider.size.y / 2)
                turnAround = true;
                    //moveDirection *= -1; 
        }
        else
        {
            turnAround = true;
        }
        if (Physics.Raycast(rayForward, out hitInfo, moveDirection, LayerMask.GetMask("Platform")))
        {
            if (hitInfo.distance <= (bodyCollider.size.x / 2 + moveDirection * Time.deltaTime))
                turnAround = true;
            //moveDirection *= -1;
        }
        
        
        if (turnAround)
            moveDirection *= -1;

        if (moveDirection > 0 && this.transform.right != -Vector3.right)
        {
            this.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 180, 0));
            Debug.Log("Rotate right");
        }
        else if (moveDirection < 0 && this.transform.right != Vector3.right)
        {
            this.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
            Debug.Log("Rotate left");
        }
    }

    private void FixedUpdate()
    {
        if (!IsAlive)
            return;


        

        // TODO move back and forth
        rigidBody.MovePosition(rigidBody.transform.position + new Vector3(moveDirection, 0, 0) * Time.deltaTime);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player")
            return;

        //Debug.Log("Me:");
        //foreach (ContactPoint point in collision.contacts)
        //{
        //    Debug.Log("Point: " + (point.point.x - transform.position.x) + ", " + (point.point.y - transform.position.y));
        //}

        //Debug.Log("Other:");
        //foreach (ContactPoint point in collision.contacts)
        //{
        //    Debug.Log("Point: " + (point.point.x - collision.gameObject.transform.position.x) + ", " + (point.point.y - collision.gameObject.transform.position.y));
        //}
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
