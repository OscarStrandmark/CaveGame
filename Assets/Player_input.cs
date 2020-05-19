using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Player_input : MonoBehaviour
{

    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;

    private float runningSpeed = 3.8f;
    private float firstJumpForce = 4.7f;
    private float secondJumpForce = 3.7f;
    private bool canDoubleJump = false;

    bool isGrounded;

    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    Transform groundCheck_Left;

    [SerializeField]
    Transform groundCheck_Right;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {

        if (isGrounded)
        {
            canDoubleJump = true;
        }

        if (Input.GetKey("z"))
        {
            if (isGrounded)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, firstJumpForce);
                animator.Play("ninja_jump");
            }
            else {
                if (canDoubleJump)
                {

                    rb2d.velocity = new Vector2(rb2d.velocity.x, firstJumpForce);
                    animator.Play("ninja_jump");
                    canDoubleJump = false;
                }
            }
        }

        if ((Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Tiles"))) ||
            (Physics2D.Linecast(transform.position, groundCheck_Left.position, 1 << LayerMask.NameToLayer("Tiles"))) ||
            (Physics2D.Linecast(transform.position, groundCheck_Right.position, 1 << LayerMask.NameToLayer("Tiles"))))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetKey("right"))
        {
            rb2d.velocity = new Vector2(runningSpeed, rb2d.velocity.y);
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
        else if (Input.GetKey("left"))
        { 
            rb2d.velocity = new Vector2(-runningSpeed, rb2d.velocity.y);
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

        else
        {
            if (isGrounded)
            {
                animator.Play("ninja_idle");
            }

            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }

        
    }
}
