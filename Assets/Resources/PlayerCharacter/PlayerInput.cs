using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private float JumpHeight = 375f;
    [SerializeField] private float WalkSpeed = 2f;
    [SerializeField] private float SprintModifier = 1.33f;
    [SerializeField] private float fallSpeed = 4f;

    [SerializeField] private GameObject bombObject;

    [SerializeField] private LayerMask platformLayerMask;

    private CircleCollider2D groundCollider;
    private Rigidbody2D rb2d;

    private static int RIGHT = 1;
    private static int LEFT = 2;
    private int lastmovement;

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
            lastmovement = LEFT;
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
            lastmovement = RIGHT;
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
            if (groundCollider.IsTouchingLayers(platformLayerMask))
            {
                Vector2 jumpVector = new Vector2(0, JumpHeight);
                rb2d.AddForce(jumpVector, ForceMode2D.Impulse);
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            //Place bomb to the right or left depending on which way the character is facing.
            if(lastmovement == LEFT) { Instantiate(bombObject, gameObject.transform.position + new Vector3(-1,0), Quaternion.identity); }
            if(lastmovement == RIGHT) { Instantiate(bombObject, gameObject.transform.position + new Vector3(1, 0), Quaternion.identity); }
        }
        
    }
}
