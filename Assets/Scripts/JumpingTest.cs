using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingTest : MonoBehaviour
{

    Rigidbody rigidBody;
    CapsuleCollider capsuleCollider;
    Animator animator;

    [SerializeField]
    private bool inAir = true;
    
    public float jumpSpeed;

    public float speed = 6.0F;

    float moveHorizontal = 0f;
    float currentMoveHorizontal = 0f;

    Vector3 moveDirection = Vector3.zero;

    // Use this for initialization
    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A))
            moveHorizontal = -1f;
        else if (Input.GetKey(KeyCode.D))
            moveHorizontal = 1f;
        else
            moveHorizontal = 0f;
    
        if (Mathf.Abs(moveHorizontal - currentMoveHorizontal) > 0.01f)
        {
            currentMoveHorizontal = Mathf.Lerp(currentMoveHorizontal, moveHorizontal, Time.deltaTime* speed);
        }
        else
        {
            currentMoveHorizontal = moveHorizontal;
        }

        moveDirection.x = currentMoveHorizontal * speed / 2f;

        animator.SetFloat("MoveSpeed", Mathf.Abs(currentMoveHorizontal));
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (IsGrounded())
        {
            if (inAir) // Just landed
            {
                animator.SetBool("Grounded", true);

                inAir = false;
            }
            
        }
        else
        {
            if (!inAir)
            {
                animator.SetBool("Grounded", false);

                inAir = true;
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Jump();

        }

        rigidBody.MovePosition(transform.position + moveDirection * Time.deltaTime);
    }

    bool IsGrounded()
    {
        RaycastHit hitInfo;

        Ray ray = new Ray(transform.position + capsuleCollider.center, -Vector3.up);
        
        if (Physics.Raycast(ray, out hitInfo, 10f))
        {
            return hitInfo.distance <= capsuleCollider.height / 2 + 0.001f;
        }

        return false;
    }

    void Jump()
    {
        if (!inAir)
        {
            rigidBody.velocity = new Vector3(0, jumpSpeed / 5f, 0);

            animator.SetBool("Grounded", false);
        }

        inAir = true;
    }
}
