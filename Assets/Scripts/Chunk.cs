using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    private int[,] tileIDs;

    public bool up = false;
    public bool down = false;
    public bool left = false;
    public bool right = false;
    public bool entrance = false;
    public bool exit = false;

    public Chunk(string data)
    {
        tileIDs = new int[40,40];
        int index = 0;

        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                tileIDs[row,col] = data.ToCharArray()[index];
                index += 1;
            }
        }
    }

    public int[,] getChunkMatrix()
    {
        return tileIDs;
    }
}
