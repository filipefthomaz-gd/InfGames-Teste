using System.Collections;
using System.Collections.Generic;
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

    private bool addedPoint;
    public bool lineStatus = false;


    void Start()
    {
      line = GetComponent<LineRenderer>();
      audioSource = GetComponent<AudioSource>();
    }

    public void AddLinePoint(Vector2 newPoint, int arrayPosition, bool moving)
    {
        if (!storedLinePoints.Contains(newPoint))
        {
          if(!moving || (moving && (newPoint.x == storedLinePoints[storedLinePoints.Count-1].x || newPoint.y == storedLinePoints[storedLinePoints.Count-1].y)))
          {
            arrayPos.Add(arrayPosition);
            storedLinePoints.Add(newPoint); // add the new point to our saved list of line points
            line.positionCount = storedLinePoints.Count; // set the line’s vertex count to how many points we now have, which will be 1 more than it is currently
            line.SetPosition(storedLinePoints.Count - 1, newPoint);
            addedPoint = true;
          }
        }
        else
            addedPoint = false;
    }

    //Check line status
    public bool CheckLineStatus(Vector2 newPoint, Vector2 startPoint, Vector2 endPoint, GameObject[] nodes)
    {
      if(!lineStatus)
      {
        if (addedPoint)
        {
            if (storedLinePoints.Contains(startPoint) && storedLinePoints.Contains(endPoint))
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


    public void RemoveAllPoints()
    {
        storedLinePoints = new List<Vector2>();
        arrayPos = new List<int>();
        line.positionCount = 0;
    }


    //Reset line; If line is completed or midway and one presses one of the extremity nodes;
    public void ResetLine(Vector2[] centerPos, List<int> specialPos, int hitPos, GameObject[] nodes)
    {
      foreach(int position in arrayPos)
      {
        if(position != specialPos[lineId*2] && position != specialPos[lineId*2 + 1])
          nodes[position].GetComponent<SpriteRenderer>().color = Color.white;
      }
      lineStatus = false;
      RemoveAllPoints();
      AddLinePoint(centerPos[hitPos], hitPos, false);
    }

   /* void RemoveLastLinePoint()
    {
        storedLinePoints.RemoveAll(storedLinePoints.Count - 1); // remove the last point from the line
        line.SetVertexCount(storedLinePoints.Count); // set the line’s vertex count to how many points we now have, which will be 1 fewer than it is currently
    }*/
    // Update is called once per frame

}
