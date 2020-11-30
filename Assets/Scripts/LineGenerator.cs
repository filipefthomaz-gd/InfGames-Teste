using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class LineGenerator : MonoBehaviour
{
    public int lineId;
    public List<Vector2> storedLinePoints = new List<Vector2>();
    public List<int> arrayPos = new List<int>();
    public ParticleSystem victoryPs;

    private LineRenderer line;
    private AudioSource audioSource;
    public int[] lineExtremities = new int[2];

    private bool addedPoint;
    public bool lineStatus = false;


    void Start()
    {
      audioSource = GetComponent<AudioSource>();
    }

    public void SetupLine(int extremity1, int extremity2, int id, Color lineColor)
    {
      //Set ID
      lineId = id;

      line = GetComponent<LineRenderer>();
      line.SetColors(lineColor, lineColor);

      //Set Extremities
      lineExtremities[0] = extremity1;
      lineExtremities[1] = extremity2;
    }



    //Functino that deals with point addition to the line
    public void AddLinePoint(Vector2 newPoint, int arrayPosition, bool moving)
    {
        //If point is new, add it to the line
        if (!storedLinePoints.Contains(newPoint))
        {
          if(!moving || (moving && (newPoint.x == storedLinePoints.Last().x || newPoint.y == storedLinePoints.Last().y)))
          {
            arrayPos.Add(arrayPosition); //Actual position
            storedLinePoints.Add(newPoint); // add the new point to our saved list of line points
            line.positionCount = storedLinePoints.Count; // set the line’s vertex count to how many points we now have, which will be 1 more than it is currently
            line.SetPosition(storedLinePoints.Count - 1, newPoint);
            addedPoint = true;
          }
        }
        //If point is old and the one before the current point, remove it
        else if(newPoint != storedLinePoints.Last())
        {
          if(newPoint == storedLinePoints[storedLinePoints.Count-2])
          {
            storedLinePoints.Remove(storedLinePoints.Last());
            arrayPos.Remove(arrayPos.Last());
            line.positionCount -= 1;
          }
          addedPoint = false;
        }
        else
          addedPoint = false;
    }

    //Check line status: True is line completed; False is line not completed
    public bool CheckLineStatus(Vector2 newPoint, GameObject[] nodes)
    {
      if(!lineStatus)
      {
        if (addedPoint)
        {
            if (arrayPos.Contains(lineExtremities[0]) && arrayPos.Contains(lineExtremities[1]))
                return CompletedLine(nodes);

            else
                return false;
        }
        return false;
      }

      else
        return true;
    }


    //Called after a line has been completed
    private bool CompletedLine(GameObject[] nodes)
    {
      audioSource.Play(); //play sound

      lineStatus = true;

      foreach(int position in arrayPos)
      {
        nodes[position].GetComponent<SpriteRenderer>().color = line.startColor; //Colorize the node

        //Add a small ParticleSystem emiiter to each node of the line
        ParticleSystem ps= Instantiate(victoryPs, nodes[position].transform);
        var main = ps.main;
        var emission = ps.emission;
        main.startColor = line.startColor;
        emission.enabled = true;
        Destroy(ps, 2f);
      }
      return true;
    }


    //Remove Entire Line (To be used when someone re-initiates a line by touching on one of its extremities)
    public void RemoveAllPoints()
    {
        storedLinePoints = new List<Vector2>();
        arrayPos = new List<int>();
        line.positionCount = 0;
    }


    //Reset line; If line is completed or midway and one presses one of the extremity nodes;
    public void ResetLine(Vector2 newPoint, int hitPos, GameObject[] nodes)
    {
      foreach(int position in arrayPos)
      {
        if(position != lineExtremities[0] && position != lineExtremities[1])
          nodes[position].GetComponent<SpriteRenderer>().color = Color.white;
      }
      lineStatus = false;
      RemoveAllPoints();
      AddLinePoint(newPoint, hitPos, false);
    }

   /* void RemoveLastLinePoint()
    {
        storedLinePoints.RemoveAll(storedLinePoints.Count - 1); // remove the last point from the line
        line.SetVertexCount(storedLinePoints.Count); // set the line’s vertex count to how many points we now have, which will be 1 fewer than it is currently
    }*/
    // Update is called once per frame

}
