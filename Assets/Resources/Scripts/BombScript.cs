using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombScript : MonoBehaviour
{
    private float timer;

    [SerializeField] private GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        timer = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        timer = timer - Time.deltaTime;

        if (timer <= 0)
        {
            Instantiate(explosion, gameObject.transform.position,Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
