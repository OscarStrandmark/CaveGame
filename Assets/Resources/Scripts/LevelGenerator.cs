using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelGenerator : MonoBehaviour
{
    /**
         * A level:
         * A level is 40x40 tiles, these tiles are divided into 4x4 chunks of tiles.
         * 
         * How a level is generated:
         * One chunk on the top layer is randomly chosen to as the entrance chunk, this chunk has to have an entrance Tile.
         * One chunk on the bottom layer is randomly chosen as the exit chunk, this chunk has to have an exit Tile.
         * A path is generated through the level this way:
         * From the entrance chunk the generator steps a random amount of chunks to the left or right. This chunk is the "dropdown" chunk, where the player will move down to the next level.
         * The generator steps right or left a random amount of steps, the chosen chunk has a connector downwards. Repeat this until the last row of chunks.
         * On the last row, instead of dropping down create the exit.
         * Any chunk that is not included in the "critical path" generated gets random chunks. Only a path to the exit is guaranteed. 
         * (This is not the algorithm used, but its easier to explain this way)
         * 
         * The chunks in the "Chunks" folder have a naming scheme that helps the generator pick the chunks.
         * 
         * The scheme is:
         * ABCDEF_[NAME]
         * 
         * ABCDEF are 1 or 0 depending on if it exists or not in the chunk.
         * A = Connector up
         * B = Connector down
         * C = Connector left
         * D = Connector right
         * E = Chunk has an entrance
         * F = Chunk has an exit
         * 
         * */

    //Refs to objects or components. 
    private Tilemap tilemap;
    private List<Chunk> chunkList = new List<Chunk>();
    private Chunk[,] chunkMatrix;
    private System.Random random;
    private MusicRandomizer music;

    //Tiles
    [SerializeField] private TileBase rockTile;
    [SerializeField] private TileBase rockHardTile;
    [SerializeField] private TileBase entranceTile;
    [SerializeField] private TileBase exitTile;

    //Unused after turning these tiles into prefabs
    //[SerializeField] private TileBase spikeTile;
    //[SerializeField] private TileBase arrowTrapLeftTile;
    //[SerializeField] private TileBase arrowTrapRightTile;

    //Treasure rocks
    [SerializeField] private TileBase rockGoldTile;
    [SerializeField] private TileBase rockEmeraldTile;
    [SerializeField] private TileBase rockRubyTile;
    [SerializeField] private TileBase rockDiamondTile;

    //Treasure items
    [SerializeField] private GameObject treasureGold;
    [SerializeField] private GameObject treasureEmerald;
    [SerializeField] private GameObject treasureRuby;
    [SerializeField] private GameObject treasureDiamond;


    //Trap prefabs
    [SerializeField] private GameObject arrowTrapLeft;
    [SerializeField] private GameObject arrowTrapRight;
    [SerializeField] private GameObject spikePrefab;

    [SerializeField] private GameObject RedEnemy;
    [SerializeField] private GameObject ShieldSkeleton;
    [SerializeField] private GameObject SpearSkeleton;
    [SerializeField] private GameObject SwordSkeleton;
    [SerializeField] private GameObject Spider;


    //Tile IDs - Used to load chunk files
    private static readonly int AIR            = 0;
    private static readonly int ROCK           = 1;
    private static readonly int SPIKES         = 2;
    private static readonly int ARROWTRAPLEFT  = 3;
    private static readonly int ARROWTRAPRIGHT = 4;
    private static readonly int ENTRANCE       = 5;
    private static readonly int EXIT           = 6;

    //Entrance coordinates
    private int entranceX;
    private int entranceY;

    //Exit coordinates
    private int exitX;
    private int exitY;

    //Var to count level the player is on.
    private int levelcounter = 0;

    void Start()
    {
        music = GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<MusicRandomizer>();
        generateLevel();
    }

    //if(levelcounter == 0) { }

    private void generateLevel()
    {
        random = new System.Random();

        if(levelcounter == 0)
        {
            //Get tilemap
            tilemap = GetComponent<Tilemap>();
        }
        

        for (int x = 0; x < 40; x++)
        {
            for (int y = 0; y > -40; y--)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }


        if (levelcounter == 0) 
        {
            //Place border of indestructible rock
            for (int x = -1; x <= 40; x++)
            {
                tilemap.SetTile(new Vector3Int(x, 1, 0), rockHardTile);  //Top wall
                tilemap.SetTile(new Vector3Int(x, -40, 0), rockHardTile);//Bottom wall
            }
            for (int y = 1; y >= -40; y--)
            {
                tilemap.SetTile(new Vector3Int(-1, y, 0), rockHardTile); //Left wall
                tilemap.SetTile(new Vector3Int(40, y, 0), rockHardTile); //Right wall
            }

            tilemap.FloodFill(new Vector3Int(-5, 5, 0), rockHardTile);

            //Load all chunks into memory from files.
            readChunks();
        }
   
        if(levelcounter > 0)
        {
            //Remove all old arrowtraps and arrows when generating new level
            GameObject[] arrowtraps = GameObject.FindGameObjectsWithTag("ArrowTrap");
            foreach (GameObject obj in arrowtraps)
            {
                Destroy(obj);
            }
            GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
            foreach (GameObject obj in arrows)
            {
                Destroy(obj);
            }
            GameObject[] spikes = GameObject.FindGameObjectsWithTag("Spike");
            foreach (GameObject obj in spikes)
            {
                Destroy(obj);
            }
            GameObject[] spiders = GameObject.FindGameObjectsWithTag("Spider");
            foreach (GameObject obj in spiders)
            {
                Destroy(obj);
            }
            GameObject[] skels = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject obj in skels)
            {
                Destroy(obj);
            }
        }
        

        //Matrix for marking critical path. 
        bool[,] pathMatrix = new bool[4, 4];
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                pathMatrix[row, col] = false;
            }
        }

        //Randomize the positions where the layers will connect, also force them to not be the same as the last one. 
        int entrance_X = random.Next(0, 4);
        int connectRow01_x = entrance_X;
        while (connectRow01_x == entrance_X) { connectRow01_x = random.Next(0, 4); }
        int connectRow12_x = random.Next(0, 4);
        while (connectRow12_x == connectRow01_x) { connectRow12_x = random.Next(0, 4); }
        int connectRow23_x = random.Next(0, 4);
        while (connectRow12_x == connectRow23_x) { connectRow23_x = random.Next(0, 4); }
        int exit_x = connectRow23_x;
        while (exit_x == connectRow23_x) { exit_x = random.Next(0, 4); }

        //-----------------------------------------------------------------

        //Place important pieces on pathMatrix
        pathMatrix[0, entrance_X] = true;

        pathMatrix[0, connectRow01_x] = true;
        pathMatrix[1, connectRow01_x] = true;

        pathMatrix[1, connectRow12_x] = true;
        pathMatrix[2, connectRow12_x] = true;

        pathMatrix[2, connectRow23_x] = true;
        pathMatrix[3, connectRow23_x] = true;

        pathMatrix[3, exit_x] = true;

        //Fill chunks between important pieces in pathMatrix
        if ((Math.Abs((entrance_X + 1) - (connectRow01_x + 1))) > 1) //Row 0
        {
            if (entrance_X > connectRow01_x)
            {
                int currentX = entrance_X;
                while (currentX != connectRow01_x)
                {
                    currentX--;
                    pathMatrix[0, currentX] = true;
                }
            }
            else
            if (entrance_X < connectRow01_x)
            {
                int currentX = entrance_X;
                while (currentX != connectRow01_x)
                {
                    currentX++;
                    pathMatrix[0, currentX] = true;
                }
            }
        }

        if ((Math.Abs((connectRow01_x + 1) - (connectRow12_x + 1))) > 1) //Row 1
        {
            if (connectRow01_x > connectRow12_x)
            {
                int currentX = connectRow01_x;
                while (currentX != connectRow12_x)
                {
                    currentX--;
                    pathMatrix[1, currentX] = true;
                }
            }
            else
            if (connectRow01_x < connectRow12_x)
            {
                int currentX = connectRow01_x;
                while (currentX != connectRow12_x)
                {
                    currentX++;
                    pathMatrix[1, currentX] = true;
                }
            }
        }

        if ((Math.Abs((connectRow12_x + 1) - (connectRow23_x + 1))) > 1) //Row 2
        {
            if (connectRow12_x > connectRow23_x)
            {
                int currentX = connectRow12_x;
                while (currentX != connectRow23_x)
                {
                    currentX--;
                    pathMatrix[2, currentX] = true;
                }
            }
            else
            if (connectRow12_x < connectRow23_x)
            {
                int currentX = connectRow12_x;
                while (currentX != connectRow23_x)
                {
                    currentX++;
                    pathMatrix[2, currentX] = true;
                }
            }
        }

        if ((Math.Abs((connectRow23_x + 1) - (exit_x + 1))) > 1) //Row 3
        {
            if (connectRow23_x > exit_x)
            {
                int currentX = connectRow23_x;
                while (currentX != exit_x)
                {
                    currentX--;
                    pathMatrix[3, currentX] = true;
                }
            }
            else
            if (connectRow23_x < exit_x)
            {
                int currentX = connectRow23_x;
                while (currentX != exit_x)
                {
                    currentX++;
                    pathMatrix[3, currentX] = true;
                }
            }
        }

        //-----------------------------------------------------------------

        //Now a critical path through the map has been decided on, now translate that path to chunks and create the map
        //
        chunkMatrix = new Chunk[4, 4]; //Create a matrix to store what chunk needs to be put where

        //Fill chunks not on vital path with random chunks.
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (!pathMatrix[row, col])
                {
                    chunkMatrix[row, col] = getRandomFillChunk();
                }
            }
        }

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (pathMatrix[row, col])
                {
                    chunkMatrix[row, col] = getChunkWithRequirements(false, false, true, true, false, false);
                }
            }
        }

        //-----------------------------------------------------------------

        int conn; //Var for storing the result of checking sides. 

        //ALL these rows generate the right chunks for chunks on the critical path

        //Set entrance
        conn = checkSides(0, entrance_X, pathMatrix);
        if (conn == LEFT) { chunkMatrix[0, entrance_X] = getChunkWithRequirements(false, false, true, false, true, false); }
        if (conn == RIGHT) { chunkMatrix[0, entrance_X] = getChunkWithRequirements(false, false, false, true, true, false); }
        if (conn == BOTH) { chunkMatrix[0, entrance_X] = getChunkWithRequirements(false, false, true, true, true, false); }

        //Set dropdown from row 0 to row 1
        conn = checkSides(0, connectRow01_x, pathMatrix);
        if (conn == LEFT) { chunkMatrix[0, connectRow01_x] = getChunkWithRequirements(false, true, true, false, false, false); }
        if (conn == RIGHT) { chunkMatrix[0, connectRow01_x] = getChunkWithRequirements(false, true, false, true, false, false); }
        if (conn == BOTH) { chunkMatrix[0, connectRow01_x] = getChunkWithRequirements(false, true, true, true, false, false); }

        //Set drop from row 0 to row 1
        conn = checkSides(1, connectRow01_x, pathMatrix);
        if (conn == LEFT) { chunkMatrix[1, connectRow01_x] = getChunkWithRequirements(true, false, true, false, false, false); }
        if (conn == RIGHT) { chunkMatrix[1, connectRow01_x] = getChunkWithRequirements(true, false, false, true, false, false); }
        if (conn == BOTH) { chunkMatrix[1, connectRow01_x] = getChunkWithRequirements(true, false, true, true, false, false); }

        //Set dropdown from row 1 from row 2
        conn = checkSides(1, connectRow12_x, pathMatrix);
        if (conn == LEFT) { chunkMatrix[1, connectRow12_x] = getChunkWithRequirements(false, true, true, false, false, false); }
        if (conn == RIGHT) { chunkMatrix[1, connectRow12_x] = getChunkWithRequirements(false, true, false, true, false, false); }
        if (conn == BOTH) { chunkMatrix[1, connectRow12_x] = getChunkWithRequirements(false, true, true, true, false, false); }

        //Set drop from row 1 to row 2
        conn = checkSides(2, connectRow12_x, pathMatrix);
        if (conn == LEFT) { chunkMatrix[2, connectRow12_x] = getChunkWithRequirements(true, false, true, false, false, false); }
        if (conn == RIGHT) { chunkMatrix[2, connectRow12_x] = getChunkWithRequirements(true, false, false, true, false, false); }
        if (conn == BOTH) { chunkMatrix[2, connectRow12_x] = getChunkWithRequirements(true, false, true, true, false, false); }

        //Set dropdown from row 2 from row 3
        conn = checkSides(2, connectRow23_x, pathMatrix);
        if (conn == LEFT) { chunkMatrix[2, connectRow23_x] = getChunkWithRequirements(false, true, true, false, false, false); }
        if (conn == RIGHT) { chunkMatrix[2, connectRow23_x] = getChunkWithRequirements(false, true, false, true, false, false); }
        if (conn == BOTH) { chunkMatrix[2, connectRow23_x] = getChunkWithRequirements(false, true, true, true, false, false); }

        //Set drop from row 2 to row 3
        conn = checkSides(3, connectRow23_x, pathMatrix);
        if (conn == LEFT) { chunkMatrix[3, connectRow23_x] = getChunkWithRequirements(true, false, true, false, false, false); }
        if (conn == RIGHT) { chunkMatrix[3, connectRow23_x] = getChunkWithRequirements(true, false, false, true, false, false); }
        if (conn == BOTH) { chunkMatrix[3, connectRow23_x] = getChunkWithRequirements(true, false, true, true, false, false); }

        //Set exit
        conn = checkSides(3, exit_x, pathMatrix);
        if (conn == LEFT) { chunkMatrix[3, exit_x] = getChunkWithRequirements(false, false, true, false, false, true); }
        if (conn == RIGHT) { chunkMatrix[3, exit_x] = getChunkWithRequirements(false, false, false, true, false, true); }
        if (conn == BOTH) { chunkMatrix[3, exit_x] = getChunkWithRequirements(false, false, true, true, false, true); }

        //DEBUG
        bool[,] tilesPlaced = new bool[40, 40];
        for (int row = 0; row < 40; row++)
        {
            for (int col = 0; col < 40; col++)
            {
                tilesPlaced[row, col] = false;
            }
        }

        int entrancesCount = 0;
        int exitsCount = 0;

        int gold = 0;
        int emerald = 0;
        int ruby = 0;
        int diamond = 0;

        //Now for placing out all 1600 tiles, one chunk at a time
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                Chunk currentChunk = chunkMatrix[row, col];
                int[,] tileMatrix = currentChunk.getChunkMatrix();

                for (int chunkRow = 0; chunkRow < 10; chunkRow++)
                {
                    for (int chunkCol = 0; chunkCol < 10; chunkCol++)
                    {
                        int finalRow = (row * 10) + chunkRow;
                        int finalCol = (col * 10) + chunkCol;
                        int tileVal = tileMatrix[chunkRow, chunkCol];

                        tilesPlaced[finalRow, finalCol] = true;

                        switch (tileVal)
                        {
                            case '0':
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), null); //Remove tile - set it as an "air" tile
                               
                                //if success on a 5% roll, spawn a random piece of treasure in the tile
                                int randomval = random.Next(1, 101);
                                if(randomval > 95)
                                {
                                     randomval = random.Next(0, 101);
                                    if(randomval >= 50 && randomval < 75)
                                    {
                                        Instantiate(Spider, new Vector3Int(finalCol, -finalRow, 0), Quaternion.identity);
                                    }
                                    else
                                    if (randomval >= 75 && randomval <= 85) //Place gold
                                    {
                                        Instantiate(treasureGold, new Vector3Int(finalCol, -finalRow, 0), Quaternion.identity);
                                    }
                                    else
                                    if (randomval >= 90 && randomval <= 95) //Place emerald
                                    {
                                        Instantiate(treasureEmerald, new Vector3Int(finalCol, -finalRow, 0), Quaternion.identity);
                                    }
                                    else
                                    if (randomval >= 96 && randomval <= 98) //Place ruby
                                    {
                                        Instantiate(treasureRuby, new Vector3Int(finalCol, -finalRow, 0), Quaternion.identity);
                                    }
                                    else
                                    if (randomval >= 100 && randomval <= 100) //Place diamond
                                    {
                                        Instantiate(treasureDiamond, new Vector3Int(finalCol, -finalRow, 0), Quaternion.identity);
                                    }
                                }
                                break;
                            case '1': //Rock (Or rock with treasure)

                                //Roll for if the rock will contain treasure
                                int val = random.Next(0, 101);

                                if (val >= 75 && val <= 85) //Place gold
                                {
                                    tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), rockGoldTile);
                                    gold++;
                                }
                                else
                                if (val >= 90 && val <= 95) //Place emerald
                                {
                                    tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), rockEmeraldTile);
                                    emerald++;
                                }
                                else
                                if (val >= 96 && val <= 98) //Place ruby
                                {
                                    tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), rockRubyTile);
                                    ruby++;
                                }
                                else
                                if (val >= 100 && val <= 100) //Place diamond
                                {
                                    tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), rockDiamondTile);
                                    diamond++;
                                }
                                else //Place nothing
                                {
                                    tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), rockTile);
                                }
                                break;
                            case '2': //Spikes
                                Instantiate(spikePrefab, new Vector3(finalCol - 0.48f, -finalRow - 1.17f), Quaternion.identity);
                                break;
                            case '3': //ArrowtrapLeft
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), null);
                                Instantiate(arrowTrapLeft,new Vector3(finalCol-0.48f,-finalRow-1.17f),Quaternion.identity);
                                break;
                            case '4': //ArrowtrapRight
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), null);
                                Instantiate(arrowTrapRight, new Vector3(finalCol-0.48f, -finalRow-1.17f), Quaternion.identity);
                                break;
                            case '5': //Entrance
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), entranceTile);
                                entranceX = finalCol;
                                entranceY = -finalRow;
                                GameObject playerCharacter = GameObject.FindWithTag("Player");
                                playerCharacter.transform.position = new Vector3(entranceX - 1, entranceY - 1);
                                entrancesCount++;
                                break;
                            case '6': //Exit
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), exitTile);
                                exitX = finalCol;
                                exitY = -finalRow;
                                exitsCount++;
                                break;
                        }
                        //Debug.Log("Placed block at: (" + finalCol + "," + finalRow + ")");
                    }
                }
            }
        }
        //Generate enemies, skiten fungerar inte alls xd
        bool generateEnemies = true;
        if(generateEnemies)
        {
            for (int row = 0; row < 40; row++)
            {
                for (int col = 0; col < 40; col++)
                {
                    if (tilemap.GetTile(new Vector3Int(row, -col, 0)) == null)
                    {
                        TileBase tileBelow = tilemap.GetTile(new Vector3Int(col, -row - 1, 0));
                        TileBase tileBelowRight = tilemap.GetTile(new Vector3Int(col + 1, -row - 1, 0));
                        TileBase tileBelowleft = tilemap.GetTile(new Vector3Int(col - 1, -row - 1, 0));

                        Debug.Log("debug");

                        if ((tileBelow != null && tileBelowleft != null && tileBelowRight != null) || true)
                        {
                            int val = random.Next(0, 101);
                            if (val >= 95) // 5% to spawn enemy
                            {
                                val = random.Next(0, 101);
                                bool spawnRed = (val >= 0 && val < 10) && false;
                                bool spawnSpear = (val >= 10 && val < 20) && false;
                                bool spawnShield = (val >= 20 && val < 45) && false;
                                bool spawnSword = (val >= 45 && val < 70) && false;
                                bool spawnSpider = (val >= 70 && val <= 100) && true;
                                if (spawnRed) { Instantiate(RedEnemy, new Vector3Int(col, -row, 0), Quaternion.identity); }
                                else
                                if (spawnSpear) { Instantiate(SpearSkeleton, new Vector3Int(col, -row, 0), Quaternion.identity); }
                                else
                                if (spawnShield) { Instantiate(ShieldSkeleton, new Vector3Int(col, -row, 0), Quaternion.identity); }
                                else
                                if (spawnSword) { Instantiate(SwordSkeleton, new Vector3Int(col, -row, 0), Quaternion.identity); }
                                else
                                if (spawnSpider) { Instantiate(Spider, new Vector3Int(col, -row, 0), Quaternion.identity); }
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log("Gold placed: " + gold);
        Debug.Log("Emeralds placed: " + emerald);
        Debug.Log("Rubies placed: " + ruby);
        Debug.Log("Diamonds placed: " + diamond);

        if (entrancesCount > 1) { Debug.LogError("Too many entrances! Placed " + entrancesCount); }
        if (exitsCount > 1) { Debug.LogError("Too many exits! Placed " + exitsCount); }

        int count = 0;

        for (int row = 0; row < 40; row++)
        {
            for (int col = 0; col < 40; col++)
            {
                if (tilesPlaced[row, col]) { count++; }
            }
        }
        Debug.Log("Tiles placed: " + count);
        music.playSong();
    }

    private void readChunks()
    {
        foreach (string file in System.IO.Directory.GetFiles("Assets/Chunks", "*.txt"))
        {

            //Read files and set chunk properties
            string data = File.ReadAllText(file);
            Chunk chunk = new Chunk(data);
            string boolstring = "" + file[14] + file[15] + file[16] + file[17] + file[18] + file[19];
            if (boolstring[0] == '1') //Up
            {
                chunk.up = true;
            }
            if (boolstring[1] == '1') //Down
            {
                chunk.down = true;
            }
            if (boolstring[2] == '1') //Left
            {
                chunk.left = true;
            }
            if (boolstring[3] == '1') //Right
            {
                chunk.right = true;
            }
            if (boolstring[4] == '1') //Entrance
            {
                chunk.entrance = true;
            }
            if (boolstring[5] == '1') //Exit
            {
                chunk.exit = true;
            }

            chunkList.Add(chunk);

        }
        Debug.Log("Chunks read: " + chunkList.Count);
    }

    private Chunk getChunkWithRequirements(bool requireUp, bool requireDown, bool requireLeft, bool requireRight, bool requireEntrance, bool requireExit)
    {
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

            //Do not allow chunks containing entrance or exit tiles unless specified, we only want 1 entrance and exit per level.
            if(!requireEntrance && c.entrance) { flagCount = 0; }
            if(!requireExit && c.exit) { flagCount = 0; }

            if(flagCount == reqSum)
            {
                goodChunks.Add(c);
            }
        }

        if(goodChunks.Count == 0)
        {
            string errStr = "CHUNK NEEDED: ";
            if (requireUp) { errStr += "up "; }
            if (requireDown) { errStr += "down "; }
            if (requireLeft) { errStr += "left "; }
            if (requireRight) { errStr += "right "; }
            if (requireEntrance) { errStr += "entrance "; }
            if (requireExit) { errStr += "exit "; }
            Debug.LogError(errStr);
            return null;
        }
        else
        {
            int index = random.Next(0, goodChunks.Count);

            //Debug.Log("Amount of good chunks found: " + goodChunks.Count);
            //Debug.Log("Index chosen: " + index);

            return goodChunks[index];
        }
    }

    private Chunk getRandomFillChunk()
    {
        Chunk c = chunkList[random.Next(0, chunkList.Count)]; //Get random chunk to start
        while(c.entrance || c.exit) //Do until a chunk without an entrance or exit is found
        {
            c = chunkList[random.Next(0, chunkList.Count)]; //Pick at random until a good one is found
        }
        return c;
    }

    private static int NONE = -1;
    private static int LEFT  = 0;
    private static int RIGHT = 1;
    private static int BOTH  = 2;

    private int checkSides(int row, int col, bool[,] matrix) //This will be done a bunch, which is why its in its own method
    {
        bool checkLeft = true;
        bool checkRight = true;
        if(col == 0) { checkLeft = false; } //If leftmost column, only check right of chunk
        if(col == 3) { checkRight  = false; } //If rightmost column, only check left of chunk

        bool rightIsTrue = false;
        bool leftIsTrue = false;

        if (checkRight)
        {
            if (matrix[row, col + 1])
            {
                rightIsTrue = true; //Right square is true
            }
        }
        if(checkLeft)
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

    public void CheckForExit(int x, int y)
    {

        if(x+1 == exitX && y+1 == exitY)
        {
            levelcounter++;
            if (levelcounter < 4)
            {
                GameObject[] pickupList = GameObject.FindGameObjectsWithTag("ScorePickup");
                foreach (GameObject i in pickupList)
                {
                    Destroy(i);
                }
                generateLevel();
            }
            else
            {
                //Win game.
                GameObject go = GameObject.FindGameObjectWithTag("HUD"); //Remove hud
                Destroy(go);

                go = GameObject.FindGameObjectWithTag("WinScreen");
                go.GetComponentInChildren<Canvas>().enabled = true;
                Text[] texts = go.GetComponentsInChildren<Text>();
                GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<MusicRandomizer>().stopSong();
                go.GetComponentInChildren<AudioSource>().Play();

                GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Player_Input>().setWon();
            }

        }


    }

    // Update is called once per frame, never used
    void Update(){}


}
