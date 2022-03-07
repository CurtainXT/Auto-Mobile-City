using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PolyPerfect.City;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;

    bool isProcessingPath;

    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, List<Path> startPaths, Action<List<Path>, bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, startPaths, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if(!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, currentPathRequest.startPaths);
        }
    }

    public void FinishedProcessingPath(List<Path> path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public List<Path> startPaths;
        public Action<List<Path>, bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, List<Path> _startPath, Action<List<Path>, bool> _callback)
        {
            pathStart = _start;
            pathEnd = _end;
            startPaths = _startPath;
            callback = _callback;
        }
    }
}
