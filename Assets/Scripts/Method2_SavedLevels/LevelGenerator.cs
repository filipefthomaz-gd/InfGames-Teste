using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    public GeneratedLevelData genLevelData;
    [SerializeField]
    public SavedLevelData savedLevel;

    public GameObject gridUnit;
    public GameObject standardNode;
    public GameObject specialNode;
    public GameObject linePrefab;
    //public GameObject canvasPrefab;

    public LayerMask gridLayer;

    private Color[] colors = new Color[] {Color.green, Color.blue, Color.red, Color.yellow, Color.black };
    public int numberOfLevels;

    private int gridSize;
    private int numberOfLines;
    private Vector2[] extremity;

    private float distanceFactor;
    private List<int> specialPositions;


    // Generates a gridSize x gridSize grid centered on the screen, and adds N-times two points to be connected
    private void GenerateGrid(int level)
    {
      //Create all GameObjects that will compose a level
        var GO = new GameObject();
        GO.name = "Level " + level;

        var Grid = new GameObject();
        Grid.name = "Grid";

        var Lines = new GameObject();
        Lines.name = "Lines";

        LevelLineManager levelLineManager = Grid.AddComponent<LevelLineManager>();
        levelLineManager.lines = new List<LineGenerator>();

        Grid.transform.parent = GO.transform;
        Lines.transform.parent = GO.transform;

        levelLineManager.allNodes = new GameObject[gridSize * gridSize];
        specialPositions = new List<int>();

        //Add and setup lines
        for(int i = 0; i < numberOfLines; i++ )
        {
          var line = Instantiate(linePrefab, Lines.transform);
          LineGenerator lineGen = line.GetComponent<LineGenerator>();
          lineGen.SetupLine((int)extremity[i].x, (int)extremity[i].y, i, colors[i]);
          levelLineManager.lines.Add(lineGen);

          specialPositions.Add(lineGen.lineExtremities[0]);
          specialPositions.Add(lineGen.lineExtremities[1]);
        }

        levelLineManager.gridLayer = gridLayer;

        //DistanceFactor makes the distance between points smaller for larger grids;
        distanceFactor = 4.5f / gridSize;

        //Generate Grid based on gridSize and number of lines
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
                gridSquare.transform.parent = Grid.transform;
                node.transform.parent = gridSquare.transform;
                gridSquare.transform.localScale *= 5.0f / gridSize;

                gridSquare.name = (currentPos).ToString();

                //Relation between array position with world (x,y) position stored in centerPositions
                levelLineManager.allNodes[currentPos] = node;
            }
        }
    }



    // Start is called before the first frame update
    void Awake()
    {
      for(int k = 0; k < numberOfLevels; k++)
      {
        //Data needed for a level:
        // - gridSize M (generates an MxM grid)
        // - numberOfLines N (generates N lines and N*2 extremity nodes)
        // - extremity E (position of E nodes as a Vector2 corresponding to the 2 positions of one line extremities)
        
        gridSize = genLevelData.genLevels[k].gridSize;
        numberOfLines = genLevelData.genLevels[k].numberOfLines;
        extremity = genLevelData.genLevels[k].extremityPositions;

        GenerateGrid(k);
      }
    }
  }
