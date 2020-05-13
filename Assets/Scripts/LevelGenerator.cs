using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    private static readonly int AIR            = 0;
    private static readonly int ROCK           = 1;
    private static readonly int SPIKES         = 2;
    private static readonly int ARROWTRAPLEFT  = 3;
    private static readonly int ARROWTRAPRIGHT = 4;
    private static readonly int ENTRANCE       = 5;
    private static readonly int EXIT           = 6;

    private int entranceX;
    private int entranceY;

    void Start()
    {
        //Get tilemap
        tilemap = GetComponent<Tilemap>();

        //Get the tiles, grabbing them from a place in the tilemap. Lazy, i know.
        rockTile = tilemap.GetTile(new Vector3Int(-33,36,0));
        rockHardTile = tilemap.GetTile(new Vector3Int(-32, 36, 0));
        entranceTile = tilemap.GetTile(new Vector3Int(-30, 36, 0));
        exitTile = tilemap.GetTile(new Vector3Int(-29, 36, 0));
        spikeTile = tilemap.GetTile(new Vector3Int(-31, 36, 0));
        arrowtrapLeftTile = tilemap.GetTile(new Vector3Int(-28, 36, 0));
        arrowtrapRightTile = tilemap.GetTile(new Vector3Int(-27, 36, 0));

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

        //Load all chunks into memory from files.
        readChunks();

        System.Random random = new System.Random();

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
        int entrance_X = random.Next(0,4);
        int connectRow01_x = entrance_X;
        while(connectRow01_x == entrance_X) { connectRow01_x = random.Next(0, 4); }
        int connectRow12_x = random.Next(0, 4);
        int connectRow23_x = random.Next(0, 4);
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
        if((Math.Abs((entrance_X + 1) -(connectRow01_x + 1))) > 1) //Row 0
        {
            if(entrance_X > connectRow01_x)
            {
                int currentX = entrance_X;
                while(currentX != connectRow01_x)
                {
                    currentX--;
                    pathMatrix[0, currentX] = true;
                }
            }
            else
            if(entrance_X < connectRow01_x)
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
        if(conn == LEFT ) { chunkMatrix[0, entrance_X] = getChunkWithRequirements(false, false, true, false, true, false); }
        if(conn == RIGHT) { chunkMatrix[0, entrance_X] = getChunkWithRequirements(false, false, false, true, true, false); }
        if(conn == BOTH)  { chunkMatrix[0, entrance_X] = getChunkWithRequirements(false, false, true, true, true, false); }

        //Set dropdown from row 0 to row 1
        conn = checkSides(0, connectRow01_x, pathMatrix);
        if(conn == LEFT ) { chunkMatrix[0, connectRow01_x] = getChunkWithRequirements(false, true, true, false, false, false); }
        if(conn == RIGHT) { chunkMatrix[0, connectRow01_x] = getChunkWithRequirements(false, true, false, true, false, false); }
        if(conn == BOTH)  { chunkMatrix[0, connectRow01_x] = getChunkWithRequirements(false, true, true, true, false, false); }

        //Set drop from row 0 to row 1
        conn = checkSides(1, connectRow01_x, pathMatrix);
        if(conn == LEFT ) { chunkMatrix[1, connectRow01_x] = getChunkWithRequirements(true, false, true, false, false, false);}
        if(conn == RIGHT) { chunkMatrix[1, connectRow01_x] = getChunkWithRequirements(true, false, false, true, false, false);}
        if(conn == BOTH) { chunkMatrix[1, connectRow01_x] = getChunkWithRequirements(true, false, true, true, false, false); }

        //Set dropdown from row 1 from row 2
        conn = checkSides(1,connectRow12_x,pathMatrix);
        if(conn == LEFT ) { chunkMatrix[1, connectRow12_x] = getChunkWithRequirements(false, true, true, false, false, false);}
        if(conn == RIGHT) { chunkMatrix[1, connectRow12_x] = getChunkWithRequirements(false, true, false, true, false, false);}
        if(conn == BOTH)  { chunkMatrix[1, connectRow12_x] = getChunkWithRequirements(false, true, true, true, false, false); }

        //Set drop from row 1 to row 2
        conn = checkSides(2, connectRow12_x, pathMatrix);
        if(conn == LEFT ) { chunkMatrix[2, connectRow12_x] = getChunkWithRequirements(true, false, true, false, false, false);}
        if(conn == RIGHT) { chunkMatrix[2, connectRow12_x] = getChunkWithRequirements(true, false, false, true, false, false);}
        if(conn == BOTH)  { chunkMatrix[2, connectRow12_x] = getChunkWithRequirements(true, false, true, true, false, false); }

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
        int exitsCount     = 0;

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
                        int finalRow = (row*10) + chunkRow;
                        int finalCol = (col*10) + chunkCol;
                        int tileVal = tileMatrix[chunkRow, chunkCol];

                        tilesPlaced[finalRow, finalCol] = true;

                        switch (tileVal)
                        {
                            case '0':
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0), null); //Remove tile - set it as an "air" tile
                                break;
                            case '1': //Rock
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0),rockTile);
                                break;
                            case '2': //Spikes
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0),spikeTile);
                                break;
                            case '3': //ArrowtrapLeft
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0),arrowtrapLeftTile);
                                break;
                            case '4': //ArrowtrapRight
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0),arrowtrapRightTile);
                                break;
                            case '5': //Entrance
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0),entranceTile);
                                entranceX = finalCol;
                                entranceY = -finalRow;
                                GameObject playerCharacter = GameObject.FindWithTag("Player");
                                playerCharacter.transform.position = new Vector3(entranceX-1, entranceY-1);
                                entrancesCount++;
                                break;
                            case '6': //Exit
                                tilemap.SetTile(new Vector3Int(finalCol, -finalRow, 0),exitTile);
                                exitsCount++;
                                break;
                        }
                        //Debug.Log("Placed block at: (" + finalCol + "," + finalRow + ")");
                    }
                }
            }
        }

        if(entrancesCount > 1) { Debug.LogError("Too many entrances! Placed " + entrancesCount); }
        if(exitsCount > 1) { Debug.LogError("Too many exits! Placed " + exitsCount); }

        int count = 0;

        for (int row = 0; row < 40; row++)
        {
            for (int col = 0; col < 40; col++)
            {
                if(tilesPlaced[row, col]) { count++; }
            }
        }
        Debug.Log("Tiles placed: " + count);
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
            int index = rand.Next(0, goodChunks.Count);

            //Debug.Log("Amount of good chunks found: " + goodChunks.Count);
            //Debug.Log("Index chosen: " + index);

            return goodChunks[index];
        }
    }

    private Chunk getRandomFillChunk()
    {
        System.Random r = new System.Random();
        Chunk c = chunkList[r.Next(0, chunkList.Count - 1)]; //Get random chunk to start
        while(c.entrance || c.exit) //Do until a chunk without an entrance or exit is found
        {
            c = chunkList[r.Next(0, chunkList.Count - 1)]; //Pick at random until a good one is found
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


    // Update is called once per frame, never used
    void Update(){}


}
