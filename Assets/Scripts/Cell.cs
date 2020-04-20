using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cell : MonoBehaviour
{
    public Color color;
    public GameObject player;
    public GameObject letter;
    public GameObject person;
    public GameObject item; 
    public GameObject obstacle;
    public Sprite[] letter_sprites;
    public Sprite[] person_sprites;
    public Sprite[] obstacle_sprites;

    public Letter letterObject;
    public string personName;
    public static HashSet<int> used_ppl = new HashSet<int>();

    public SpriteRenderer dialogBox;
    public SpriteRenderer heart;
    public SpriteRenderer brokenHeart;

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
    public enum CellType { BG, OBSTACLE, ITEM, LETTER, PERSON };

    // Start is called before the first frame update
    void Start()
    {

    }

    public string GetPersonName()
    {
        return personName;
    }

    public void SetLetterObject(Letter letter, string to, string from)
    {
        letterObject = letter;
        letterObject.SetTo(to);
        letterObject.SetFrom(from);
    }

    public void SetLetterObject(Letter letter)
    {
        letterObject = letter;
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
        type = CellType.BG;
    }

    public void SetPlayer(bool on)
    {
        player.SetActive(on);
    }

    public bool SetLetter(int i)
    {
        if (this.type != CellType.BG)
        {
            return false;
        }
        letter.SetActive(true);
        letter.GetComponent<SpriteRenderer>().sprite = letter_sprites[i];
        this.type = CellType.LETTER;
        return true;
    }

    public void PickupLetter()
    {
        letter.SetActive(false);
        this.type = CellType.BG;
    }

    public CellType GetCellType()
    {
        return type;
    }

    public Sprite SetPerson(string name)
    {
        Debug.Log("Hello~");

        int index = Random.Range(0, person_sprites.Length);
        while (used_ppl.Contains(index))
        {
            index = Random.Range(0, person_sprites.Length);
        }
        Debug.Log("Using index " + index + "for person sprite " + name);
        used_ppl.Add(index);
        person.SetActive(true);
        person.GetComponent<SpriteRenderer>().sprite = person_sprites[index];
        this.personName = name;
        this.type = CellType.PERSON;

        return person_sprites[index];
    }

    public void SetHappy()
    {
        dialogBox.enabled = true;
        heart.enabled = true;
        brokenHeart.enabled = false;
    }

    public void SetUnhappy()
    {
        dialogBox.enabled = true;
        heart.enabled = false;
        brokenHeart.enabled = true;
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
        obstacle.GetComponent<SpriteRenderer>().sprite = obstacle_sprites[Random.Range(0, obstacle_sprites.Length)];
        return true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        used_ppl = new HashSet<int>();
    }

}
