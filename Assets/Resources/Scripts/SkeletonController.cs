using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;

public class SkeletonController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D enemyBody;
    private Rigidbody2D playerBody;

    private float deathAnimationStarttime;

   

    private enum State { Walking, Knockback, Dead, Attack}
    private State currentState;

    private bool wallDetected, groundDetected, playerDetectedRight, playerDetectedLeft, isDamageAble = true;

    private int direction = 1, damageDirection;
    private UnityEngine.Vector2 moveVector;
    [SerializeField] private int attackDamage;
    [SerializeField] private float groundCheckDistance, wallCheckDistance,playerCheckDistance, moveSpeed, chaseSpeed, maxHealth, knockBackDuration, targetingStoppingDistance, attackRange, attackRadius,
        attackAnimationLength, deathAnimationLength;
    [SerializeField] private Transform groundCheck, wallCheck, playerCheck, attackHitBox;
    [SerializeField] private LayerMask groundLayerMask, playerCharacterLayer, IsDamageable;
    [SerializeField] private UnityEngine.Vector2 knockBackSpeed;

   
    private float currentHealth, knockBackStartTime, attackStartTime;

    private void Start()
    {
        enemyBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerBody = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Rigidbody2D>();  // ??

        
    }
    private void Awake()
    {
        currentHealth = maxHealth;

    }

    private void Update()
    {
       

        switch (currentState)
        {
            case State.Walking:
                UpdateWalkingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
            case State.Attack:
                UpdateAttackState();
                break;
        }
    }

    //--Walking state-------------------------------------

    private void EnterWalkingState()
    {
        animator.SetBool("Knockback", false);
        animator.SetBool("IsAttacking", false);
    }

    private void UpdateWalkingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, UnityEngine.Vector2.down, groundCheckDistance, groundLayerMask);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayerMask);
        playerDetectedRight = Physics2D.Raycast(playerCheck.position, transform.right, playerCheckDistance, playerCharacterLayer);
        playerDetectedLeft = Physics2D.Raycast(playerCheck.position, transform.right * -1, playerCheckDistance, playerCharacterLayer);

         if ((UnityEngine.Vector2.Distance(playerCheck.position, playerBody.transform.position) < attackRange) && animator.GetBool("IsAttacking") == false)
        {
            if (playerToLeft() && direction == 1 || playerToLeft() == false && direction == -1)
            {
                enemyBody.transform.Rotate(0f, 180f, 0f);
                direction *= -1;
            }
            SwitchState(State.Attack);
        }

       else if (!groundDetected || wallDetected)
        {
            enemyBody.transform.Rotate(0f, 180f, 0f);
            direction *= -1;
        } 
        else if (playerDetectedLeft || playerDetectedRight && (UnityEngine.Vector2.Distance(playerCheck.position, playerBody.transform.position) > targetingStoppingDistance))
        {

            if (playerToLeft() && direction == 1 || playerToLeft()==false && direction == -1)
            {
                enemyBody.transform.Rotate(0f, 180f, 0f);
                direction *= -1; 
            }
          enemyBody.position = UnityEngine.Vector2.MoveTowards(enemyBody.transform.position, playerBody.transform.position, chaseSpeed * Time.deltaTime);
   
        }
        else
        {
            
          transform.Translate(UnityEngine.Vector2.right * moveSpeed * Time.deltaTime);
        }

       
    }

    private void ExitWalkingState()
    {

    }

    //--Knockback state-----------------------------------

    private void EnterKnockbackState()
    {
        knockBackStartTime = Time.time;
        moveVector.Set(knockBackSpeed.x * damageDirection, knockBackSpeed.y);
        enemyBody.velocity = moveVector;

        animator.SetBool("Knockback", true);


    }

    private void UpdateKnockbackState()
    {
        if(Time.time >= knockBackStartTime + knockBackDuration)
        {
            isDamageAble = true;
            SwitchState(State.Walking);
        }
    }

    private void ExitKnockbackState()
    {
        animator.SetBool("Knockback", false);
    }

    //--Dead state --------------------------------------

    private void EnterDeadState()
    {
        deathAnimationStarttime = Time.time;
        animator.Play("Dead");
    }

    private void UpdateDeadState()
    {
        if(Time.time >= deathAnimationStarttime + deathAnimationLength)
        {
            Destroy(gameObject);
        }
    }

    private void ExitDeadState()
    {

    }

  // --Attack State----------------------------------------

    private void EnterAttackState()
    {
        attackStartTime = Time.time;

        animator.SetBool("IsAttacking", true);
    }

    private void UpdateAttackState()
    {
        
        if (Time.time >= attackStartTime + attackAnimationLength){ // Change state after the animation time has passed 
            if (enemyBody == GameObject.Find("Archer"))            // Make sure to check the animation length in animator.
            {
                shootArrow();
            }
            

            SwitchState(State.Walking);
        }
    }

    private void ExitAttackState()
    {
        animator.SetBool("IsAttacking", false);
    }
    // OTHER

    private void SwitchState(State state) {
        
        switch (currentState)
        {
            case State.Walking:
                ExitWalkingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
            case State.Attack:
                ExitAttackState();
                break;
        }

        switch (state)
        {
            case State.Walking:
                EnterWalkingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
            case State.Attack:
                EnterAttackState();
                break;
        }
        currentState = state;


    }

    private bool playerToLeft()
    {
        if (playerBody.transform.position.x < enemyBody.transform.position.x)
        {
            return true;  //player on left
        } else
        {
            return false;  //player on right
        }

    }

    private void checkAttackHitBox() // Called from Attack animation
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attackHitBox.position, attackRadius, IsDamageable);

        foreach (Collider2D collider in detectedObjects)
        {
            GameObject obj = collider.gameObject;

            obj.GetComponent<Player_Health>().hit(attackDamage);

        }
    }

    private void shootArrow()
    {
        
    }

    public void damage(float[] damageDetails)
    {
        if (isDamageAble)
        {
            isDamageAble = false;
            currentHealth = currentHealth - damageDetails[0];

            if (damageDetails[1] > enemyBody.transform.position.x)
            {
                damageDirection = -1;
            }
            else
            {
                damageDirection = 1;
            }

            if (currentHealth > 0)
            {
                SwitchState(State.Knockback);
            }
            else if (currentHealth <= 0.0)
            {
                SwitchState(State.Dead);
            }
        }
       
    }
}
