using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyPerfect.City;
using System;

// 可以将Unit视为车辆驾驶员
public class Unit : MonoBehaviour, IPooledObject
{
    public Transform target;
    public VehicleController controller;
    List<Path> path = new List<Path>();
    int currentPathIndex;
    int currentPathPositionIndex;
    Vector3 currentTargetWaypoint;

    [HideInInspector]
    public bool bNeedPath = true;
    [HideInInspector]
    public float maxPathSpeed = 5f;
    // Demo
    public float LifeCount = 30;
    [HideInInspector]
    public float CurrentMaxSpeed
    {
        get
        {
            return currentMaxSpeed;
        }
        set
        {
            if(value < controller.defaultMaxSpeed)
                currentMaxSpeed = value;
            else
                currentMaxSpeed = controller.defaultMaxSpeed;
        }
    }

    public bool randomDestination = true;
    [SerializeField]
    private bool isMoving = true;
    [SerializeField]
    private bool drivingBihindCar = false;
    [SerializeField]
    private bool drivingTrafficLights = false;
    [SerializeField]
    private bool stopByCar = false;
    private float currentMaxSpeed;

    // Editor
    private Vector3 randomGizmosColor;

    private VehicleController carInFront;

    public void OnObjectSpawn()
    {
        // 给个初始目标
        LifeCount = UnityEngine.Random.Range(30, 60);
        controller.defaultMaxSpeed = UnityEngine.Random.Range(6f, 10f);
        FindNextTarget();
        path.Clear();
        randomGizmosColor = new Vector3(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
    }

    public void DeActive()
    {
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        // Demo
        LifeCount -= Time.deltaTime;
        if (LifeCount < 0)
            DeActive();
        //

        if (bNeedPath)
        {
            PathRequestManager.PathRequest newRequest = new PathRequestManager.PathRequest(transform.position, target.position, this.gameObject, 
                path.Count == 0 ? null : path[path.Count - 1].nextPaths, OnPathFound);
            PathRequestManager.RequestPath(newRequest);
            bNeedPath = false;
        }

        CalculateInput();
    }

    public void OnPathFound(List<Path> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
        else
        {
            Debug.LogWarning(this.gameObject.name + " can not find a path!");
            // Demo
            DeActive();
            //
            //FindNextTarget();
        }
    }

    IEnumerator FollowPath()
    {
        isMoving = true;
        currentPathIndex = 0;
        currentPathPositionIndex = FindClosestPathPositionIndexInUnitFront(path[currentPathIndex]);
        currentTargetWaypoint = path[currentPathIndex].pathPositions[currentPathPositionIndex].position;

        while (true)
        {
            if (currentPathIndex >= path.Count)
            {
                isMoving = false;
                FindNextTarget();
                yield break;
            }
            maxPathSpeed = path[currentPathIndex].speed;
            CurrentMaxSpeed = maxPathSpeed;
            while (true)
            {
                if ((transform.position - currentTargetWaypoint).sqrMagnitude < 0.2f * controller.currentSpeedSqr)
                {
                    currentPathPositionIndex++;
                    if (currentPathPositionIndex >= path[currentPathIndex].pathPositions.Count)
                    {
                        currentPathPositionIndex = 0;
                        break;
                    }

                    currentTargetWaypoint = path[currentPathIndex].pathPositions[currentPathPositionIndex].position;
                }
                yield return null;
            }
            currentPathIndex++;
        }
    }

    private int FindClosestPathPositionIndexInUnitFront(Path path)
    {
        int currentClosestIndex = 0;
        Transform ClosestPathPosition = null;
        for (int i = 0; i < path.pathPositions.Count; i++)
        {
            Transform CurrentIndexPosition = path.pathPositions[i];
            if (Vector3.Dot(transform.forward, (CurrentIndexPosition.position - transform.position).normalized) > 0)
            {
                if(ClosestPathPosition == null)
                {
                    currentClosestIndex = i;
                    ClosestPathPosition = CurrentIndexPosition;
                }

                if ((CurrentIndexPosition.position - transform.position).sqrMagnitude < (ClosestPathPosition.position - transform.position).sqrMagnitude)
                {
                    currentClosestIndex = i;
                    ClosestPathPosition = CurrentIndexPosition;
                }
            }
        }

        return currentClosestIndex;
    }

    public void FindNextTarget()
    {
        if (randomDestination)
        {
            while(true)
            {
                Tile t = Tile.tiles[UnityEngine.Random.Range(0, Tile.tiles.Count - 1)];
                if (t != null)
                {
                    target = t.transform;
                }

                if(Vector3.Distance(transform.position, target.position) > 90f)
                {
                    bNeedPath = true;
                    break;
                }

            }

            //if (t.tileType == Tile.TileType.Road || t.tileType == Tile.TileType.RoadAndRail)
            //{
            //    if (t.verticalType == Tile.VerticalType.Bridge)
            //    {
            //        destination = t.transform.position + (Vector3.up * 12);
            //    }
            //    else
            //    {
            //        destination = t.transform.position;
            //    }
            //}
            //}
            //else
            //{
            //    //Destination of the first checkpoint
            //    destination = checkpoints[0];
            //    start = transform.position;
            //}
        }
    }

    private void CalculateInput()
    {
        // steering
        float leftOrRight = AngleDir(transform.forward, currentTargetWaypoint - transform.position, transform.up);
        if (leftOrRight > 0)
            controller.horizontalInput = 1f;
        else
            controller.horizontalInput = -1f;
        if (leftOrRight == 0)
            controller.horizontalInput = 0;

        // throttle
        //if ( /*|| !isMoving/*|| (transform.position - nextWaypoint).sqrMagnitude < 20f*/)
        //{
        //    isMoving = false;
        //}
        //else
        //{
        //    controller.verticalInput = 1f;
        //}
        if (drivingTrafficLights || controller.currentSpeedSqr > CurrentMaxSpeed * CurrentMaxSpeed || stopByCar || drivingBihindCar && controller.currentSpeedSqr > carInFront.currentSpeedSqr)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
            controller.verticalInput = 1f;
        }

        // break
        if (!isMoving)
        {
            controller.isBreaking = true;
        }
        else
        {
            controller.isBreaking = false;
        }

    }

