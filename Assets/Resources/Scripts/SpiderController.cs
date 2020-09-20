using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class SpiderController : MonoBehaviour
{
    private System.Random random;
    
    private GameObject player;

    private BoxCollider2D damageBox;
    private Rigidbody2D rb2d;

    private AudioSource audio;

    private bool focusingPlayer;
    private float lastJumpAt;

    // Start is called before the first frame update
    void Start()
    {
        lastJumpAt = Time.time;
        player = GameObject.FindGameObjectWithTag("Player");
        audio = gameObject.GetComponentInChildren<AudioSource>();
        random = new System.Random();
        focusingPlayer = false;

        rb2d = gameObject.GetComponentInChildren<Rigidbody2D>();

        damageBox = gameObject.GetComponentInChildren<BoxCollider2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
        Debug.Log(distance);
        if(distance < 7.5f)
        {
            focusingPlayer = true;
        }
        else
        {
            focusingPlayer = false;
            if(Time.time - lastJumpAt > 3)
            {
                lastJumpAt = Time.time;
                int val = random.Next(0, 4);
                if (val == 0) // 25% chance to jump in random direction
                {
                    val = random.Next(0, 2); //50% chance of right & left
                    Debug.Log("Direction roll: " + val);
                    if (val == 1)
                    {
                        rb2d.AddForce(new Vector2(3, 5), ForceMode2D.Impulse);
                    }
                    else
                    {
                        rb2d.AddForce(new Vector2(-3, 5), ForceMode2D.Impulse);
                    }
                }
            }
        }

        if (focusingPlayer)
        {
            if (Time.time - lastJumpAt > 3)
            {
                lastJumpAt = Time.time;
                float playerx = player.transform.position.x;
                if(playerx > gameObject.transform.position.x)
                {
                    rb2d.AddForce(new Vector2(3, 5), ForceMode2D.Impulse);
                }
                else
                if(playerx < gameObject.transform.position.x)
                {
                    rb2d.AddForce(new Vector2(-3, 5), ForceMode2D.Impulse);
                }
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            collision.gameObject.GetComponentInChildren<Player_Health>().hit(1);
        }
    }
}
