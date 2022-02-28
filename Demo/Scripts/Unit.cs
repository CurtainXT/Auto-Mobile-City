using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyPerfect.City;

public class Unit : MonoBehaviour
{
    public Transform target;
    public VehicleController controller;
    //public float speed = 1;
    List<Path> path;
    int pathIndex;
    int pathPositionIndex;
    bool bNeedPath = true;

    void Update()
    {
        if(bNeedPath)
        {
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
            bNeedPath = false;
        }
    }

    public void OnPathFound(List<Path> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0].pathPositions[0].position;

        while (true)
        {
            if (pathIndex >= path.Count)
            {
                yield break;
            }
            while(true)
            {
                if ((transform.position - currentWaypoint).sqrMagnitude < 1f)
                {
                    pathPositionIndex++;
                    if (pathPositionIndex >= path[pathIndex].pathPositions.Count)
                    {
                        pathPositionIndex = 0;
                        break;
                    }

                    currentWaypoint = path[pathIndex].pathPositions[pathPositionIndex].position;
                }
                controller.GetNextWaypoint(currentWaypoint);
                yield return null;
            }
            pathIndex++;

            //transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);

        }
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = pathIndex; i < path.Count; i++)
            {

                for (int j = 0; j < path[i].pathPositions.Count; j++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(path[i].pathPositions[j].position, Vector3.one * 0.1f);

                    //if (j == pathPositionIndex)
                    //{
                    //    //Gizmos.DrawLine(transform.position, path[i].pathPositions[j].position);
                    //}
                    //else
                    //{
                    //    Gizmos.DrawLine(path[i].pathPositions[j - 1].position, path[i].pathPositions[j].position);
                    //}
                }

            }
        }
    }
}
