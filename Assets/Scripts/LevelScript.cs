using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelScript : MonoBehaviour
{
    public AudioManager audioManager;
    public LetterManager letterManager;

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
    public Text pointsLabel;
    public Text objectiveLabel;
    public string objectiveString;
    public GameObject letterPreview;

    public bool holdingLetter = false;
    public Letter currentLetter;
    private bool toggleLetter = false;  // hide --> show, show --> hide

    string[] personANames = new List<string> { "Skye", "Noa", "Caelan", "Flynn", "Jesse" }.ToArray();
    string[] personBNames = new List<string> { "Harper", "Hayden", "Linden", "Nevada", "Oakley"}.ToArray();

    string personA;
    string personB;

    // Start is called before the first frame update
    void Start()
    {
        personA = personANames[Random.Range(0, personANames.Length)];
        personB = personBNames[Random.Range(0, personBNames.Length)];

        objectiveString = "Objectives \n \n- Pick up \"emotion\" letters using X and find the true emotions.";

        SetBackgroundCells();

        SetPlayerStartingPosition();

        FillGoalsAndItems();
    }

    private void SetBackgroundCells()
    {
        cells = new GameObject[rows, columns];
        // Decide color scheme first

        float x_pos = -7.7f;
        float y_pos = 4.7f;
        float cell_width = .59f;
         
        ColorScheme color_scheme = new ColorScheme();
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
                    letterManager.SetRealLetter(cells[x, y].GetComponent<Cell>(), personA, personB);
                } else
                {
                    letterManager.SetRealLetter(cells[x, y].GetComponent<Cell>(), personB, personA);
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
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
                cells[path.x, path.y].GetComponent<Cell>().SetPerson(finalGoalCount);
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
                        letterManager.SetFakeLetter(cells[i, j].GetComponent<Cell>(), person1, person2);
                    }
                    else if (Random.Range(0, 4) == 1)
                    {
                        // Put items at a 25% chance
                        cells[i, j].GetComponent<Cell>().SetItem();
                    }
                    else if (!noObstaclesAllowed.Contains(new Coordinates(i, j)) && Random.Range(0, 2) == 1)
                    {
                        // 50% obstacle
                        cells[i, j].GetComponent<Cell>().PlaceObstacle(true);
                    }
                }
            }
        }
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
        float seconds = Time.timeSinceLevelLoad;
        string mins = "00";
        string secs = "00";
        if (seconds > 60)
        {
            mins = ((int)seconds / 60).ToString();
        }

        if (seconds > 0)
        {
            int secs_int = ((int)seconds % 60);
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
    }

    // Update is called once per frame
    void Update()
    {
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
                    objectiveString = "Objectives \n \n - NEW!! To open current letter, press Y. \n - To swap with a new letter, press X on top of letter.";
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
            }

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
        else if (Input.GetKey(KeyCode.R))
        {
            Debug.LogError("Reloading scene from command.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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