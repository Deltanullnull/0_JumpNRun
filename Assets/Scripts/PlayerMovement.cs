using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    CharacterController charController;
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

    Vector3 impactForce = Vector3.zero;

    // Use this for initialization
    void Awake ()
    {
        charController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
            return;

        float moveHorizontalPre = moveHorizontal;
        float moveVertical = 0f;

        

        // Fell to the death
        if (transform.position.y < -2f || isDying)
        {
            Die();
            return;
        }

        if (impactForce.magnitude > 0.1f)
        {
            Knockback();

            currentMoveHorizontal = 0f;

            animator.SetBool("WasHit", false);

            return;
        }
        //else
        {
            impactForce = Vector3.zero;
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

        if (charController.isGrounded)
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

            if (Input.GetKey(KeyCode.Space) && !killedEnemy)
            {
                Jump();

            }
            
        }
        else
        {
            

            animator.SetBool("Grounded", false);

            inAir = true;
        }

        if (killedEnemy)
            Jump();

        moveDirection.x = currentMoveHorizontal * speed / 2f;
        
        moveDirection.y -= gravity * Time.deltaTime;

        charController.Move(moveDirection * Time.deltaTime);

        animator.SetFloat("MoveSpeed", currentMoveHorizontal);

    }

    void Jump()
    {
        killedEnemy = false;

        Debug.Log("Jump");

        moveDirection.y = jumpSpeed;

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
        charController.Move(impactForce * Time.deltaTime);

        impactForce = Vector3.Lerp(impactForce, Vector3.zero, 5 * Time.deltaTime);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Platform")
            return;

        if (hit.gameObject.tag == "Enemy" && hit.gameObject.GetComponent<EnemyMovement>().IsAlive)
        {
            if (hit.collider.GetType() == typeof(BoxCollider)) // Body
            {
                Debug.Log("Ouch");

                animator.SetBool("WasHit", true);

                health--;

                if (health == 0)
                {
                    isDying = true;
                    return;
                }

                impactForce = new Vector3(-2f, 4f, 0);

                //animator.SetBool("Grounded", false);
                inAir = true;
            }
            else if (hit.collider.GetType() == typeof(SphereCollider)) // Head
            {
                Debug.Log("Kill");

                KillEnemy(hit.gameObject);
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
    }


    void KillEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyMovement>().Die();

        killedEnemy = true;
        //Destroy(enemy);
    }
}
