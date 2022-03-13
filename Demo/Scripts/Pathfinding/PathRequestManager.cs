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

    public static void RequestPath(PathRequest _request)
    {
        PathRequest newRequest = _request;
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if(!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            if(!currentPathRequest.requester.activeSelf)
            {
                TryProcessNext();
                return;
            }
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, currentPathRequest.startPaths);
        }
    }

    public void FinishedProcessingPath(List<Path> path, bool success)
    {
        // 由于对象池设计 寻路请求的对象有可能正处于未激活
        if(currentPathRequest.requester.activeSelf)
        {
            currentPathRequest.callback(path, success);
        }

        isProcessingPath = false;
        TryProcessNext();
    }

    public struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public GameObject requester;
        public List<Path> startPaths;
        public Action<List<Path>, bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, GameObject _requester, List<Path> _startPath, Action<List<Path>, bool> _callback)
        {
            pathStart = _start;
            pathEnd = _end;
            requester = _requester;
            startPaths = _startPath;
            callback = _callback;
        }
    }
}
