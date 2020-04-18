using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Color color;
    public GameObject player;
    public GameObject goal;
    public GameObject item;
    public GameObject obstacle;
    public Sprite[] sprites;

    CellType type;

    // Bg - simple old bg.
    // Obstacle - stands in the way between you and goals.
    // Item - Will be spread out. Could be critical or non-critical.
    // Goal - Goals for ending the level.
    // Generation algorithm -
    //  1. Place goals
    //  2. Find path to goals
    //  3. Add items on the path to goals
    //  4. Excepting the paths
    //     - Add noncrucial items at random (25%?)
    //     - Add obstacles (50%?)
    public enum CellType { PLAYER, BG, OBSTACLE, ITEM, GOAL };
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool IsValidNeighbor()
    {
        return this.type != CellType.OBSTACLE;
    }

    public void SetColor(Color color)
    {
        this.color = color;
        this.GetComponent<SpriteRenderer>().color = color;
        this.type = CellType.BG;
    }

    public void SetPlayer(bool on)
    {
        player.SetActive(on);
        if (on)
        {
            this.type = CellType.PLAYER;   
        } else
        {
            this.type = CellType.BG;
        }
    }

    public bool SetGoal(int i)
    {
        if (this.type == CellType.PLAYER)
        {
            return false;
        }
        // TODO: Add distance checks to make sure it's far from player
        goal.GetComponent<SpriteRenderer>().sprite = sprites[i];
        this.type = CellType.GOAL;
        return true;
    }

    public CellType GetCellType()
    {
        return type;
    }

    public bool PlaceObstacle()
    {
        if (this.type != CellType.BG)
        {
            return false;
        }

        obstacle.SetActive(true);
        type = CellType.OBSTACLE;
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
