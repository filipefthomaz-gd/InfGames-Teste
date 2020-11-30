using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLineManager : MonoBehaviour
{
    public LayerMask gridLayer;

    public GameObject[] allNodes;

    private int numberOfLines;
    private LineGenerator line;
    public GameObject finished;

    private float distanceFactor;
    private List<int> specialPositions = new List<int>();
    private Vector2[] centerPositions;
    private int completedLines;

    private bool generatingLine = false;
    private bool victory;

    //public static int currentLevel;

    private Color[] colors = new Color[] {Color.green, Color.blue, Color.red, Color.yellow, Color.black };

    public List<LineGenerator> lines;

    // Start is called before the first frame update
    void Start()
    {
      finished = GameObject.Find("Finished");
      finished.SetActive(false);
      foreach (LineGenerator lineGen in lines)
      {
        specialPositions.Add(lineGen.lineExtremities[0]);
        specialPositions.Add(lineGen.lineExtremities[1]);
      }
    }

    //UpdateLine
    void UpdateLine(Vector3 housePosition, string touchedHouse, Touch touch)
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
                line.ResetLine(housePosition, hitPosition, allNodes);

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

        //If person is moving its pressed finger, check if it went to a new unused position;
        else if (touch.phase.ToString() == "Moved" && generatingLine)
        {
            bool readyToAdd = true;
            if(!specialPositions.Contains(hitPosition) || hitPosition == line.lineExtremities[0] || hitPosition == line.lineExtremities[1])
            {
              foreach(LineGenerator lineGen in lines)
              {
                if(lineGen.arrayPos.Contains(hitPosition) && lineGen.lineId != line.lineId)
                  readyToAdd = false;
              }
              if(readyToAdd)
                line.AddLinePoint(housePosition, hitPosition, true);

              else
                generatingLine = false;
            }

            else
              generatingLine = false;

            if(!line.lineStatus)
              line.CheckLineStatus(housePosition, allNodes);

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

      if (completedLines == lines.Count)
      {
          generatingLine = false;
          victory = true;
          PlayerPrefs.SetInt("Level_2 " + GridGenerator.currentLevel, 1);
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
                UpdateLine(hit.collider.transform.position, hit.collider.name, touch);

            else
                generatingLine = false;
        }
    }

    //BackToMenu
    public void BackToMenu()
    {
        GridGenerator.currentLevel = -1;
        SceneManager.LoadScene("MainMenu");
    }

    //Load Next Level or Retry (nextLevel bool is defined on each Button OnClick)
    public void LoadLevel(bool nextLevel)
    {
        if(nextLevel)
          GridGenerator.currentLevel ++;

        if(GridGenerator.currentLevel == 5)
          SceneManager.LoadScene("MainMenu");
        else

          SceneManager.LoadScene("LevelScene2");
    }
  }