    private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 1f)
        {
            return 0.6f;
        }
        else if (dir < -1f)
        {
            return -0.6f;
        }
        else
        {
            return 0.0f;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && !other.isTrigger)
        {
            float direction = Vector3.Angle(transform.forward, other.transform.forward);
            float carDirection = Vector3.Angle(transform.right, (other.transform.position - transform.position).normalized);
            if (direction < 50)
            {
                drivingBihindCar = true;
                carInFront = other.GetComponentInParent<VehicleController>();
            }
            if (direction > 40 && carDirection < 100 && carDirection > 45)
            {
                stopByCar = true;
            }

        }
        if (other.CompareTag("TrafficLight") && !drivingTrafficLights)
        {
            TrafficLight trafic = other.GetComponent<TrafficLight>();
            if (Vector3.Angle(-trafic.transform.forward, transform.forward) < 25)
            {
                if (!trafic.isGreen)
                {
                    drivingTrafficLights = true;
                    isMoving = false;
                    trafic.lightChange += StartMoving;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car") && !other.isTrigger)
        {
            StopCoroutine(StartMovingAfterWait(0.2f));
            StartCoroutine(StartMovingAfterWait(0.2f));
            drivingBihindCar = false;
        }
        else if (other.CompareTag("TrafficLight"))
        {
            TrafficLight trafic = other.GetComponent<TrafficLight>();
            trafic.lightChange -= StartMoving;
            drivingTrafficLights = false;
        }
    }

    void StartMoving(bool isGreen)
    {
        if (isGreen)
        {
            drivingTrafficLights = false;
        }
    }

    IEnumerator StartMovingAfterWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        stopByCar = false;
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = currentPathIndex; i < path.Count; i++)
            {
                //int j = 1;
                //if (i == currentPathIndex)
                //    j = (currentPathPositionIndex == 0) ? 1 : currentPathPositionIndex;
                for (int j = 0; j < path[i].pathPositions.Count; j++)
                {
                    Gizmos.color = new Color(randomGizmosColor.x, randomGizmosColor.y, randomGizmosColor.z, 1f);


                    if ((i > currentPathIndex && j != 0) || (i == currentPathIndex && j > currentPathPositionIndex))
                    {
                        Gizmos.DrawCube(path[i].pathPositions[j].position, Vector3.one * 0.4f);
                        Gizmos.DrawLine(path[i].pathPositions[j - 1].position, path[i].pathPositions[j].position);
                    }
                    else if(i == currentPathIndex && j == currentPathPositionIndex)
                    {
                        Gizmos.DrawCube(path[i].pathPositions[j].position, Vector3.one * 0.4f);
                        Gizmos.DrawLine(transform.position, path[i].pathPositions[j].position);
                    }

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
