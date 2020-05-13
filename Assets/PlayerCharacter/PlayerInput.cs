using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private float JumpHeight = 375f;
    [SerializeField] private float WalkSpeed = 2f;
    [SerializeField] private float SprintModifier = 1.33f;
    [SerializeField] private float fallSpeed = 4f;

    [SerializeField] private LayerMask platformLayerMask;

    private CircleCollider2D groundCollider;
    private Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        groundCollider = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
        //Moving left
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Vector2 jumpVector;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                jumpVector = new Vector2(-1 * (float)(WalkSpeed * SprintModifier), 0); //If running
            }
            else
            {
                jumpVector = new Vector2(-1 * WalkSpeed, 0); //If not running
            }
            rb2d.AddForce(jumpVector, ForceMode2D.Impulse);
        }

        //Moving right
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector2 jumpVector;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                jumpVector = new Vector2(1 * (float)(WalkSpeed * SprintModifier), 0); //If running
            }
            else
            {
                jumpVector = new Vector2(1 * WalkSpeed, 0); //If not running
            }
            rb2d.AddForce(jumpVector,ForceMode2D.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (groundCollider.IsTouchingLayers(platformLayerMask) || true) //TODO: Remove debug condition
            {
                Vector2 jumpVector = new Vector2(0, JumpHeight);
                rb2d.AddForce(jumpVector, ForceMode2D.Impulse);
            }
        }
    }
}
