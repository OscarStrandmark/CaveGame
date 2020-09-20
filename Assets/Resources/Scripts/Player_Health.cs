using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Player_Health : MonoBehaviour
{

    [SerializeField] private GameObject deathMusicHolder;

    //Values
    [SerializeField] private int startHealth;
    private int health;

    //Reference to UI
    private Text lifeText;

    //Used for invis frams
    private float lasthitat;
    private bool invuln = false;
    private SpriteRenderer pcSprite;
    private float blinktime;

    //Bool to not call death-function more than once
    private bool dead = false;

    //Refs to used objects
    private Rigidbody2D rb2d;
    private AudioSource hitSound;
    private ScoreCalc score;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        hitSound = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
        score = GameObject.FindGameObjectWithTag("Player").GetComponent<ScoreCalc>();

        //Set values
        lasthitat = Time.time;
        health = startHealth;

        //Get references to stuff
        pcSprite = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<SpriteRenderer>();

        Text[] texts = GameObject.FindGameObjectWithTag("HUD").GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            if (t.tag.Equals("LifeText"))
            {
                lifeText = t;
            }
        }
        lifeText.text = "" + startHealth;
    }

    public void hit(int damage)
    {
        if(!invuln) //If hit in the last 1.6 seconds
        {
            hitSound.Play();
            rb2d.velocity = Vector3.zero;
            rb2d.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
            invuln = true;
            lasthitat = Time.time;
            health -= damage;
            lifeText.text = "" + health;

            if (health <= 0)
            {
                Death(); //End game
            }
        }
    }

    private void Death()
    {
        if (!dead) 
        {
            //Set dead
            dead = true;

            //Remove old hud.
            GameObject go = GameObject.FindGameObjectWithTag("HUD"); //Remove hud
            Destroy(go);
            
            //Disable inputs
            gameObject.GetComponentInChildren<Player_Input>().setDead(); 
            
            //Stop music
            GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<MusicRandomizer>().stopSong();
            
            //Create prefab that plays deathmusic
            Instantiate(deathMusicHolder, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y), Quaternion.identity);
           
            //Enable deathscreen
            GameObject deathScreen = GameObject.FindGameObjectWithTag("DeathHUD");
            deathScreen.GetComponentInChildren<Canvas>().enabled = true;
            
            //Set score on deathscreen
            Text[] texts = deathScreen.GetComponentsInChildren<Text>();
            foreach (Text text in texts)
            {
                if(text.name == "MoneyText")
                {
                    text.text = "$" + score.getScore();
                }
            }
            //View deathscreen
            deathScreen.SetActive(true);
        }
    }

    public void DeathBySpikes()
    {
        Death();
    }

    public void DeathByExplosion()
    {
        Death();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            hit(1);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Death();
        }

        if (Time.time - lasthitat > 1.6f)
        {
            invuln = false;
        } 


        if (invuln)
        {
            float timesincelastblink = Time.time - blinktime;
            if(timesincelastblink > 0.2f)
            {
                if (pcSprite.enabled)
                {
                    pcSprite.enabled = false;
                }
                else
                {
                    pcSprite.enabled = true;
                }
            }
        }
        else
        {
            if (!pcSprite.enabled)
            {
                pcSprite.enabled = true;
            }
        }
    }
}
