using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {

    Rigidbody rigidBody;
    CapsuleCollider capsuleCollider;
    Animator animator;


    private bool falling = false;
    private bool inAir = true;

    private bool jumping = false;

    [SerializeField]
    private int health;

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    float moveHorizontal = 0f;
    float currentMoveHorizontal = 0f;

    private bool killedEnemy = false;
    bool isAlive = true;
    bool isDying = false;

    bool wasHit = false;
    bool knockbacking = false;

    const int jumpMax = 10;
    int remainingJumpTime = jumpMax;

    [SerializeField]
    Vector3 currentForce = Vector3.zero;

    Vector3 impactForce = Vector3.zero;

    // Use this for initialization
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();

    }

    // Update is called once per frame
    void Update()
    {

        if (!isAlive)
            return;

        // Fell to the death
        if (transform.position.y < -2f || isDying)
        {
            Die();
            return;
        }

        
        if (Input.GetKey(KeyCode.A))
            moveHorizontal = -1f;
        else if (Input.GetKey(KeyCode.D))
            moveHorizontal = 1f;
        else
            moveHorizontal = 0f;
     

        if (Mathf.Abs(moveHorizontal - currentMoveHorizontal) > 0.01f)
        {
            currentMoveHorizontal = Mathf.Lerp(currentMoveHorizontal, moveHorizontal, Time.deltaTime * speed);
        }
        else
        {
            currentMoveHorizontal = moveHorizontal;
        }

        // rotate
        if (moveHorizontal == 1f && this.transform.forward != Vector3.forward)
        {
            this.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 90, 0));
        }
        else if (moveHorizontal == -1f && this.transform.forward != -Vector3.forward)
        {
            this.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, -90, 0));
        }

        

        /*if (killedEnemy)
            JumpAfterKill();*/

        moveDirection.x = currentMoveHorizontal * speed / 2f;
        
        animator.SetFloat("MoveSpeed", Mathf.Abs(currentMoveHorizontal));

    }

    
    

    void FixedUpdate()
    {
        currentForce = rigidBody.velocity;

        if (IsGrounded())
        {
            if (inAir) // Just landed
            {
                // TODO set animation
                animator.SetBool("Grounded", true);


                inAir = false;
                
                Land();
            }
        }
        else
        {
            animator.SetBool("Grounded", false);

            inAir = true;
        }

        if (wasHit)
        {
            wasHit = false;
            animator.SetBool("WasHit", false);
            knockbacking = true;
            rigidBody.AddForce(new Vector3(0, 20, 0).normalized * 100f);
            rigidBody.AddForce(new Vector3(-20, 0, 0).normalized * 100f, ForceMode.Acceleration);
            return;
        }
        else
        {
            if (knockbacking && !IsGrounded())
            {
                Debug.Log("Adding force");

                

                rigidBody.AddForce(new Vector3(-20, 0, 0).normalized * 100f, ForceMode.Acceleration);

                return;
            }
            else if (knockbacking)
            {
                Debug.Log("Knockback over");

                knockbacking = false;

                
            }
        }




        if (Input.GetKey(KeyCode.Space) && !killedEnemy)
        {
            Jump();

            /*if (inAir)
            {
                //Jump();
            }
            else if (IsGrounded())
            {
                JumpStart();
            }*/
        }
        else
        {
            if (inAir)
                remainingJumpTime = 0;
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
        if (remainingJumpTime > 0 && !inAir)
        {
            //if (remainingJumpTime == jumpMax)
            
            Debug.Log("Start jump with current vel: " + rigidBody.velocity.y + " and remaining time " + remainingJumpTime);


            rigidBody.velocity = new Vector3(0, jumpSpeed / 5f, 0);

            remainingJumpTime--;
        }
        
        inAir = true;
    }

    void JumpStart()
    {
        Debug.Log("Start jump with current vel: " + rigidBody.velocity );

        //if (remainingJumpTime > 0)
        {
            

            rigidBody.AddForce(new Vector3(0, jumpSpeed / 5f, 0), ForceMode.Impulse);

            remainingJumpTime--;
        }

        inAir = true;
    }

    void JumpAfterKill()
    {
        killedEnemy = false;

        rigidBody.AddForce(new Vector3(0, jumpSpeed / 5f, 0), ForceMode.Impulse);

        inAir = true;
    }

    void Land()
    {
        remainingJumpTime = jumpMax;

        Debug.Log("Landed");
    }

    void Die()
    {
        Debug.Log("I just died in your arms tonight");

        isAlive = false;

        DestroyImmediate(gameObject);
    }



   /* private void OnCollisionEnter(Collision collision)
    {
        

        if (collision.gameObject.tag == "Platform")
            return;

        if (collision.gameObject.tag == "Enemy" && collision.gameObject.GetComponent<EnemyMovement>().IsAlive)
        {
            if (collision.collider.GetType() == typeof(BoxCollider)) // Body
            {
                Debug.Log("Ouch");

                animator.SetBool("WasHit", true);

                health--;

                if (health == 0)
                {
                    isDying = true;
                    return;
                }
                
                wasHit = true;

                

                //animator.SetBool("WasHit", false);

                //animator.SetBool("Grounded", false);
                inAir = true;
            }
            else if (collision.collider.GetType() == typeof(SphereCollider)) // Head
            {
                Debug.Log("Kill");

                KillEnemy(collision.gameObject);
            }


        }
    }


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered");

        if (other.tag == "Enemy")
        {
            Debug.Log("Collided with enemy");

            // TODO kill enemy
            KillEnemy(other.gameObject);
        }
    }*/


    void KillEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().Die();

        killedEnemy = true;
        //Destroy(enemy);
    }
}
