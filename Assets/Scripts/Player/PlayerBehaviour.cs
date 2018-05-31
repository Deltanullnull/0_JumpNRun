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

    private AudioSource[] audioSteps;

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

    

    int invincibleTime = 60;
    int invincibleTimeRemaining = 0;

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

        audioSteps = GetComponents<AudioSource>();
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

        Physics.IgnoreLayerCollision(11, 12); // Ignore Enemy - PlayerFeet
        Physics.IgnoreLayerCollision(10, 13); // Ignore Head - Player

    }

    private void Start()
    {
        health = maxHealth;

        LifeScript.Instance.ResetHealth();
    }

    // Update is called once per frame
    void Update()
    {
        
        // Fell to the death
        if (transform.position.y < -2f)
        {
            
            AudioScript.Instance.PlaySound(1, 0.35f);

            LifeScript.Instance.SetHealth(0);

            GameManagerScript.Instance.playerDied = true;

            DestroyImmediate(gameObject);
            
            return;
        }

        if (!isAlive)
            return;
        
        if (GameManagerScript.Instance.levelActive)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveHorizontal = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveHorizontal = 1f;
            else
                moveHorizontal = 0f;
        }
        else
        {
            moveHorizontal = 0f;
        }
        
     

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

                audioSteps[3].Play();
            }
        }
        else
        {
            if (TouchingWall(out wallDirection))
            {
                Debug.Log("Sliding");

                sliding = true;

                knockbacking = false;


                GetComponent<SlidingScript>().SetParticleVisible(true);

                currentMoveHorizontal = 0;


                // reduce falling speed
                rigidBody.velocity = new Vector3(0, -0.5f, 0) * Time.deltaTime;
            }
            else
            {
                GetComponent<SlidingScript>().SetParticleVisible(false);
            }

            animator.SetBool("Grounded", false);

            inAir = true;
        }

        if (wasHit)
        {
            wasHit = false;
            animator.SetBool("WasHit", false);
            knockbacking = true;

            audioSteps[4].Play();

            StartCoroutine("Invincibility");

            invincibleTimeRemaining = invincibleTime;

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

        if (GameManagerScript.Instance.levelActive)
        {
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
        }

        bool releaseWall = (sliding && moveDirection.x * wallDirection < 0);

        if (!sliding || releaseWall) // TODO let go if moving opposite direction of wall
        {
            

            rigidBody.MovePosition(transform.position + moveDirection * Time.deltaTime);
        }
    }

    IEnumerator Invincibility()
    {
        // TODO change so that we still can collide with head
        Physics.IgnoreLayerCollision(10, 11, true);

        GameObject meshComponent = transform.GetChild(1).gameObject;

        Color materialColor = meshComponent.GetComponent<Renderer>().material.color;

        

        for (int i = 0; i < invincibleTime; i++)
        {
            // make flashing

            if ((i / 5) % 2 == 0)
            {
                materialColor.a = 0;
            }
            else
            {
                materialColor.a = 1;
            }
            
            
            foreach (Material material in meshComponent.GetComponent<Renderer>().materials)
            {
                material.color = materialColor;
                //meshComponent.GetComponent<Renderer>().material.color = materialColor;
            }
            

            yield return null;
        }

        materialColor.a = 1;

        foreach (Material material in meshComponent.GetComponent<Renderer>().materials)
        {
            material.color = materialColor;
            //meshComponent.GetComponent<Renderer>().material.color = materialColor;
        }

        //meshComponent.GetComponent<Renderer>().material.color = materialColor;

        Physics.IgnoreLayerCollision(10, 11, false);

        yield return null;
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

        int ignoreLayer = ~LayerMask.GetMask("PlayerFeet"); // Ignore layer with player feet

        if (Physics.Raycast(ray, out hitInfo, 10f, ignoreLayer))
        {
            //Debug.Log("Hit ground: " + hitInfo.distance);

            //if (hitInfo.collider != footCollider)
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
            rigidBody.velocity = new Vector3(0, jumpSpeed, 0);

            remainingJumpTime--;
        }
        
        inAir = true;
    }

    void WallJump(int wallDirection)
    {
        Debug.Log("Wall jump");

        // get direction of jump
        rigidBody.velocity = new Vector3(-wallDirection * 5, jumpSpeed * 1.5f, 0);

        remainingJumpTime = 0;
        
        inAir = true;
    }

    void JumpStart()
    {
        if (remainingJumpTime > 0 && !inAir)
        {
            rigidBody.velocity = new Vector3(0, jumpSpeed, 0);
            
            remainingJumpTime--;

            audioSteps[2].Play();
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

            Debug.Log("Me:");
            foreach (ContactPoint point in collision.contacts)
            {
                Debug.Log("Point: " + (point.point.x - transform.position.x) + ", " + (point.point.y - transform.position.y));
            }

            Debug.Log("Other:");
            foreach (ContactPoint point in collision.contacts)
            {
                Debug.Log("Point: " + (point.point.x - collision.gameObject.transform.position.x) + ", " + (point.point.y - collision.gameObject.transform.position.y));
            }

            if (collision.collider.GetType() == typeof(BoxCollider)) // Body
            {
                //Debug.Log("Ouch");
                
                float contactPoint = transform.position.x - collision.contacts[0].point.x;

                if (contactPoint > 0)
                    hitDirection = -1;
                else
                    hitDirection = 1;
                
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
                // We usually have only one collision point
                float contactPoint =  collision.contacts[0].point.y - transform.position.y;
                
                //if (contactPoint < 0.15) // Lower part of player collides with head
                // Not working, since we'll push enemy unless we jump on its head
                KillEnemy(collision.gameObject);
            }


        }
    }

    void StepRight()
    {
        audioSteps[0].Play();
    }

    void StepLeft()
    {
        audioSteps[1].Play();
    }

    void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "Coin")
        {
            other.gameObject.GetComponent<CoinScript>().Collect();
        }
        
        if (other.tag == "Checkpoint")
        {
            // trigger checkpoint
            GameManagerScript.Instance.PassCheckpoint(other.gameObject);
        }

        if (other.tag == "Goal")
        {
            // TODO finish level
            GameManagerScript.Instance.WinLevel();
        }
    }


    void KillEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().Die();

        killedEnemy = true;
    }
}
