using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    public GameObject cell;
    public int rows = 17;
    public int columns = 27;
    public GameObject[,] cells;
    public Coordinates currPos = new Coordinates(0, 0);
    public List<Coordinates> activeGoals = new List<Coordinates>();

    // Start is called before the first frame update
    void Start()
    {
        cells = new GameObject[rows, columns];
        // Decide color scheme first

        float x_pos = -7.7f;
        float y_pos = 4.7f;
        float cell_width = .59f;

        // Set background cells
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

        // Find starting place
        currPos.x = Random.Range(0, rows);
        currPos.y = Random.Range(0, columns);
        Debug.Log("Starting position: " + currPos.x + ", " + currPos.y);
        cells[currPos.x, currPos.y].GetComponent<Cell>().SetPlayer(true);

        // Find two goals
        int goalCount = 2;
        int goalsPlaced = 0;
        int[] goals = GetGoals(2);
        while (goalsPlaced < goalCount)
        {
            int x = Random.Range(0, rows);
            int y = Random.Range(0, columns);
            if (cells[x, y].GetComponent<Cell>().SetGoal(goals[goalsPlaced]))
            {
                goalsPlaced += 1;
                activeGoals.Add(new Coordinates(x, y));
                Debug.Log("Goal " + goalsPlaced + " position: " + x + ", " + y);
            }
        }

        // Fill some random obstacles
        int randomObstacleCount = Random.Range(34, 54);
        int obstaclesPlaced = 0;
        while (obstaclesPlaced < randomObstacleCount)
        {
            if (cells[Random.Range(0, rows), Random.Range(0, columns)].GetComponent<Cell>().PlaceObstacle())
            {
                obstaclesPlaced += 1;
            }
        }

        // A* search from start to goals
        foreach (Coordinates goal in activeGoals)
        {
            List<Coordinates> pathToGoal = AstarFindPath(goal);
            foreach (Coordinates c in pathToGoal.ToArray())
            {
                Debug.Log("Path: " + c.x + ", " + c.y);
                cells[c.x, c.y].GetComponent<Cell>().SetColor(new Color32(0xFE, 0xE2, 0xE1, 0xFF));
            }

        }
        
       
        // Fill out more

        // 
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

        Debug.Log("HINA" + passThroughCostHeuristic[currPos]);

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

    // Update is called once per frame
    void Update()
    {
        bool move = false;
        int newX = currPos.x;
        int newY = currPos.y;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            move = true;
            newX -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            move = true;
            newX += 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            move = true;
            newY -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
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

    bool MoveProcess(int newX, int newY)
    {
        if (cells[newX, newY].GetComponent<Cell>().GetCellType() != Cell.CellType.BG) {
            return false;
        }
        // If item, pick it up
        // If goal, take them
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