using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCalc : MonoBehaviour
{

    public int score = 0;

    private Text scoreText;
    private Text WinscreenText;

    // Start is called before the first frame update
    void Start()
    {
        Text[] texts = GameObject.FindGameObjectWithTag("HUD").GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            if (t.tag.Equals("MoneyText"))
            {
                scoreText = t;
            }
        }
        scoreText.text = "" + score;

        texts = GameObject.FindGameObjectWithTag("WinScreen").GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            if (t.name.Equals("ScoreText"))
            {
                WinscreenText = t;
            }
        }
        WinscreenText.text = "$" + score;
    }

    // Update is called once per frame
    void Update()
    {}

    public void addValue(int val)
    {
        score += val;
        if(scoreText != null)
        {
            scoreText.text = "" + score;
        }
        if(WinscreenText != null)
        {
            WinscreenText.text = "$" + score;
        }
        
        
    }   

    public int getScore()
    {
        return score;
    }
}
