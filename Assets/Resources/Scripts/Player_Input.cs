using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Input : MonoBehaviour
{
    //Refs to objects/components used. 
    private Rigidbody2D rigidBody2D;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    [SerializeField] private GameObject bombObject;
    private Player_Inventory inv;
    private LevelGenerator lvlGen;

    //Values for movement speed and jump height
    private float runningSpeed = 3.8f;
    private float firstJumpForce = 6.5f / 1.3f; //Divided to keep the original value in case we remove double jump

    //Objects & values relating to attacking
    [SerializeField] private float attackRadius;
    [SerializeField] private float attackDamage;
    [SerializeField] private LayerMask IsDamageable;
    [SerializeField] private Transform attackHitbox;
    private float[] attackDetails = new float[2];
    

    //Bools for diffrent character states
    private bool isAttacking;
    private bool isRunning;
    private bool isJumping;
    private bool isGrounded;
    
    //Vars for double jumping
    private int jumpsLeft;
    private float lastJumpAt;

    //Refs to groundcheck gameObjects.
    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    Transform groundCheck_Left; // To make ground-detection better when player on a ledge

    [SerializeField]
    Transform groundCheck_Right; // To make ground-detection better when player on a ledge

    //Vars to control if bombs are thrown/dropped right or left
    private static int RIGHT = 1;
    private static int LEFT = 2;
    private int lastmovement;

    //Bool for if the player is alive, set to true on death to disable movement inputs
    private bool isDead;
    private bool hasWon;

    void Start()
    {
        lastJumpAt = Time.time;
        isDead = false;
        hasWon = false;
        lastmovement = RIGHT;
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        lvlGen = GameObject.FindGameObjectWithTag("Tiles").GetComponentInChildren<LevelGenerator>();
        inv = gameObject.GetComponentInChildren<Player_Inventory>();
    }

    /*
     * Called from an event in the ninja_sword_attack animation, 
     * gets called on last animation sprite
     */
    void setIsAttackingToFalse()
    {
        isAttacking = false;
    }

    private void Update()
    {
        //Code for placing bombs, didn't work inside the FixedUpdate() for some reason. ¯\_(ツ)_/¯
        if (Input.GetKeyDown(KeyCode.D) && !isDead && !hasWon)
        {
            if (inv.useBomb())
            {
                //Place bomb to the right or left depending on which way the character is facing.
                if (lastmovement == LEFT)
                {
                    GameObject go = Instantiate(bombObject, gameObject.transform.position + new Vector3(-1, 0), Quaternion.identity);
                    if (!Input.GetKey(KeyCode.DownArrow)) //If holding down, place bomb. If not, throw it!
                    {
                        go.GetComponentInChildren<Rigidbody2D>().AddForce(new Vector2(-5f, 3f), ForceMode2D.Impulse);
                    }
                }
                else
                if(lastmovement == RIGHT)
                {
                    GameObject go = Instantiate(bombObject, gameObject.transform.position + new Vector3(1, 0), Quaternion.identity);
                    if (!Input.GetKey(KeyCode.DownArrow))//If holding down, place bomb. If not, throw it!
                    {
                        go.GetComponentInChildren<Rigidbody2D>().AddForce(new Vector2(5f, 3f), ForceMode2D.Impulse);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            lvlGen.CheckForExit((int)gameObject.transform.position.x, (int)gameObject.transform.position.y);
        }

        if (Input.GetKey(KeyCode.LeftShift)) //If holding shift, enable running speed.
        {
            runningSpeed = 5f;
        }
        else
        {
            runningSpeed = 3.8f;
        }
    }

    void FixedUpdate()
    {
        // Linecast from the grounchecks to see of any of them is touching the ground, if so, set isGrounded to true.
        if ((Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Tiles"))) ||
            (Physics2D.Linecast(transform.position, groundCheck_Left.position, 1 << LayerMask.NameToLayer("Tiles"))) ||
            (Physics2D.Linecast(transform.position, groundCheck_Right.position, 1 << LayerMask.NameToLayer("Tiles"))))
        {
            isGrounded = true;
            isJumping = false;
            jumpsLeft = 2;
        }
        else
        {
            isGrounded = false;
        }

        // Jump
        if (Input.GetKey("z") && !isDead && !hasWon)
        {
            isAttacking = false;
            //If player has jumps left and there has been at least .2 seconds since last jump. The last part is to prevent one press of the button to trigger two jumps in one frame
            //Couldnt get it to work with Input.GetKeyDown();
            if (jumpsLeft > 0 && (Time.time - lastJumpAt > 0.2f)) 
            {
                lastJumpAt = Time.time; //Set time of jump
                jumpsLeft--; //Reduce jump count by 1;
                isJumping = true; //Set jump-state
                rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, firstJumpForce); //Apply force
                animator.Play("ninja_jump"); //Play anim
            }


        }

        // Attack
        if (isGrounded && !isRunning && Input.GetKey("x") && !isDead && !hasWon)
        {
            isAttacking = true;
            animator.Play("ninja_sword_attack");

            Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attackHitbox.position, attackRadius, IsDamageable);

            attackDetails[0] = attackDamage;
            attackDetails[1] = rigidBody2D.transform.position.x;


            foreach (Collider2D collider in detectedObjects)
            {
                GameObject obj = collider.gameObject;

                obj.GetComponent<SkeletonController>().damage(attackDetails);

                if (collider.gameObject.tag.Equals("Spider"))
                {
                    Destroy(obj);
                }

            }
        }

        // Run to the right
        if (Input.GetKey("right") && !hasWon && !isDead)
        {
            lastmovement = RIGHT;
            isAttacking = false;
            isRunning = true;
            rigidBody2D.velocity = new Vector2(runningSpeed, rigidBody2D.velocity.y);
            if (isGrounded)
            {
                animator.Play("ninja_run");
            }

            else
            {
                animator.Play("ninja_fall");
            }

            spriteRenderer.flipX = false;
        }

        // Run left
        else if (Input.GetKey("left") && !isDead && !hasWon)
        {
            lastmovement = LEFT;
            isRunning = true;
            isAttacking = false;
            rigidBody2D.velocity = new Vector2(-runningSpeed, rigidBody2D.velocity.y);
            if (isGrounded)
            {
                animator.Play("ninja_run");
            }
            else
            {
                animator.Play("ninja_fall");
            }

            spriteRenderer.flipX = true;
        }

        // Idle
        else
        {
            isRunning = false;
            if (isGrounded && !isAttacking)
            {
                animator.Play("ninja_idle");
            }

            rigidBody2D.velocity = new Vector2(0, rigidBody2D.velocity.y);
        }

        //Only allow if player is dead.
        if (isDead)
        {
            if (Input.GetKey(KeyCode.Y)) //Reload game
            {
                SceneManager.LoadScene("Level");
            }
            else
            if (Input.GetKey(KeyCode.N)) //Close game
            {
                Application.Quit();
            }
        }

        if (hasWon)
        {
            if (Input.GetKey(KeyCode.R)) //Reload game
            {
                SceneManager.LoadScene("Level");
            }
            else
            if (Input.GetKey(KeyCode.E)) //Close game
            {
                Application.Quit();
            }
        }
    }


    //Called when player dies, disables input for movement
    public void setDead()
    {
        isDead = true;
    }

    //Called when the player wins, disables input for movement
    public void setWon()
    {
        hasWon = true;
    }
}
