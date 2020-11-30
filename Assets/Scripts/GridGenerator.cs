using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    private int completedLines;

    private bool generatingLine = false;
    private bool victory;

    public static int currentLevel;

    private Color[] colors = new Color[] {Color.green, Color.blue, Color.red, Color.yellow, Color.black };

    private List<LineGenerator> lines = new List<LineGenerator>();

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
      for(int i = 0; i < numberOfLines; i++)
      {
        //Setting color of line equal to its extremity nodes and number each line;
        GameObject lineObject = Instantiate(linePrefab);
        LineGenerator lineGen = lineObject.GetComponent<LineGenerator>();
        lineGen.SetupLine(specialPositions[i*2], specialPositions[i*2+1], i, colors[i]);
        lines.Add(lineGen); //Store each instantiated line on List 'lines';
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

    //UpdateLine
    void UpdateLine(string touchedHouse, Touch touch)
    {
        int hitPosition = int.Parse(touchedHouse);

        //If begin a touch, check if the touch is in a Extremety (SpecialPositions) or on the tip of an unfinished line
        if (touch.phase.ToString() == "Began")
        {
            if (specialPositions.Contains(hitPosition))
            {
                var listIndex = specialPositions.FindIndex(i => i == hitPosition);
                int currentLine = (int)Mathf.Floor((float)listIndex/2);

                line = lines[currentLine];
                line.ResetLine(centerPositions[hitPosition], hitPosition, allNodes);

                generatingLine = true;
            }
            else
            {
              foreach(LineGenerator lineGen in lines)
              {
                if((lineGen.arrayPos.Count != 0) ? (hitPosition == lineGen.arrayPos.Last()) : false)
                {
                  line = lineGen;
                  generatingLine = true;
                  break;
                }
              }
            }
        }


        else if (touch.phase.ToString() == "Moved" && generatingLine)
        {
            if(!specialPositions.Contains(hitPosition) || hitPosition == line.lineExtremities[0] || hitPosition == line.lineExtremities[1])
              line.AddLinePoint(centerPositions[hitPosition], hitPosition, true);

            else
              generatingLine = false;

            if(!line.lineStatus)
              line.CheckLineStatus(centerPositions[hitPosition], allNodes);

            if(line.lineStatus)
              generatingLine = false;

            CheckVictory();

        }

        else if (touch.phase.ToString() == "Ended")
            generatingLine = false;
    }


    //Checks if all Lines have status == true
    void CheckVictory()
    {
      completedLines = 0;
      foreach(LineGenerator lineGen in lines)
      {
        if(lineGen.lineStatus)
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

    //BackToMenu
    public void BackToMenu()
    {
        currentLevel = -1;
        SceneManager.LoadScene("MainMenu");
    }

    //Load Next Level or Retry (nextLevel bool is defined on each Button OnClick)
    public void LoadLevel(bool nextLevel)
    {
        if(nextLevel)
          currentLevel ++;

        if(currentLevel == 17)
          SceneManager.LoadScene("MainMenu");
        else

          SceneManager.LoadScene("LevelScene");
    }
}
