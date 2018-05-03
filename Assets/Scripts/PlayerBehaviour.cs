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

    [SerializeField]
    Vector3 moveForward = Vector3.zero;

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

        //Ray ray = new Ray(transform.position + capsuleCollider.center, -Vector3.up);

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.green);

        float moveHorizontalPre = moveHorizontal;
        float moveVertical = 0f;

        moveForward = transform.forward;

        // Fell to the death
        if (transform.position.y < -2f || isDying)
        {
            Die();
            return;
        }

        
        //else
        {
            //impactForce = Vector3.zero;
            if (Input.GetKey(KeyCode.A))
                moveHorizontal = -1f;
            else if (Input.GetKey(KeyCode.D))
                moveHorizontal = 1f;
            else
                moveHorizontal = 0f;
        }

        if (Mathf.Abs(moveHorizontal - currentMoveHorizontal) > 0.01f)
        {
            currentMoveHorizontal = Mathf.Lerp(currentMoveHorizontal, moveHorizontal, Time.deltaTime * speed);
        }
        else
        {
            currentMoveHorizontal = moveHorizontal;
        }

        // TODO rotate
        if (moveHorizontal == 1f && this.transform.forward != Vector3.forward)
        {
            this.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 90, 0));
        }
        else if (moveHorizontal == -1f && this.transform.forward != -Vector3.forward)
        {
            this.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, -90, 0));
        }

        if (IsGrounded())
        {
            if (inAir) // Just landed
            {
                // TODO set animation
                animator.SetBool("Grounded", true);


                inAir = false;

                //moveDirection = new Vector3(moveHorizontal, 0, 0);


            }

            moveDirection = Vector3.zero;

            //moveDirection = new Vector3(currentMoveHorizontal, 0, 0) * speed / 2f;

            

        }
        else
        {


            animator.SetBool("Grounded", false);

            inAir = true;
        }

        if (killedEnemy)
            Jump();

        moveDirection.x = currentMoveHorizontal * speed / 2f;

        //moveDirection.y -= gravity * Time.deltaTime;

        

        //charController.Move(moveDirection * Time.deltaTime);

        animator.SetFloat("MoveSpeed", Mathf.Abs(currentMoveHorizontal));

    }

    bool wasHit = false;
    bool knockbacking = false;
    

    void FixedUpdate()
    {
        currentForce = rigidBody.velocity;

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

        }
        

        rigidBody.MovePosition(transform.position + moveDirection * Time.deltaTime);
        
    }

    bool IsGrounded()
    {
        RaycastHit hitInfo;
        
        Ray ray = new Ray(transform.position + capsuleCollider.center, -Vector3.up);

        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.green);

        if (Physics.Raycast(ray, out hitInfo, 10f/*, LayerMask.GetMask("Platform")*/))
        {
            return hitInfo.distance <= capsuleCollider.height / 2 + 0.01f;
        }

        return false;
    }

    void Jump()
    {
        killedEnemy = false;

        Debug.Log("Jump");

        rigidBody.AddForce(new Vector3(0, jumpSpeed, 0));

        //moveDirection.y = jumpSpeed;

        inAir = true;
    }

    void Die()
    {
        Debug.Log("I just died in your arms tonight");

        isAlive = false;

        DestroyImmediate(gameObject);
    }

    void Knockback()
    {
        Debug.Log("Knockback");

        //rigidBody.AddForce(impactForce, ForceMode.Impulse);

        //charController.Move(impactForce * Time.deltaTime);

        //impactForce = Vector3.Lerp(impactForce, Vector3.zero, 5 * Time.deltaTime);
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

                

                //animator.SetBool("WasHit", false);

                //animator.SetBool("Grounded", false);
                inAir = true;
            }
            /*else if (hit.collider.GetType() == typeof(SphereCollider)) // Head
            {
                Debug.Log("Kill");

                KillEnemy(hit.gameObject);
            }*/


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
    }


    void KillEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().Die();

        killedEnemy = true;
        //Destroy(enemy);
    }
}
