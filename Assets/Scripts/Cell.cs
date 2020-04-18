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
    public Sprite[] goal_sprites;
    public Sprite[] final_goal_sprites;

    CellType type;

    // Bg - simple old bg.
    // Obstacle - stands in the way between you and goals.
    // Item - Will be spread out. Could be critical or non-critical.
    // Goal - Goals for ending the level.
    // Generation algorithm -
    //  1. Place goals, place random obstacles
    //  2. Find path to goals using A*, remove obstacles if needed
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
    
    public void SetItem()
    {
        if (this.type != CellType.BG)
        {
            return;
        }
        item.SetActive(true);
        this.type = CellType.ITEM;
        return;
    }

    public void ConsumeItem()
    {
        item.SetActive(false);
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
        goal.SetActive(true);
        goal.GetComponent<SpriteRenderer>().sprite = goal_sprites[i];
        this.type = CellType.GOAL;
        return true;
    }

    public CellType GetCellType()
    {
        return type;
    }

    public void SetFinalGoal(int i)
    {
        goal.SetActive(true);
        goal.GetComponent<SpriteRenderer>().sprite = final_goal_sprites[i];
        this.type = CellType.GOAL;
    }

    public bool PlaceObstacle(bool on)
    {
        if (on)
        {
            if (this.type != CellType.BG)
            {
                return false;
            }
            type = CellType.OBSTACLE;
        } else
        {
            type = CellType.BG;
        }

        obstacle.SetActive(on);
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
