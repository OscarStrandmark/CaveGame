using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    [SerializeField] private int value;

    private BoxCollider2D bc2d;

    private ScoreCalc sc;
    private BoxCollider2D playerCollider;
    private AudioSource audioSource;

    bool collected = false;

    private float timeSinceTouched;

    // Start is called before the first frame update
    void Start()
    {
        //Get script to add to score when picked up
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerCollider = player.GetComponentInChildren<BoxCollider2D>();
        sc = player.GetComponentInChildren<ScoreCalc>();
        audioSource = GetComponent<AudioSource>();
        bc2d = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCollider.IsTouching(bc2d) && !collected)
        {
            collected = true;
            sc.addValue(value);
            audioSource.Play();
            gameObject.transform.SetPositionAndRotation(new Vector3(100, 100, 0), Quaternion.identity);
        }

        if (collected)
        {
            timeSinceTouched += Time.deltaTime;
        }

        if(timeSinceTouched >= 0.33f)
        {
            Destroy(gameObject);
        }
    }


}
