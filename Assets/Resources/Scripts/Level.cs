using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level
{
    private Chunk[,] chunks;

    public Level()
    {
        chunks = new Chunk[4,4];
    }

    public void setChunk(Chunk c,int row,int col)
    {
        chunks[row, col] = c;
    }
    
    public Chunk GetChunk(int row,int col)
    {
        return chunks[row, col];
    }
}
