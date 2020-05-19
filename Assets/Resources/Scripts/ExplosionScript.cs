using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class ExplosionScript : MonoBehaviour
{

    private Tilemap tilemap;
    private CircleCollider2D cc2d;

    [SerializeField] GameObject diamond;
    [SerializeField] GameObject ruby;
    [SerializeField] GameObject emerald;
    [SerializeField] GameObject gold;

    private float timesincecreated;

    // Start is called before the first frame update
    void Start()
    {
        GameObject grid = GameObject.FindGameObjectWithTag("Tilemap");
        tilemap = grid.GetComponentInChildren<Tilemap>();
        cc2d = GetComponent<CircleCollider2D>();

        int x_pos = (int)gameObject.transform.position.x;
        int y_pos = (int)gameObject.transform.position.y;

        for (int i = x_pos-2; i <= x_pos+2; i++)
        {
            for (int j = y_pos-2; j <= y_pos+2; j++)
            {
            
                TileBase tile = tilemap.GetTile(new Vector3Int(i, j+2, 0));
                if (tile != null)
                {
                    string tilename = tile.name;
                    if (tilename != "SupaHardRock" || tilename != "Entrance" || tilename != "Exit") { tilemap.SetTile(new Vector3Int(i, j + 2, 0), null); } //FIX EXPLOSION
                    Debug.Log(tile.name);
                    switch (tilename)
                    {
                        case "DiamondRock":
                            Instantiate(diamond, new Vector3Int(i, j + 2,0),Quaternion.identity);
                            break;
                        case "RubyRock":
                            Instantiate(ruby, new Vector3Int(i, j + 2, 0), Quaternion.identity);
                            break;
                        case "EmeraldRock":
                            Instantiate(emerald, new Vector3Int(i, j + 2, 0), Quaternion.identity);
                            break;
                        case "GoldRock":
                            Instantiate(gold, new Vector3Int(i, j + 2, 0), Quaternion.identity);
                            break;
                    }
                }

            }
        }
        timesincecreated = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timesincecreated += Time.deltaTime;

        if(timesincecreated >= 1)
        {
            Destroy(gameObject);
        }
    }
}
