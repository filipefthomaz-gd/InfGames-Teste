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
    public LevelData levelData;

    public int numberOfLevels;

    private int gridSize;
    private int numberOfLines;

    private float distanceFactor;
    private List<int> specialPositions;
    private Vector2[] centerPositions;


    // Generates a gridSize x gridSize grid centered on the screen, and adds N-times two points to be connected
    void GenerateGrid(int level)
    {
        specialPositions = new List<int>();
        centerPositions = new Vector2[gridSize * gridSize];

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

                centerPositions[currentPos] = new Vector2(xWorldPos, yWorldPos);
            }
        }

        genLevelData.genLevels[level].gridPositions = new Vector2[gridSize * gridSize];
        genLevelData.genLevels[level].specialArrayPos = new int[numberOfLines*2];

        for(int i = 0; i< centerPositions.Length; i++)
        {
          genLevelData.genLevels[level].gridPositions[i] = centerPositions[i];
        }

        for(int i = 0; i< specialPositions.Count; i++)
        {
          genLevelData.genLevels[level].specialArrayPos[i] = specialPositions.ToArray()[i];
        }
    }



    // Start is called before the first frame update
    void Start()
    {
      for(int k = 0; k < numberOfLevels; k++)
      {
        gridSize = levelData.levels[k].gridSize;
        numberOfLines = levelData.levels[k].numberOfLines;

        GenerateGrid(k);
      }
    }
  }
