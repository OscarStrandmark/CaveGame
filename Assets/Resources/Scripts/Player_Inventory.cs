using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Inventory : MonoBehaviour
{
    [SerializeField] private int StartBombs;
    private int bombsLeft;

    private Text bombText;

    // Start is called before the first frame update
    void Start()
    {
        bombsLeft = StartBombs;

        Text[] texts = GameObject.FindGameObjectWithTag("HUD").GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            if (t.tag.Equals("BombText"))
            {
                bombText = t;
            }
        }
        bombText.text = "" + StartBombs;
    }

    public bool useBomb()
    {
        if(bombsLeft == 0)
        {
            return false;
        }
        else
        {
            bombsLeft--;
            bombText.text = "" + bombsLeft;
            return true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
