using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    CharacterController charController;
    Animator animator;
    

    private bool falling = false;
    private bool inAir = true;

    private bool jumping = false;

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    float moveHorizontal = 0f;
    float currentMoveHorizontal = 0f;

    // Use this for initialization
    void Awake ()
    {
        charController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontalPre = moveHorizontal;
        float moveVertical = 0f;

        if (Input.GetKey(KeyCode.A))
            moveHorizontal = -1f;
        else if (Input.GetKey(KeyCode.D))
            moveHorizontal = 1f;
        else
            moveHorizontal = 0f;




        if (charController.isGrounded)
        {
            if (inAir) // Just landed
            {
                // TODO set animation
                animator.SetBool("Grounded", true);

                inAir = false;

                //moveDirection = new Vector3(moveHorizontal, 0, 0);

                if (Input.GetKey(KeyCode.Space) && !jumping)
                {
                    Debug.Log("Jump");

                    jumping = true;

                    
                }
            }
            else
            {
                //animator.SetFloat("MoveSpeed", moveHorizontal);

                if (moveHorizontal != moveHorizontalPre)
                {
                    StartCoroutine(AccelerateMovement());
                }
                    
            }

            moveDirection = new Vector3(currentMoveHorizontal, 0, 0);

            //moveDirection = transform.TransformDirection(moveDirection);

            //charController.Move(moveDirection);
            
        }
        else
        {
            animator.SetBool("Grounded", false);
        }

        // TODO if moveDirection not zero, start coroutine

        charController.Move(moveDirection * Time.deltaTime);
   
    }

    IEnumerator AccelerateMovement()
    {

        while (Mathf.Abs(moveHorizontal - currentMoveHorizontal) > 0.01f)
        {
            currentMoveHorizontal = Mathf.Lerp(currentMoveHorizontal, moveHorizontal, Time.deltaTime * speed);

            animator.SetFloat("MoveSpeed", currentMoveHorizontal);

            yield return null;
        }

        //yield return null;

    }
}
