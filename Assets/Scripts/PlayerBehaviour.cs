using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {

    Rigidbody rigidBody;
    BoxCollider playerCollider;
    BoxCollider footCollider;
    Animator animator;

    private bool falling = false;
    private bool inAir = true;

    private bool jumping = false;

    [SerializeField]
    private int health;

    [SerializeField]
    private int maxHealth;

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

    int invicibleTime = 60;
    int invicibleTimeRemaining = 0;

    private int hitDirection = 0;
    
    [SerializeField]
    int jumpMax = 10;
    int remainingJumpTime;

    [SerializeField]
    Vector3 currentForce = Vector3.zero;

    [SerializeField]
    Vector3 currentVelocity = Vector3.zero;

    Vector3 impactForce = Vector3.zero;

    // Use this for initialization
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider>();

        var colliders = GetComponents<BoxCollider>();

        foreach (BoxCollider coll in colliders)
        {
            if (coll.isTrigger)
            {
                footCollider = coll;
                
            }
            else
            {
                playerCollider = coll;
            }
        }

        remainingJumpTime = jumpMax;

        

        
    }

    private void Start()
    {
        health = maxHealth;

        LifeScript.Instance.ResetHealth();

        Debug.Log("Setting health");
    }

    // Update is called once per frame
    void Update()
    {
        if (invicibleTimeRemaining > 0)
        {
            // Turn off collider with enemy

            Physics.IgnoreLayerCollision(10, 11, true);
            invicibleTimeRemaining--;
        }
        else
        {
            Physics.IgnoreLayerCollision(10, 11, false);
        }
        
        // Fell to the death

        if (transform.position.y < -2f)
        {
            GameManagerScript.Instance.playerDied = true;

            DestroyImmediate(gameObject);
            return;
        }

        if (!isAlive)
            return;


        

        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveHorizontal = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveHorizontal = 1f;
        else
            moveHorizontal = 0f;
     

        // TODO set currentMoveHorizontal instantly to zero when sliding
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
        if (!isAlive)
            return;

        if (isDying)
        {
            Die();
            return;
        }

        currentVelocity = rigidBody.velocity;

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

                currentMoveHorizontal = 0;


                // reduce falling speed
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

            invicibleTimeRemaining = invicibleTime;

            // TODO hit from which direction?
            rigidBody.velocity = new Vector3(-2 * hitDirection, 2, 0);
            return;
        }
        else
        {
            if (knockbacking )
            {
                if (!IsGrounded())
                    return;

                knockbacking = false;
            }
        }


        // TODO Make sure we won't jump again unless we released the space button and press again
        // if we hold space down, set spaceDown to true
        // if spaceDown, we cannot reach JumpStart()

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
            else if (IsGrounded() && !spacePressed)
            {
                JumpStart();
            }
            else if (sliding && !spacePressed)
            {
                WallJump(wallDirection);
            }

            spacePressed = true;
        }
        else
        {
            
            if (killedEnemy)
            {
                JumpAfterKill();
            }
            else
            {
                if (inAir)
                    remainingJumpTime = 0;
            }

            spacePressed = false;
        }

        bool releaseWall = (sliding && moveDirection.x * wallDirection < 0);

        if (!sliding || releaseWall) // TODO let go if moving opposite direction of wall
        {
            

            rigidBody.MovePosition(transform.position + moveDirection * Time.deltaTime);
        }
    }

    bool TouchingWall(out int direction)
    {
        RaycastHit hitInfo;

        direction = 0;

        Ray ray = new Ray(transform.position + playerCollider.center, -Vector3.right);
        
        if (Physics.Raycast(ray, out hitInfo, 4f, LayerMask.GetMask("Platform"))) // check one side
        {
            direction = -1;

            if (hitInfo.distance <= playerCollider.size.z / 2 + 0.001f)
                return true;
        }

        ray = new Ray(transform.position + playerCollider.center, Vector3.right);

        if (Physics.Raycast(ray, out hitInfo, 4f, LayerMask.GetMask("Platform"))) // check another side
        {
            direction = 1;

            return hitInfo.distance <= playerCollider.size.z / 2 + 0.001f;
        }
        
        return false;
    }

    bool IsGrounded()
    {
        RaycastHit hitInfo;
        
        Ray ray = new Ray(transform.position + playerCollider.center, -Vector3.up);

        if (Physics.Raycast(ray, out hitInfo, 10f, LayerMask.GetMask("Checkpoint")))
        {
            return false;
        }

        int ignoreLayer = ~(1 << 10);

        if (Physics.Raycast(ray, out hitInfo, 10f, ignoreLayer))
        {
            if (hitInfo.collider != footCollider)
            {
                return hitInfo.distance <= playerCollider.size.y / 2 + 0.001f;
            }

            
        }

        return false;
    }

    void Jump()
    {
        if (remainingJumpTime > 0)
        {
            //Debug.Log("Jumping: " + remainingJumpTime);

            rigidBody.velocity = new Vector3(0, jumpSpeed, 0);

            remainingJumpTime--;
        }
        
        inAir = true;
    }

    void WallJump(int wallDirection)
    {
        Debug.Log("Wall jump");

        // TODO get direction of jump

        rigidBody.velocity = new Vector3(-wallDirection * 5, jumpSpeed * 1.5f, 0);

        remainingJumpTime = 0;
        
        inAir = true;
    }

    void JumpStart()
    {
        //Debug.Log("Start jump with current vel: " + rigidBody.velocity );

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

        //Debug.Log("Landed");
    }


    
    void Die()
    {
        // TODO make character spin around and throw in the air; deactivate collider

        isAlive = false;

        isDying = false;
        
        playerCollider.enabled = false;

        rigidBody.constraints = RigidbodyConstraints.None;


        rigidBody.velocity = new Vector3(0, 10f, -1f);
        //DestroyImmediate(gameObject);

        StartCoroutine("SpinAround");
    }

    IEnumerator SpinAround()
    {
        while (true)
        {
            transform.Rotate(Vector3.back, 10f * Time.deltaTime);
            yield return null;
        }
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

                float contactPoint = transform.position.x - collision.contacts[0].point.x;

                if (contactPoint > 0)
                    hitDirection = -1;
                else
                    hitDirection = 1;

                foreach (ContactPoint point in collision.contacts)
                {
                    Debug.Log("Point: " + (transform.position.x - point.point.x) + ", " + point.point.y);    
                }

                animator.SetBool("WasHit", true);

                health--;

                LifeScript.Instance.SetHealth((float)health / maxHealth);

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
            other.gameObject.GetComponent<CoinScript>().Collect();
        }
        
        if (other.tag == "Checkpoint")
        {
            // TODO trigger checkpoint
            GameManagerScript.Instance.PassCheckpoint(other.gameObject);
        }
    }


    void KillEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().Die();

        killedEnemy = true;
        //Destroy(enemy);
    }
}
