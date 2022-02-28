using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using PolyPerfect.City;

public class Pathfinding : MonoBehaviour
{
    private BinaryHeap Open = new BinaryHeap(2048);
    private Dictionary<Guid, int> OpenIDs = new Dictionary<Guid, int>();
    private Dictionary<Guid, PathNode> Closed = new Dictionary<Guid, PathNode>();
    [HideInInspector]
    public List<Path> PathList = new List<Path>();
    [HideInInspector]
    public List<Path> wholePath;

    private Vector3 startPoint;
    private Vector3 endPoint;

    private Tile endTile;
    private Tile startTile;
    private Guid endId;

    private PathRequestManager requestManager;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
    }

    public void StartFindPath(Vector3 pathStart, Vector3 targetPos)
    {
        StartCoroutine(FindPath(pathStart, targetPos));
    }

    IEnumerator FindPath(Vector3 start, Vector3 end)
    {
        Open.Clear();
        PathList = new List<Path>();
        Closed.Clear();
        OpenIDs.Clear();

        startPoint = start;
        endPoint = end;

        startTile = FindClosestTile(startPoint);
        endTile = FindClosestTile(endPoint);
        List<Path> startPaths;
        startPaths = startTile.paths;
        foreach (Path path in startPaths)
        {
            float h = CalculateHeuristic(path.pathPositions[path.pathPositions.Count - 1].position);
            float g = Vector3.Distance(startPoint, path.pathPositions[0].position) + Vector3.Distance(transform.position, path.pathPositions[path.pathPositions.Count - 1].position);
            PathNode node = new PathNode() { path = path, lastNode = null, currentScore = g, score = h + g };
            Open.Insert(node);
            OpenIDs.Add(node.path.Id, 0);
        }
        //int i = 0;
        bool pathSuccess = false;
        while (Open.Count > 0)
        {

            PathNode node = GetBestNode();
            //Debug.Log(i++ + " " + (node.score) + " "+ node.path.transform.parent.parent.name);
            if (node.path.TileId == endTile.Id)
            {
                Closed.Add(node.path.Id, node);
                endId = node.path.Id;
                pathSuccess = true;
                break;
            }
            foreach (Path item in node.path.nextPaths)
            {
                if (item != null)
                {
                    if (!Closed.ContainsKey(item.Id) && !OpenIDs.ContainsKey(item.Id))
                    {
                        Vector3 distance = item.pathPositions[0].position - item.pathPositions[item.pathPositions.Count - 1].position;
                        float currentScore = node.currentScore + Math.Abs(distance.x) + Math.Abs(distance.y) + Math.Abs(distance.z) - ((item.speed / 10) * item.transform.lossyScale.x);

                        Open.Insert(new PathNode() { path = item, lastNode = node, currentScore = currentScore, score = CalculateHeuristic(item.pathPositions[item.pathPositions.Count - 1].position) + currentScore });
                        OpenIDs.Add(item.Id, 0);
                    }
                }
            }
            Closed.Add(node.path.Id, node);

        }

        if (pathSuccess)
        {
            Closed[endId].path = FindClosestPath(endPoint, Closed[endId].lastNode.path.nextPaths);
            GetPathList(Closed[endId]);
            PathList.Reverse();
            //PathList[0] = FindClosestPath(startPoint, startPaths);
            requestManager.FinishedProcessingPath(PathList, pathSuccess);
        }

        yield return null;
        
    }


    private PathNode GetBestNode()
    {

        PathNode pathNode = Open.PopTop();
        OpenIDs.Remove(pathNode.path.Id);
        return pathNode;
    }

    private float CalculateHeuristic(Vector3 currentTile)
    {
        Vector3 distance = endPoint - currentTile;
        return Math.Abs(distance.x) + Math.Abs(distance.y) + Math.Abs(distance.z);
    }
    private void GetPathList(PathNode thisNode)
    {
        if (thisNode != null)
        {
            PathList.Add(thisNode.path);
            GetPathList(thisNode.lastNode);
        }
    }

    private Tile FindClosestTile(Vector3 point)
    {
        Tile closestTile = null;
        Collider[] coliders = Physics.OverlapBox(point, new Vector3(Mathf.Abs(2 * transform.lossyScale.x), Mathf.Abs(2 * transform.lossyScale.y), Mathf.Abs(2 * transform.lossyScale.z)));
        foreach (Collider collider in coliders)
        {
            closestTile = collider.transform.GetComponent<Tile>();
            if (closestTile != null)
            {
                return closestTile;
            }
        }
        float minDistance = Mathf.Infinity;

        foreach (Tile tile in Tile.tiles)
        {
            float distance = Vector3.Distance(tile.transform.position, point);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }
    private Path FindClosestPath(Vector3 point, List<Path> paths)
    {
        Path closestPath = null;
        float minDistance = Mathf.Infinity;
        foreach (Path path in paths)
        {
            for (int i = 0; i < path.pathPositions.Count; i++)
            {
                float distance = Vector3.Distance(path.pathPositions[i].position, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPath = path;
                }
            }
        }
        return closestPath;
    }
}
#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(Pathfinding)), CanEditMultipleObjects]
public class CustomPathEditor : Editor
{
    Pathfinding navPath;
    private void OnEnable()
    {
        navPath = target as Pathfinding;
    }
    void OnSceneGUI()
    {
        if (navPath.wholePath.Count == 0)
        {
            if (navPath.PathList.Count != 0)
            {
                for (int i = 0; i < navPath.PathList.Count; i++)
                {
                    if (navPath.PathList[i] != null)
                    {
                        if (i < navPath.PathList.Count - 1)
                        {
                            for (int j = 1; j < navPath.PathList[i].pathPositions.Count; j++)
                            {
                                Handles.color = Color.white;
                                Handles.DrawLine(navPath.PathList[i].pathPositions[j - 1].position, navPath.PathList[i].pathPositions[j].position);
                                Handles.color = Color.blue;
                                Handles.ArrowHandleCap(0, navPath.PathList[i].pathPositions[j - 1].position, Quaternion.LookRotation(navPath.PathList[i].pathPositions[j].position - navPath.PathList[i].pathPositions[j - 1].position), 3f, EventType.Repaint);
                                if (i == 0)
                                    Handles.color = Color.blue;
                                else if (i == navPath.PathList.Count - 1)
                                    Handles.color = Color.red;
                                else
                                    Handles.color = Color.white;
                                Handles.SphereHandleCap(0, navPath.PathList[i].pathPositions[j].position, Quaternion.LookRotation(navPath.PathList[i].pathPositions[j].position), 0.2f, EventType.Repaint);
                            }

                        }
                    }

                }
            }
        }
        else
        {
            for (int i = 0; i < navPath.wholePath.Count; i++)
            {
                if (navPath.wholePath[i] != null)
                {
                    if (i < navPath.wholePath.Count - 1)
                    {
                        for (int j = 1; j < navPath.wholePath[i].pathPositions.Count; j++)
                        {
                            Handles.color = Color.white;
                            Handles.DrawLine(navPath.wholePath[i].pathPositions[j - 1].position, navPath.wholePath[i].pathPositions[j].position);
                            Handles.color = Color.blue;
                            Handles.ArrowHandleCap(0, navPath.wholePath[i].pathPositions[j - 1].position, Quaternion.LookRotation(navPath.wholePath[i].pathPositions[j].position - navPath.wholePath[i].pathPositions[j - 1].position), 3f, EventType.Repaint);
                            if (i == 0)
                                Handles.color = Color.blue;
                            else if (i == navPath.wholePath.Count - 1)
                                Handles.color = Color.red;
                            else
                                Handles.color = Color.white;
                            Handles.SphereHandleCap(0, navPath.wholePath[i].pathPositions[j].position, Quaternion.LookRotation(navPath.wholePath[i].pathPositions[j].position), 0.2f, EventType.Repaint);
                        }

                    }
                }

            }
        }

    }
}
#endif
    #endregion

