using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class ExplosionScript : MonoBehaviour
{

    private Tilemap tilemap;
    private GameObject player;

    [SerializeField] GameObject diamond;
    [SerializeField] GameObject ruby;
    [SerializeField] GameObject emerald;
    [SerializeField] GameObject gold;

    [SerializeField] TileBase debugTile;

    private float timesincecreated;

    // Start is called before the first frame update
    void Start()
    {
        //Get objects needed
        GameObject grid = GameObject.FindGameObjectWithTag("Tilemap");
        tilemap = grid.GetComponentInChildren<Tilemap>();

        //Get hitboxes
        player = GameObject.FindGameObjectWithTag("Player");

        //If player is too close to bomb, die
        if(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.y),new Vector2(gameObject.transform.position.x,gameObject.transform.position.y)) <= 3f)
        {
            player.GetComponentInChildren<Player_Health>().DeathByExplosion();
        }
        
        //Kill spiders
        GameObject[] spiders = GameObject.FindGameObjectsWithTag("Spider");
        foreach (GameObject spider in spiders)
        {
            if (Vector3.Distance(spider.transform.position, gameObject.transform.position) <= 3f)
            {
                Destroy(spider);
            }
        }
        //Kill skeletons
        GameObject[] skels = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject skel in skels)
        {
            if (Vector3.Distance(skel.transform.position, gameObject.transform.position) <= 3f)
            {
                Destroy(skel);
            }
        }
        //Get position of explosion
        int x_pos = (int)gameObject.transform.position.x+1;
        int y_pos = (int)gameObject.transform.position.y+1;

        //Delete tiles in square around bomb
        for (int i = x_pos-2; i <= x_pos+2; i++)
        {
            for (int j = y_pos-2; j <= y_pos+2; j++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(i, j, 0));
                if (tile != null)
                {
                    string tilename = tile.name;
                    if (!tilename.Equals("SupaHardRock") && !tilename.Equals("Entrance") && !tilename.Equals("Exit")) 
                    {
                        tilemap.SetTile(new Vector3Int(i, j, 0), null); 
                    }
                    //If destroyed tile is one that contains treasure, spawn the treasure
                    switch (tilename)
                    {
                        case "DiamondRock":
                            Instantiate(diamond, new Vector3Int(i, j,0),Quaternion.identity);
                            break;
                        case "RubyRock":
                            Instantiate(ruby, new Vector3Int(i, j, 0), Quaternion.identity);
                            break;
                        case "EmeraldRock":
                            Instantiate(emerald, new Vector3Int(i, j, 0), Quaternion.identity);
                            break;
                        case "GoldRock":
                            Instantiate(gold, new Vector3Int(i, j, 0), Quaternion.identity);
                            break;
                    }
                    
                }

            }
        }
        timesincecreated = 0;

        //Since arrowtraps are not tiles, we have to handle them diffrently.
        GameObject[] arrowtraps = GameObject.FindGameObjectsWithTag("ArrowTrap");
        foreach (GameObject obj in arrowtraps)
        {
            float distance = Vector3.Distance(new Vector3(x_pos, y_pos), new Vector3(obj.transform.position.x, obj.transform.position.y));
            Debug.Log(distance);
            if(distance <= 4f)
            {
                Destroy(obj);
            }
        }
        //Same with spikes, they are not tiles...
        GameObject[] spikes = GameObject.FindGameObjectsWithTag("Spike");
        foreach (GameObject obj in spikes)
        {
            float distance = Vector3.Distance(new Vector3(x_pos, y_pos), new Vector3(obj.transform.position.x, obj.transform.position.y));
            Debug.Log(distance);
            if (distance <= 4f)
            {
                Destroy(obj);
            }
        }

        //Have explosions move treasures and arrows, for fun :-)
        GameObject[] treasures = GameObject.FindGameObjectsWithTag("ScorePickup");
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        List<GameObject> stuff = new List<GameObject>();

        foreach (GameObject item in treasures)
        {
            stuff.Add(item);
        }
        foreach (GameObject item in arrows)
        {
            stuff.Add(item);
        }

        foreach (GameObject obj in stuff)
        {
            float distance = Vector3.Distance(new Vector3(x_pos, y_pos), new Vector3(obj.transform.position.x, obj.transform.position.y));
            float deg = Vector3.Angle(new Vector3(x_pos, y_pos), new Vector3(obj.transform.position.x, obj.transform.position.y));
            if (distance <= 5f)
            {
                float xvel = 0;
                float yvel = 0;
                float force = 5;
                if(obj.tag.Equals("Arrow")) { force = 1; }

                //This logic is a mess and doesnt work correctly. Look over again if we have time. 
                if ((deg <= 45 && deg >= 0) || (deg <= 360 && deg >= 315)) { xvel += force; }
                if ((deg <= 180 && deg >= 135) || (deg <= 225 && deg >= 180)) { xvel -= force; }

                if ((deg <= 135 && deg > 90) ||(deg >= 30 && deg <= 90)) { yvel += force; }
                if ((deg <= 270 && deg > 225) || (deg >= 315 && deg <= 270)) { yvel -= force; }

                obj.GetComponentInChildren<Rigidbody2D>().AddForce(new Vector3(xvel, yvel), ForceMode2D.Impulse);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Remove gameobject after 1s to reduce clutter in gameObjects
        timesincecreated += Time.deltaTime;
        if(timesincecreated >= 1)
        {
            Destroy(gameObject);
        }
    }
}
