using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ArrowTrapLeft : MonoBehaviour
{

    [SerializeField] private GameObject ObjectToFire;
    [SerializeField] private float arrowForce;

    private BoxCollider2D detectionbox;

    private bool fired = false;

    private float x;
    private float y;

    // Start is called before the first frame update
    void Start()
    {
        BoxCollider2D[] boxes = gameObject.GetComponentsInChildren<BoxCollider2D>();
        foreach (BoxCollider2D box in boxes)
        {
            if (box.isTrigger)
            {
                detectionbox = box;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!fired && (collision.gameObject.tag.Equals("Player") || collision.gameObject.tag.Equals("Bomb") || collision.gameObject.tag.Equals("Arrow") || collision.gameObject.tag.Equals("Enemy") || collision.gameObject.tag.Equals("Spider"))  && collision.IsTouching(detectionbox))
        {
            fired = true;
            x = gameObject.transform.position.x;
            y = gameObject.transform.position.y;
            GameObject go = Instantiate(ObjectToFire, new Vector3(x - 1.5f, y), Quaternion.identity);
            go.GetComponentInChildren<Rigidbody2D>().AddForce(new Vector2(-arrowForce, 0), ForceMode2D.Impulse);
            gameObject.GetComponentInChildren<AudioSource>().Play();
        }
    }
}
