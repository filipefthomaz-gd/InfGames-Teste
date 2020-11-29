using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GridGenerator : MonoBehaviour
{
    [SerializeField]
    public LevelData levelData;

    [Header ("General Prefabs")]
    public GameObject gridUnit;
    public GameObject standardNode;
    public GameObject specialNode;
    public GameObject linePrefab;

    public LayerMask gridLayer;

    private int gridSize;

    [Space]

    private int numberOfLines;
    private LineGenerator line;
    public GameObject finished;

    private float distanceFactor;
    private List<int> specialPositions;
    private Vector2[] centerPositions;
    private GameObject[] allNodes;
    private bool[] lineStatus;
    private int completedLines;

    private bool generatingLine = false;
    private bool victory;

    public static int currentLevel;

    private Color[] colors = new Color[] {Color.green, Color.blue, Color.red, Color.yellow, Color.black };

    private List<GameObject> lines = new List<GameObject>();

    // Generates a gridSize x gridSize grid centered on the screen, and adds N-times two points to be connected
    void GenerateGrid()
    {
        specialPositions = new List<int>();
        centerPositions = new Vector2[gridSize * gridSize];
        allNodes = new GameObject[gridSize * gridSize];

        //DistanceFactor makes the distance between points smaller for larger grids;
        distanceFactor = 4.5f / gridSize;

        for (int i = 0; i < numberOfLines * 2; i++)
        {
            specialPositions.Add(-1);
        }

        //Generates random positions for start and end nodes of numberOfLines lines;
        for (int i = 0; i < numberOfLines * 2; i++)
        {
            int newPosition = -1;
            while (specialPositions.Contains(newPosition))
                newPosition = Random.Range(0, gridSize * gridSize - 1);

            specialPositions[i] = newPosition;
        }


        //Generate the grid
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject gridSquare, node;

                int currentPos = x * gridSize + y; //Array position (between 0 and gridSize*gridSize - 1)

                //Actual world position of nodes / square grids
                float xWorldPos = (x - (gridSize - 1) / 2.0f ) * distanceFactor;
                float yWorldPos = (y - (gridSize - 1) / 2.0f ) * distanceFactor;

                gridSquare = Instantiate(gridUnit, new Vector3(xWorldPos, yWorldPos, 0), Quaternion.identity);

                if (specialPositions.Contains(currentPos))
                {
                    //Color of special nodes. Nodes of the same color are paired: [0,1] - Color1, [2,3] - Color2, etc....
                    var listIndex = specialPositions.FindIndex(i => i == currentPos);
                    Color nodeColor = colors[(int)Mathf.Floor((float)listIndex/2)];

                    node = Instantiate(specialNode, new Vector3(xWorldPos, yWorldPos, 0), Quaternion.identity);
                    node.GetComponent<SpriteRenderer>().color = nodeColor;
                }

                else
                    node = Instantiate(standardNode, new Vector3(xWorldPos, yWorldPos, 0), Quaternion.identity);

                //Parent grid to this GameObject and parent individual node to each grid space;
                gridSquare.transform.parent = transform;
                node.transform.parent = gridSquare.transform;
                gridSquare.transform.localScale *= 5.0f / gridSize;

                gridSquare.name = (currentPos).ToString();

                //Relation between array position with world (x,y) position stored in centerPositions
                centerPositions[currentPos] = new Vector2(xWorldPos, yWorldPos);
                allNodes[currentPos] = node;
            }
        }
    }


    //Instantiates a number of lines GO's according to how many lines are requested;
    void CreateLineGameObjects()
    {
      lineStatus = new bool[numberOfLines];

      for(int i = 0; i < numberOfLines; i++)
      {
        lineStatus[i] = false; //Initiates status of each line to false (non-completed)

        //Setting color of line equal to its extremity nodes and number each line;
        GameObject lineObject = Instantiate(linePrefab);
        lineObject.GetComponent<LineRenderer>().SetColors(colors[i], colors[i]);
        lineObject.GetComponent<LineGenerator>().lineId = i;
        lines.Add(lineObject); //Store each instantiated line on List 'lines';
      }
    }

    // Start is called before the first frame update
    void Start()
    {
      //currentLevel = 2;
      //Read values from ScriptableObject for procedural generation
        gridSize = levelData.levels[currentLevel - 1].gridSize;
        numberOfLines = levelData.levels[currentLevel - 1].numberOfLines;

        GenerateGrid();
        CreateLineGameObjects();
    }

    void UpdateLine(string touchedHouse, Touch touch)
    {
        int hitPosition = int.Parse(touchedHouse);
        if (touch.phase.ToString() == "Began")
        {
            if (specialPositions.Contains(hitPosition))//(hitPosition == startPosition || hitPosition == endPosition)
            {
                var listIndex = specialPositions.FindIndex(i => i == hitPosition);
                int currentLine = (int)Mathf.Floor((float)listIndex/2);

                line = lines[currentLine].GetComponent<LineGenerator>();
                line.ResetLine(centerPositions, specialPositions, hitPosition, allNodes);

                lineStatus[line.lineId] = false;
                generatingLine = true;
            }
            else
            {
              foreach(GameObject eachLine in lines)
              {
                LineGenerator lineGen = eachLine.GetComponent<LineGenerator>();
                if(lineGen.storedLinePoints.Count > 0)
                {
                  if(centerPositions[hitPosition] == lineGen.storedLinePoints[lineGen.storedLinePoints.Count -1])
                  {
                    line = lineGen;
                    generatingLine = true;
                    break;
                  }
                }
              }
            }
        }


        else if (touch.phase.ToString() == "Moved" && generatingLine)
        {
            line.AddLinePoint(centerPositions[hitPosition], hitPosition);

            if(!lineStatus[line.lineId])
              lineStatus[line.lineId] = line.CheckLineStatus(centerPositions[hitPosition],
                centerPositions[specialPositions[line.lineId*2]], centerPositions[specialPositions[line.lineId*2+1]], allNodes);

            if(lineStatus[line.lineId])
              generatingLine = false;

            CheckVictory();

        }

        else if (touch.phase.ToString() == "Ended")
            generatingLine = false;
    }


    void CheckVictory()
    {
      completedLines = 0;
      foreach(bool status in lineStatus)
      {
        if(status)
          completedLines++;
      }

      if (completedLines == numberOfLines)
      {
          generatingLine = false;
          victory = true;
          PlayerPrefs.SetInt("Level " + currentLevel, 1);
          finished.SetActive(true);
      }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && !victory) //if touching the screen
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, gridLayer);

            //If hitting a grid collider, UpdateLine. If not, turn line generation off;
            if (hit.collider != null)
                UpdateLine(hit.collider.name, touch);

            else
                generatingLine = false;
        }
    }

    public void BackToMenu()
    {
        currentLevel = -1;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadLevel(bool nextLevel)
    {
        if(nextLevel)
          currentLevel ++;
        SceneManager.LoadScene("TestScene");
    }
}
