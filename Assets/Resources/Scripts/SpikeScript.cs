using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeScript : MonoBehaviour
{
    private PolygonCollider2D spikeCollider;
    private AudioSource spikeSound;

    //Player objects
    private GameObject player;
    private BoxCollider2D playerCollider;
    private Rigidbody2D playerRb2d;
    private Player_Health playerHealth;

    private bool spiked = false;

    // Start is called before the first frame update
    void Start()
    {
        spikeCollider = gameObject.GetComponentInChildren<PolygonCollider2D>();
        spikeSound = gameObject.GetComponentInChildren<AudioSource>();


        player = GameObject.FindGameObjectWithTag("Player");
        playerCollider = player.GetComponentsInChildren<BoxCollider2D>()[1];
        playerRb2d = player.GetComponentInChildren<Rigidbody2D>();
        playerHealth = player.GetComponentInChildren<Player_Health>();
    }

    // Update is called once per frame
    void Update()
    {
        if (spikeCollider.IsTouching(playerCollider) && !spiked && playerRb2d.velocity.y < 0)
        {
            spiked = true;
            spikeSound.Play();
            playerHealth.DeathBySpikes();
            Debug.Log("Spiked");
        }
    }
}
