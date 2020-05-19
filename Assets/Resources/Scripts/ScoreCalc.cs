using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCalc : MonoBehaviour
{

    public int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addValue(int val)
    {
        score += val;
        Debug.Log("Score: " + score);
    }
}
