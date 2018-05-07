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

    bool spacePressed = false;

    private bool killedEnemy = false;
    bool isAlive = true;
    bool isDying = false;

    bool wasHit = false;
    bool knockbacking = false;
    
    [SerializeField]
    int jumpMax = 10;
    int remainingJumpTime;

    [SerializeField]
    Vector3 currentForce = Vector3.zero;

    Vector3 impactForce = Vector3.zero;

    // Use this for initialization
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        remainingJumpTime = jumpMax;

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

        
        moveDirection.x = currentMoveHorizontal * speed / 2f;
        
        animator.SetFloat("MoveSpeed", Mathf.Abs(currentMoveHorizontal));

    }
 

    void FixedUpdate()
    {
        currentForce = rigidBody.velocity;

        bool sliding = false;
        int wallDirection = 0;

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
            if (TouchingWall(out wallDirection))
            {
                Debug.Log("Sliding");

                sliding = true;

                
                // TODO reduce falling speed
                rigidBody.velocity = new Vector3(0, -0.5f, 0) * Time.deltaTime;
            }

            animator.SetBool("Grounded", false);

            inAir = true;
        }

        if (wasHit)
        {
            wasHit = false;
            animator.SetBool("WasHit", false);
            knockbacking = true;

            rigidBody.velocity = new Vector3(-2, 2, 0);
            return;
        }
        else
        {
            if (knockbacking && !IsGrounded())
            {
                Debug.Log("Adding force");

                

                //rigidBody.AddForce(new Vector3(-20, 0, 0).normalized * 100f, ForceMode.Acceleration);

                return;
            }
            else if (knockbacking)
            {
                Debug.Log("Knockback over");

                knockbacking = false;

                
            }
        }


        

        if (Input.GetKey(KeyCode.Space))
        {
            if (killedEnemy)
            {
                JumpAfterKill();
            }
            else if (inAir && !sliding)
            {
                Jump();
            }
            else if (IsGrounded())
            {
                JumpStart();
            }
            else if (sliding)
            {
                WallJump(wallDirection);
            }
        }
        else
        {
            Debug.Log("Released");
            
            if (killedEnemy)
            {
                JumpAfterKill();
            }
            else
            {
                if (inAir)
                    remainingJumpTime = 0;
            }
        }

        
        

        if (!sliding || (moveDirection.x * wallDirection < 0)) // TODO let go if moving opposite direction of wall
            rigidBody.MovePosition(transform.position + moveDirection * Time.deltaTime);
        
    }

    bool TouchingWall(out int direction)
    {
        RaycastHit hitInfo;

        direction = 0;

        Ray ray = new Ray(transform.position + capsuleCollider.center, -Vector3.right);
        
        if (Physics.Raycast(ray, out hitInfo, 4f)) // check one side
        {
            direction = -1;

            if (hitInfo.distance <= capsuleCollider.radius + 0.001f)
                return true;
        }

        ray = new Ray(transform.position + capsuleCollider.center, Vector3.right);

        if (Physics.Raycast(ray, out hitInfo, 4f)) // check another side
        {
            direction = 1;

            return hitInfo.distance <= capsuleCollider.radius + 0.001f;
        }
        
        return false;
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
        if (remainingJumpTime > 0)
        {
            Debug.Log("Jumping: " + remainingJumpTime);

            rigidBody.velocity = new Vector3(0, jumpSpeed, 0);

            remainingJumpTime--;
        }
        
        inAir = true;
    }

    void WallJump(int wallDirection)
    {
        Debug.Log("Wall jump");

        // TODO get direction of jump

        rigidBody.velocity = new Vector3(-wallDirection * 3, jumpSpeed, 0);

        remainingJumpTime = 0;
        
        inAir = true;
    }

    void JumpStart()
    {
        Debug.Log("Start jump with current vel: " + rigidBody.velocity );

        if (remainingJumpTime > 0 && !inAir)
        {
            rigidBody.velocity = new Vector3(0, jumpSpeed, 0);

            //rigidBody.AddForce(new Vector3(0, jumpSpeed / 5f, 0), ForceMode.Impulse);

            remainingJumpTime--;
        }

        inAir = true;
    }

    void JumpAfterKill()
    {
        Debug.Log("Jump after kill");

        killedEnemy = false;

        remainingJumpTime = jumpMax;

        //if (remainingJumpTime > 0 && !inAir)
        {
            rigidBody.velocity = new Vector3(0, jumpSpeed, 0);
            
            remainingJumpTime--;
        }

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



    private void OnCollisionEnter(Collision collision)
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

        if (other.tag == "Coin")
        {
            // TODO add score



            // TODO remove coin

            other.gameObject.GetComponent<CoinScript>().Collect();
        }
    }


    void KillEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().Die();

        killedEnemy = true;
        //Destroy(enemy);
    }
}
