using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicRandomizer : MonoBehaviour
{
    private AudioSource AS_A;
    private AudioSource AS_B;
    private AudioSource AS_C;
    private AudioSource AS_Secret;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start called");
        AudioSource[] sources = gameObject.GetComponentsInChildren<AudioSource>();
        AS_A = sources[0];
        AS_B = sources[1];
        AS_C = sources[2];
        AS_Secret = sources[3];
        foreach (AudioSource audioSource in sources)
        {
            audioSource.loop = true;
        }

    }

    //Randomize what song is playing at the start of every level
    public void playSong()
    {
        //For some reason scripts are instantiated in a diffrent order when the scene is loaded via a script
        //This is to prevent nullpointers since this method is called in the start() of another script.
        if(AS_A == null || AS_B == null || AS_C == null || AS_Secret == null) { Start(); }

        AS_A.Stop();
        AS_B.Stop();
        AS_C.Stop();
        AS_Secret.Stop();

        System.Random rand = new System.Random();

        //Randomize what song to play. 5% chance to play secret song. If it is not played the other songs are 33.33...% chance
        int val = rand.Next(1, 101);
        if(val > 95)
        {
            AS_Secret.Play();
        } 
        else
        {
            val = rand.Next(1, 4);
            switch (val)
            {
                case 1:
                    AS_A.Play();
                    break;
                case 2:
                    AS_B.Play();
                    break;
                case 3:
                    AS_C.Play();
                    break;
            }
        }
    }

    public void stopSong()
    {
        AS_A.Stop();
        AS_B.Stop();
        AS_C.Stop();
        AS_Secret.Stop();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
