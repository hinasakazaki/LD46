using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelScript : MonoBehaviour
{
    public AudioManager audioManager;
    public LetterManager letterManager;
    ColorScheme color_scheme;

    public GameObject cell;
    public int rows = 14;
    public int columns = 27;
    public GameObject[,] cells;
    public Coordinates currPos = new Coordinates(0, 0);
    public List<Coordinates> activeGoals = new List<Coordinates>();
    public List<Coordinates> finalGoals = new List<Coordinates>();
    public List<Coordinates> obstacles = new List<Coordinates>();
    public HashSet<Coordinates> noObstaclesAllowed = new HashSet<Coordinates>();

    public int points = 0;
    public Text timeLabel;
    public float timeLeft;
    public Text pointsLabel;
    public Text objectiveLabel;
    public string objectiveString;
    public GameObject letterPreview;

    public bool holdingLetter = false;
    public Letter currentLetter;
    private bool toggleLetter = false;  // hide --> show, show --> hide
    private bool showNeighborObjective = false;
    string[] personANames = new List<string> { "Skye", "Noa", "Caelan", "Flynn", "Jesse" }.ToArray();
    string[] personBNames = new List<string> { "Harper", "Hayden", "Linden", "Nevada", "Oakley"}.ToArray();

    public Text personAName;
    public Text personBName;
    public Image personAImage;
    public Image personBImage;
    string personA;
    string personB;
    public bool personAHappy;
    public bool personBHappy;

    private float therapySession = 120;
    public bool gameEnded = false;
    public GameObject endScreen;
    public GameObject endGame;
    public Text endText;

    public GameObject gamestuff;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hi starting!");
        personA = personANames[Random.Range(0, personANames.Length)];
        personB = personBNames[Random.Range(0, personBNames.Length)];
        personAName.text = personA;
        personBName.text = personB;

        timeLeft = therapySession;

        objectiveString = "Objectives \n \n- Use arrow keys to move and pick up \"emotion\" letters using X.";

        SetBackgroundCells();

        SetPlayerStartingPosition();

        FillGoalsAndItems();
        Debug.Log("Finished startup");
    }

    private void SetBackgroundCells()
    {
        cells = new GameObject[rows, columns];
        // Decide color scheme first

        float x_pos = -7.7f;
        float y_pos = 4.7f;
        float cell_width = .595f;

        color_scheme = new ColorSchemes().getRandomColorScheme();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject newCell = Instantiate(cell, new Vector3(x_pos, y_pos), Quaternion.identity);
                newCell.name = "cell " + i + ", " + j;
                newCell.transform.parent = this.transform;
                newCell.GetComponent<Cell>().SetColor(color_scheme.GetRandomColor());
                cells[i, j] = newCell;
                x_pos += cell_width;
            }
            y_pos -= cell_width;
            x_pos = -7.7f; ;
        }
    }

    private void SetPlayerStartingPosition()
    {
        // Find starting place
        // Start at some place in the wall
        int four_sided_dice = Random.Range(0, 4);
        switch (four_sided_dice)
        {
            case 0:
                currPos = new Coordinates(0, Random.Range(0, columns));
                break;
            case 1:
                currPos = new Coordinates(rows - 1, Random.Range(0, columns));
                break;
            case 2:
                currPos = new Coordinates(Random.Range(0, rows), 0);
                break;
            default:
                currPos = new Coordinates(Random.Range(0, rows), columns - 1);
                break;
        }
        Debug.Log("Starting position: " + currPos.x + ", " + currPos.y);
        cells[currPos.x, currPos.y].GetComponent<Cell>().SetPlayer(true);
    }

    private void FillGoalsAndItems()
    {
        // Find two goals
        int goalCount = 2;
        int goalsPlaced = 0;
        int[] goals = GetGoals(goalCount);

        int finalGoalCount = 0;
        while (goalsPlaced < goalCount)
        {
            int x = Random.Range(0, rows);
            int y = Random.Range(0, columns);
            Coordinates goal_candidate = new Coordinates(x, y);
            if (StraightlineEstimate(currPos, goal_candidate) < rows)
            {
                continue;
            }
            if (cells[x, y].GetComponent<Cell>().SetLetter(goals[goalsPlaced]))
            {
                if (goalsPlaced == 0)
                {
                    letterManager.SetRealLetter(cells[x, y].GetComponent<Cell>(), personB, personA);
                } else
                {
                    letterManager.SetRealLetter(cells[x, y].GetComponent<Cell>(), personA, personB);
                }
                goalsPlaced += 1;
                activeGoals.Add(goal_candidate);
                Debug.Log("Goal " + goalsPlaced + " position: " + x + ", " + y);

                // Add end goals extended from active goals
                // in the future i think this will be extended to a moving target
                // add some randomness and otherwise it's just a BFS
                Coordinates path = goal_candidate;
                int dist = 6;
                for (int i = 0; i < dist; i++)
                {
                    Coordinates[] neighbors = FindValidNeighbors(path);
                    if (neighbors.Length == 0)
                    {
                        Debug.LogError("Things are bad (no final goals available) so reloading scene.");
                        ReloadScene();
                    }
                    Coordinates randomNeighbor = neighbors[Random.Range(0, neighbors.Length)];
                    while (randomNeighbor.Equals(goal_candidate))
                    {
                        neighbors = FindValidNeighbors(path);
                        randomNeighbor = neighbors[Random.Range(0, neighbors.Length)];
                    }
                    noObstaclesAllowed.Add(path);
                    path = randomNeighbor;
                }
                // finally add final goal
                 if (goalsPlaced == 1)
                {
                    personAImage.sprite = cells[path.x, path.y].GetComponent<Cell>().SetPerson(personA);
                } else
                {
                    personBImage.sprite = cells[path.x, path.y].GetComponent<Cell>().SetPerson(personB);
                }
                finalGoals.Add(path);
                finalGoalCount += 1;
                Debug.Log("Final goal added " + path.x + ", " + path.y);
            }
        }

        // Fill some random obstacles
        int randomObstacleCount = Random.Range(20, 30);
        int obstaclesPlaced = 0;
        while (obstaclesPlaced < randomObstacleCount)
        {
            Coordinates obstacle_candidate = new Coordinates(Random.Range(0, rows), Random.Range(0, columns));
            if (!noObstaclesAllowed.Contains(obstacle_candidate) && cells[obstacle_candidate.x, obstacle_candidate.y].GetComponent<Cell>().PlaceObstacle(true))
            {
                obstaclesPlaced += 1;
                obstacles.Add(obstacle_candidate);
            }
        }
        Debug.Log("Random obstacles added");

        // A* search from start to goals
        foreach (Coordinates goal in activeGoals)
        {
            List<Coordinates> pathToGoal = AstarFindPath(goal);
            while (pathToGoal.Count == 0)
            {
                RemoveRandomObstacle();
                pathToGoal = AstarFindPath(goal);
            }
            foreach (Coordinates c in pathToGoal.ToArray())
            {
                //cells[c.x, c.y].GetComponent<Cell>().SetColor(new Color32(0xFE, 0xE2, 0xE1, 0xFF));
                noObstaclesAllowed.Add(c);
                if (!(currPos.Equals(c)) && Random.Range(0, 6) == 1)
                {
                    // Put items at a 16% chance
                    cells[c.x, c.y].GetComponent<Cell>().SetItem();
                }

            }
        }
        Debug.Log("Items along path added.");

        // For the rest of the cells, fill with random items, fake goals, and more obstacles.
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (!(i == currPos.x && j == currPos.y))
                {
                    if (Random.Range(0, 40) == 1)
                    {
                        // Put letter in at 10% chance
                        cells[i, j].GetComponent<Cell>().SetLetter(0);
                        string person1 = personA;
                        string person2 = personB;
                        if (Random.Range(0, 2) == 1)
                        {
                            person1 = personB;
                            person2 = personA;
                        }
                        Debug.Log("Trying to put fake letter.");
                        bool success = letterManager.SetFakeLetter(cells[i, j].GetComponent<Cell>(), person1, person2);
                        if (!success)
                        {
                            cells[i, j].GetComponent<Cell>().PickupLetter();
                            Debug.Log("Failed to put fake letter. It's okay");
                        }
                    }
                    else if (Random.Range(0, 4) == 1)
                    {
                        // Put items at a 25% chance
                        Debug.Log("Trying to put item.");
                        cells[i, j].GetComponent<Cell>().SetItem();
                    }
                    else if (!noObstaclesAllowed.Contains(new Coordinates(i, j)) && Random.Range(0, 2) == 1)
                    {
                        // 50% obstacle
                        Debug.Log("Trying to put obstacle.");
                        cells[i, j].GetComponent<Cell>().PlaceObstacle(true);
                    }
                }
            }
        }
        Debug.Log("Fake goals, items, and obstacles added.");
    }

    private bool NeighborIsPerson()
    {
        bool neighborIsPerson = false;
        if (currPos.x + 1 < rows && cells[currPos.x + 1, currPos.y].GetComponent<Cell>().GetCellType() == Cell.CellType.PERSON)
        {
            neighborIsPerson = true;
        }

        // down
        if (currPos.x - 1 >= 0 && cells[currPos.x - 1, currPos.y].GetComponent<Cell>().GetCellType() == Cell.CellType.PERSON)
        {
            neighborIsPerson = true;
        }

        // left
        if (currPos.y - 1 >= 0 && cells[currPos.x, currPos.y - 1].GetComponent<Cell>().GetCellType() == Cell.CellType.PERSON)
        {
            neighborIsPerson = true;
        }

        // right
        if (currPos.y + 1 < columns && cells[currPos.x, currPos.y + 1].GetComponent<Cell>().GetCellType() == Cell.CellType.PERSON)
        {
            neighborIsPerson = true;
        }
        return neighborIsPerson;
    }

    private void RemoveRandomObstacle()
    {
        int removeIndex = Random.Range(0, obstacles.Count);
        cells[obstacles[removeIndex].x, obstacles[removeIndex].y].GetComponent<Cell>().PlaceObstacle(false);
    }

    private List<Coordinates> AstarFindPath(Coordinates goal)
    {
        HashSet<Coordinates> closedSet = new HashSet<Coordinates>();
        HashSet<Coordinates> openSet = new HashSet<Coordinates>
        {
            currPos
        };
        Dictionary<Coordinates, Coordinates> cameFrom = new Dictionary<Coordinates, Coordinates>();
        Dictionary<Coordinates, int> costFromStart = new Dictionary<Coordinates, int>
        {
            [currPos] = 0
        };
        Dictionary<Coordinates, int> passThroughCostHeuristic = new Dictionary<Coordinates, int>
        {

            [currPos] = StraightlineEstimate(currPos, goal)
        };

        while (openSet.Count != 0)
        {
            // Find node in openSet with lowest passThroughCostHeuristic
            Coordinates curr = FindLowestHeuristicScore(openSet, passThroughCostHeuristic);
            if (curr.Equals(goal))
            {
                return ReconstructPath(cameFrom, curr);
            }

            openSet.Remove(curr);
            closedSet.Add(curr);
            foreach (Coordinates neighbor in FindValidNeighbors(curr))
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }
                int tentative_score = costFromStart[curr] + 1;
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                // cost from start is all initialized to infinity
                else if (costFromStart.ContainsKey(neighbor) && tentative_score >= costFromStart[neighbor])
                {
                    continue;
                }
                cameFrom[neighbor] = curr;
                costFromStart[neighbor] = tentative_score;
                passThroughCostHeuristic[neighbor] = costFromStart[neighbor] + StraightlineEstimate(neighbor, goal);
            }
        }

        return new List<Coordinates>();
    }

    private Coordinates[] FindValidNeighbors(Coordinates coordinates)
    {
        List<Coordinates> neighbors = new List<Coordinates>();

        // up
        if (coordinates.x + 1 < rows && cells[coordinates.x+1, coordinates.y].GetComponent<Cell>().IsValidNeighbor())
        {
            neighbors.Add(new Coordinates(coordinates.x + 1, coordinates.y));
        }

        // down
        if (coordinates.x - 1 >= 0 && cells[coordinates.x - 1, coordinates.y].GetComponent<Cell>().IsValidNeighbor())
        {
            neighbors.Add(new Coordinates(coordinates.x - 1, coordinates.y));
        }

        // left
        if (coordinates.y - 1 >= 0 && cells[coordinates.x, coordinates.y - 1].GetComponent<Cell>().IsValidNeighbor())
        {
            neighbors.Add(new Coordinates(coordinates.x, coordinates.y - 1));
        }

        // right
        if (coordinates.y + 1 < columns && cells[coordinates.x, coordinates.y + 1].GetComponent<Cell>().IsValidNeighbor())
        {
            neighbors.Add(new Coordinates(coordinates.x, coordinates.y + 1));
        }

        return neighbors.ToArray();
    }

    private Coordinates FindLowestHeuristicScore(HashSet<Coordinates> set, Dictionary<Coordinates, int> heuristicScores)
    {
        float min = Mathf.Infinity;
        Coordinates currMin = new Coordinates(0, 0);
        foreach (Coordinates c in set)
        {
            if (heuristicScores[c] < min)
            {
                currMin = c;
                min = heuristicScores[c];
            }
        }
        return currMin;
    }

    private List<Coordinates> ReconstructPath(Dictionary<Coordinates, Coordinates> cameFrom,
        Coordinates curr)
    {
        List<Coordinates> path = new List<Coordinates>();
        while (cameFrom.ContainsKey(curr)) {
            Coordinates old_curr = curr;
            curr = cameFrom[curr];
            path.Add(curr);
            cameFrom.Remove(old_curr);
        }

        return path;
    }

    private int StraightlineEstimate(Coordinates start, Coordinates end)
    {
        return (int) Mathf.Sqrt(Mathf.Pow((float)(start.x - end.x), 2f) +
            Mathf.Pow((float)(start.y - end.y), 2f));
     }

    // Randomly generates goals from list of goals
    private int[] GetGoals(int how_many_goals)
    {
        int total_possible_goals = 2;
        List<int> goals = new List<int>();

        while (goals.Count < how_many_goals)
        {
            int candidate = Random.Range(0, total_possible_goals);
            if (goals.Contains(candidate) != true)
            {
                goals.Add(candidate);
            }
        }

        return goals.ToArray();
    }

    public float timePass = 0f;

    private void SetTimer()
    {
        timeLeft = therapySession - Time.timeSinceLevelLoad;
        string mins = "00";
        string secs = "00";
        if (timeLeft > 60)
        {
            mins = ((int)timeLeft / 60).ToString();
        }

        if (timeLeft > 0)
        {
            int secs_int = ((int)timeLeft % 60);
            if (secs_int < 10)
            {
                secs = "0" + secs_int.ToString();
            }
            else
            {
                secs = secs_int.ToString();
            }
        }
        timeLabel.text = mins + ":" + secs.Substring(0, 2);
    }

    private void SetPoints()
    {
        pointsLabel.text = points.ToString();
    }

    private void SetObjective()
    {
        objectiveLabel.text = objectiveString;

        if (holdingLetter && !showNeighborObjective && NeighborIsPerson())
        {
            objectiveString = "Objectives \n - To open / close current letter, press Y. \n -  NEW!! If you think the letter contains \"True feelings\", deliver it to the recipient by pressing X while on the same square as them.";
            showNeighborObjective = true;
        }
    }

    private void HandleGameEnd()
    {
        letterManager.ShowLetter(currentLetter, toggleLetter);
        endScreen.SetActive(true);
        gamestuff.SetActive(false);
        endGame.SetActive(true);
        if (personAHappy && personBHappy && points > 30)
        {
            endScreen.GetComponent<SpriteRenderer>().color = color_scheme.GetBg1();
            endText.text = "Well done!! \n\n" + personA + " and " + personB + " made progress in understanding each others' feelings and it seems like their love will go on! \n\nYou finished this appointment on time with " + timeLeft + " seconds to spare and you got a very good online review for scoring " + points + " in this session. \n\n"
                + "To go to your next appointment, refresh.";

        }
        else if (personAHappy && personBHappy)
        {
            endScreen.GetComponent<SpriteRenderer>().color = color_scheme.GetBg2();
            endText.text = "Well done!! \n\n" + personA + " and " + personB + " made progress in understanding each others' feelings and it seems like their love will go on! \n\nYou finished this appointment on time with " + timeLeft + " seconds to spare and you got a very good online review DESPITE scoring " + points + " in this session. \n\n"
                + "To go to your next appointment, refresh.";

        }
        else if (personAHappy || personBHappy && points > 30)
        {
            endScreen.GetComponent<SpriteRenderer>().color = color_scheme.GetBg3();
            endText.text = "Nice try! \n\n" + personA + " and " + personB + " made some progress in understanding each others' feelings but there is a lot of work to be done. \n\nYou got a positive online review for your help and you scored " + points + " in this session. \n\n"
    + "To go to your next appointment, refresh.";
        } else if (points > 30)
        {
            endScreen.GetComponent<SpriteRenderer>().color = Color.gray;
            endText.color = Color.white;
            endText.text = "Nice try! \n\n" + personA + " and " + personB + " did not make much progress in understanding each other, but you got a positive online review for your help by scoring " + points + " in this session. \n\n"
       + "To go to your next appointment, refresh.";

        } else
        {
            endScreen.GetComponent<SpriteRenderer>().color = Color.black;
            endText.color = Color.white;
            endText.text = "Too bad! \n\n" + personA + " and " + personB + " did not make much progress in understanding each other, and you got a negative online review for scoring " + points + " in this session. \n\n"
       + "To go to your next appointment, press R.";
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene("main", LoadSceneMode.Single);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (gameEnded)
        {
            return;
        }

        if ((personAHappy && personBHappy) || (timeLeft <= 0)) {
            gameEnded = true;
            HandleGameEnd();
            return;
        }

        SetTimer();
        SetPoints();
        SetObjective();

        
        // Do letter toggle input handling here before timing is weird
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (holdingLetter)
            {
                letterManager.ShowLetter(currentLetter, toggleLetter);
                toggleLetter = !toggleLetter;
                audioManager.LetterSound();
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            Cell currCell = cells[currPos.x, currPos.y].GetComponent<Cell>();
            if (currCell.GetCellType() == Cell.CellType.LETTER)
            {
                if (!holdingLetter)
                {
                    objectiveString = "Objectives \n - NEW!! To open/close current letter, press Y. \n - To swap with a new letter, press X on top of letter.";
                    currentLetter = currCell.letterObject;
                    currCell.PickupLetter();
                    holdingLetter = true;
                    audioManager.LetterSound();
                }
                else
                {
                    // swap letters
                    currCell.PickupLetter();
                    currCell.SetLetter(0);
                    Letter tmpNewLetter = currCell.letterObject;

                    letterManager.SetLetterWithExistingLetter(currCell, currentLetter);

                    holdingLetter = true;
                    audioManager.LetterSound();
                    currentLetter = tmpNewLetter;
                }
                SetLetterPreview();
            } else if (currCell.GetCellType() == Cell.CellType.PERSON)
            {
                if (!holdingLetter)
                {
                    objectiveString = "Objective \n - Get letters using X, assess whether they capture true emotions with Y. \n - Then, give it to the recipient using X. \n - Swap letters using X.";

                    return;
                }
                else if (currCell.GetPersonName() != currentLetter.GetTo())
                {

                    objectiveString = "Tip \n - Looks like that letter wasn't addressed to that person. Try another one! \n - Remember you can swap letters using X.";
                    return;
                }
                else if (currentLetter.IsReal())
                {
                    points += 10;
                    currCell.SetHappy();
                    if (personA.Equals(currCell.GetPersonName()) && !personAHappy)
                    {
                        personAHappy = true;
                        audioManager.HappySound();
                    } else if (personB.Equals(currCell.GetPersonName()) && !personBHappy)
                    {
                        personBHappy = true;
                        audioManager.HappySound();
                    }
                    if (!(personAHappy && personBHappy))
                    {
                        objectiveString = "Great job!! \n Now make sure the communication goes both way by doing the same for your other client.";
                    }
                }
                else
                {
                    // Fake letter
                    points -= 10;
                    audioManager.SadSound();
                    currCell.SetUnhappy();
                    objectiveString = "Tip \n - Looks like that letter didn't capture true emotions. Try another one! \n - Remember you can swap letters using X.";
                }

            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.LogError("Reloading scene from command.");
            ReloadScene();
        }


        // Start input handling. We don't want to take input at every frame because it's too quick.
        if (timePass < .05f)
        {
            timePass += Time.deltaTime;
            return;
        }

        timePass = 0f;

        bool move = false;
        int newX = currPos.x;
        int newY = currPos.y;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            move = true;
            newX -= 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            move = true;
            newX += 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            move = true;
            newY -= 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            move = true;
            newY += 1;
        }
        if (move && MoveProcess(newX, newY))
        {
            Debug.Log("New player position: " + newX + ", " + newY);
            cells[currPos.x, currPos.y].GetComponent<Cell>().SetPlayer(false);
            cells[newX, newY].GetComponent<Cell>().SetPlayer(true);
            currPos.x = newX;
            currPos.y = newY;
        }
    }

    private void SetLetterPreview(bool active = true)
    {
        letterPreview.SetActive(active);
        string letterContent = currentLetter.ReadContent();
        letterPreview.GetComponent<Text>().text = "Letter preview: " + letterContent.Substring(0, Mathf.Min(letterContent.Length, 35));
    }
    
    private bool MoveProcess(int newX, int newY)
    {
        if (newX < 0 || newX >= rows || newY < 0 || newY >= columns)
        {
            return false;
        }
        Cell newCell = cells[newX, newY].GetComponent<Cell>();
        if (newCell.GetCellType() == Cell.CellType.OBSTACLE) {
            return false;
        }
        if (newCell.GetCellType() == Cell.CellType.ITEM)
        {
            newCell.ConsumeItem();
            audioManager.ItemSound();
            points += 1;
        }

        audioManager.Walk();
        return true;
    }
}


public class Coordinates
{
    public int x;
    public int y;
    public Coordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode()
    {
        return (x << 2) ^ y;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType())) return false;
        Coordinates c = (Coordinates)obj;
        return (x == c.x && y == c.y);
    }
}