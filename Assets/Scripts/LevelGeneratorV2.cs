using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGeneratorV2 : MonoBehaviour
{

    private Tilemap tilemap;

    private List<Chunk> chunkList = new List<Chunk>();

    private Chunk[,] chunkMatrix;

    //Tiles
    private TileBase rockTile;
    private TileBase rockHardTile;
    private TileBase entranceTile;
    private TileBase exitTile;
    private TileBase spikeTile;
    private TileBase arrowtrapLeftTile;
    private TileBase arrowtrapRightTile;

    //Tile IDs
    private static readonly int AIR = 0;
    private static readonly int ROCK = 1;
    private static readonly int SPIKES = 2;
    private static readonly int ARROWTRAPLEFT = 3;
    private static readonly int ARROWTRAPRIGHT = 4;
    private static readonly int ENTRANCE = 5;
    private static readonly int EXIT = 6;

    private int[,] pathMatrix;
    /*
     * Room types:
     * 0 - Not on vital path, no guaranteed exits
     * 1 - Room guaranteed to have exits on: right and left
     * 2 - Room guaranteed to have exits on: right, left and bottom, if there is a 2 above it, a top exit is also guaranteed 
     * 3 - Room guaranteed to have exits on: right, left and top.
     */

    void Start()
    {
        //Get tilemap
        tilemap = GetComponent<Tilemap>();

        //Get the tiles, grabbing them from a place in the tilemap. Lazy, i know.
        rockTile = tilemap.GetTile(new Vector3Int(-33, 36, 0));
        rockHardTile = tilemap.GetTile(new Vector3Int(-32, 36, 0));
        entranceTile = tilemap.GetTile(new Vector3Int(-30, 36, 0));
        exitTile = tilemap.GetTile(new Vector3Int(-29, 36, 0));
        spikeTile = tilemap.GetTile(new Vector3Int(-31, 36, 0));
        arrowtrapLeftTile = tilemap.GetTile(new Vector3Int(-28, 36, 0));
        arrowtrapRightTile = tilemap.GetTile(new Vector3Int(-27, 36, 0));

        //Place border of indestructible rock
        for (int x = -1; x <= 41; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 1, 0), rockHardTile);  //Top wall
            tilemap.SetTile(new Vector3Int(x, -40, 0), rockHardTile);//Bottom wall
        }
        for (int y = 1; y >= -40; y--)
        {
            tilemap.SetTile(new Vector3Int(-1, y, 0), rockHardTile); //Left wall
            tilemap.SetTile(new Vector3Int(41, y, 0), rockHardTile); //Right wall
        }

        //Load all chunks into memory from files.
        readChunks();

        System.Random random = new System.Random();

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                pathMatrix[row, col] = 0; //Make all rooms non-vital to begin with
            }
        }

        int entrance_X = random.Next(0, 3);
        bool done = false;
        int currentCol = entrance_X;
        int currentRow = 0;

        while (!done)
        {
            int action = random.Next(1, 6);
            /*
             * 1/2: Move left
             * 3/4: Move right
             * 5  : Move down
             */
        }
    }


    private void readChunks()
    {
        foreach (string file in System.IO.Directory.GetFiles("Assets/Chunks", "*.txt"))
        {

            //Read files and set chunk properties
            string data = File.ReadAllText(file);
            Chunk chunk = new Chunk(data);
            if (file[15] == '1') //Up
            {
                chunk.up = true;
            }
            if (file[16] == '1') //Down
            {
                chunk.down = true;
            }
            if (file[17] == '1') //Left
            {
                chunk.left = true;
            }
            if (file[18] == '1') //Right
            {
                chunk.right = true;
            }
            if (file[19] == '1') //Entrance
            {
                chunk.entrance = true;
            }
            if (file[20] == '1') //Exit
            {
                chunk.exit = true;
            }

            chunkList.Add(chunk);

        }
        Debug.Log("Chunks read: " + chunkList.Count);
    }

    private Chunk getChunkWithRequirements(bool requireUp, bool requireDown, bool requireLeft, bool requireRight, bool requireEntrance, bool requireExit)
    {
        System.Random rand = new System.Random();
        List<Chunk> goodChunks = new List<Chunk>();

        //Count amount of flags needed
        int reqSum = 0;
        if (requireUp) { reqSum++; }
        if (requireDown) { reqSum++; }
        if (requireLeft) { reqSum++; }
        if (requireRight) { reqSum++; }
        if (requireEntrance) { reqSum++; }
        if (requireExit) { reqSum++; }

        //Find suitable chunks
        foreach (Chunk c in chunkList)
        {
            int flagCount = 0;

            if (requireUp) { if (c.up) { flagCount++; } }
            if (requireDown) { if (c.down) { flagCount++; } }
            if (requireLeft) { if (c.left) { flagCount++; } }
            if (requireRight) { if (c.right) { flagCount++; } }
            if (requireEntrance) { if (c.entrance) { flagCount++; } }
            if (requireExit) { if (c.exit) { flagCount++; } }

            if (flagCount == reqSum)
            {
                goodChunks.Add(c);
            }
        }

        int index = rand.Next(0, goodChunks.Count);

        Debug.Log("Amount of good chunks found: " + goodChunks.Count);
        Debug.Log("Index chosen: " + index);

        return goodChunks[index];
    }

    private Chunk getRandomFillChunk()
    {
        System.Random r = new System.Random();
        Chunk c = chunkList[r.Next(0, chunkList.Count - 1)]; //Get random chunk to start
        while (c.entrance || c.exit) //Do until a chunk without an entrance or exit is found
        {
            c = chunkList[r.Next(0, chunkList.Count - 1)]; //Pick at random until a good one is found
        }
        return c;
    }

    private static int NONE = -1;
    private static int LEFT = 0;
    private static int RIGHT = 1;
    private static int BOTH = 2;

    private int checkSides(int row, int col, bool[,] matrix) //This will be done a bunch, which is why its in its own method
    {
        bool checkLeft = false;
        bool checkRight = false;
        if (col == 0) { checkRight = true; } //If leftmost column, only check right of chunk
        if (col == 3) { checkLeft = true; } //If rightmost column, only check left of chunk

        bool rightIsTrue = false;
        bool leftIsTrue = false;

        if (checkRight)
        {
            if (matrix[row, col + 1])
            {
                rightIsTrue = true; //Right square is true
            }
        }
        if (checkLeft)
        {
            if (matrix[row, col - 1])
            {
                leftIsTrue = true; //Left square is true
            }
        }

        int retVal = NONE;

        if (rightIsTrue && !leftIsTrue) { retVal = RIGHT; }
        else
        if (!rightIsTrue && leftIsTrue) { retVal = LEFT; }
        else
        if (rightIsTrue && leftIsTrue) { retVal = BOTH; }

        return retVal;
    }


    // Update is called once per frame, never used
    void Update() { }


}
