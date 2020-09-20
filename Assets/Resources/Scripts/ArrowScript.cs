using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{

    private Rigidbody2D rb2d;
    private bool broken = false;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = gameObject.GetComponentInChildren<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string collisionTag = collision.gameObject.tag;

        if(!collisionTag.Equals("ArrowTrap") && !collisionTag.Equals("Arrow"))
        {
            if (!broken)
            {
                broken = true;
                float velx = Math.Abs(rb2d.velocity.x);
                float vely = Math.Abs(rb2d.velocity.y);
                if((velx + vely) / 2 > 1)
                {
                    if (collisionTag.Equals("Player"))
                    {
                        broken = true;
                        collision.gameObject.GetComponentInChildren<Player_Health>().hit(1);
                    }

                    if (collision.Equals("Enemy"))
                    {
                        broken = true;
                        float[] attackDetails = new float[2];
                        attackDetails[0] = 1;
                        attackDetails[1] = rb2d.transform.position.x;
                        collision.GetComponent<SkeletonController>().damage(attackDetails);
                    }
                }
            }
        }
    }
}
