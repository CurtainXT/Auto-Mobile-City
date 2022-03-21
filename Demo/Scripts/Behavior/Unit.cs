using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyPerfect.City;
using System;

// 可以将Unit视为车辆驾驶员
public class Unit : MonoBehaviour, IPooledObject
{
    public bool randomDestination = true;
    public Transform target;
    public VehicleController controller;
    List<Path> path = new List<Path>();
    int currentPathIndex;
    int currentPathPositionIndex;
    Vector3 currentTargetWaypoint;
    [HideInInspector]
    public bool bNeedPath = true;

    public Vector2 defaultMaxSpeedRange;
    [HideInInspector]
    public float maxPathSpeed = 5f;
    [SerializeField]
    private float currentMaxSpeed;
    // Demo
    public float LifeCount = 30;
    public bool isShow = true;
    // --
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

    [SerializeField]
    StateType currentState = StateType.Moving;
    [SerializeField]
    private VehicleController otherCar;
    [SerializeField]
    private bool isOnCollision;
    #region Original Solution
    //[SerializeField]
    //private bool isMoving = true;
    //[SerializeField]
    //private bool drivingBehindCar = false;
    //[SerializeField]
    //private bool drivingTrafficLights = false;
    //[SerializeField]
    //private bool stopByCar = false;
    //[SerializeField]
    //private bool emergencyAvoidance = false;
    //[SerializeField]
    //private float emergencyAvoidanceTimer = 0;
    #endregion

    // Editor
    private Vector3 randomGizmosColor;
    //--

    #region Spawn&Despawn
    public void OnObjectSpawn()
    {
        // 给个初始目标
        LifeCount = UnityEngine.Random.Range(30, 60);
        controller.defaultMaxSpeed = UnityEngine.Random.Range(defaultMaxSpeedRange.x, defaultMaxSpeedRange.y);
        FindNextTarget();
        path.Clear();
        randomGizmosColor = new Vector3(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
    }

    public void DeActive()
    {
        this.gameObject.SetActive(false);
    }

    private void OnBecameVisible()
    {
        isShow = true;
    }

    private void OnBecameInvisible()
    {
        isShow = false;
    }
    #endregion

    void Update()
    {
        // Demo
        LifeCount -= Time.deltaTime;
        if (LifeCount < 0)
            DeActive();
        // --

        if (bNeedPath)
        {
            PathRequestManager.PathRequest newRequest = new PathRequestManager.PathRequest(transform.position, target.position, this.gameObject, 
                path.Count == 0 ? null : path[path.Count - 1].nextPaths, OnPathFound);
            PathRequestManager.RequestPath(newRequest);
            bNeedPath = false;
        }

        HandleStateBehavior();

        if(isShow)
        {
            if(LifeCount < 10f)
                LifeCount = 10f;
        }
    }

    #region Path Requesting&Following
    public void OnPathFound(List<Path> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
            currentState = StateType.Moving;
        }
        else
        {
            Debug.LogWarning(this.gameObject.name + " can not find a path!");
            // Demo
            DeActive();
            // --
            //FindNextTarget();
        }
    }

    IEnumerator FollowPath()
    {
        //currentState = StateType.Moving;
        currentPathIndex = 0;
        currentPathPositionIndex = FindClosestPathPositionIndexInUnitFront(path[currentPathIndex]);
        currentTargetWaypoint = path[currentPathIndex].pathPositions[currentPathPositionIndex].position;

        while (true)
        {
            if (currentPathIndex >= path.Count)
            {
                currentState = StateType.NoTarget;
                FindNextTarget();
                yield break;
            }
            maxPathSpeed = path[currentPathIndex].speed;
            //CurrentMaxSpeed = maxPathSpeed;
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

            #region Original Solution
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
            #endregion
        }
    }
    #endregion

    #region Controller Handler
    private void HandleStateBehavior()
    {
        switch (currentState)
        {
            case StateType.Moving:
                {
                    CurrentMaxSpeed = maxPathSpeed;
                    SteeringToTarget();
                    HandleThrottleAndBreak();
                }
                break;
            case StateType.MovingBehindCar:
                {
                    if (otherCar != null && otherCar.gameObject.activeSelf)
                    {
                        CurrentMaxSpeed = Mathf.Sqrt(otherCar.currentSpeedSqr);
                        if((transform.position - otherCar.transform.position).sqrMagnitude > 0.1f * controller.currentSpeedSqr ||
                            CurrentMaxSpeed <= 0.01f)
                        {
                            CurrentMaxSpeed = 0;
                        }
                        SteeringToTarget();
                        HandleThrottleAndBreak();
                    }
                    else
                    {
                        //Debug.LogWarning("Why this is no car in front when you are in MovingBehindCar State?");
                        currentState = StateType.Moving;
                    }
                }
                break;
            case StateType.StopByTrafficLight:
                {
                    CurrentMaxSpeed = 0;
                    SteeringToTarget();
                    HandleThrottleAndBreak();
                }
                break;
            case StateType.StopByCar:
                {
                    if (otherCar != null && otherCar.gameObject.activeSelf)
                    {
                        CurrentMaxSpeed = 0;
                        SteeringToTarget();
                        HandleThrottleAndBreak();
                    }
                    else
                    {
                        currentState = StateType.Moving;
                    }
                }
                break;
            case StateType.Avoidance:
                {
                    if (otherCar != null && otherCar.gameObject.activeSelf)
                    {
                        CurrentMaxSpeed = maxPathSpeed;
                        SteeringToAvoidance();
                        HandleThrottleAndBreak();
                    }
                    else
                    {
                        currentState = StateType.Moving;
                    }
                }
                break;
            case StateType.Astern:
                {
                    if (isShow)
                    {
                        StopCoroutine(DoAstern(1f));
                        StartCoroutine(DoAstern(1f));
                    }
                }
                break;
            default:
                break;
        }

        #region Original Solution
        //// steering
        //float leftOrRight = AngleDir(transform.forward, currentTargetWaypoint - transform.position, transform.up);
        //if (leftOrRight > 0)
        //    controller.horizontalInput = 1f;
        //else
        //    controller.horizontalInput = -1f;
        //if (leftOrRight == 0)
        //    controller.horizontalInput = 0;
        //if(emergencyAvoidance/* && emergencyAvoidanceTimer > 0*/)
        //{
        //    controller.horizontalInput = 1;
        //    //    emergencyAvoidanceTimer -= Time.deltaTime;
        //}
        ////else
        ////{
        ////    emergencyAvoidanceTimer = EmergencyAvoidanceTimeSpan;
        ////    emergencyAvoidance = false;
        ////}

        //// throttle
        ////if ( /*|| !isMoving/*|| (transform.position - nextWaypoint).sqrMagnitude < 20f*/)
        ////{
        ////    isMoving = false;
        ////}
        ////else
        ////{
        ////    controller.verticalInput = 1f;
        ////}
        //if (drivingTrafficLights || controller.currentSpeedSqr > CurrentMaxSpeed * CurrentMaxSpeed || stopByCar || drivingBehindCar && carInFront != null && controller.currentSpeedSqr > carInFront.currentSpeedSqr)
        //{
        //    isMoving = false;
        //}
        //else
        //{
        //    isMoving = true;
        //    controller.verticalInput = 1f;
        //}

        //// break
        //if (!isMoving)
        //{
        //    controller.isBreaking = true;
        //}
        //else
        //{
        //    controller.isBreaking = false;
        //}
        #endregion
    }

    private IEnumerator DoAstern(float Time)
    {
        controller.horizontalInput = 0;
        controller.verticalInput = -1f;
        controller.isBreaking = false;

        yield return new WaitForSeconds(Time);
        currentState = StateType.Moving;
    }

    private void SteeringToTarget()
    {
        float leftOrRight = AngleDir(transform.forward, currentTargetWaypoint - transform.position, transform.up);
        if (leftOrRight > 0)
            controller.horizontalInput = 1f;
        else
            controller.horizontalInput = -1f;
        if (leftOrRight == 0)
            controller.horizontalInput = 0;
    }

    private void SteeringToAvoidance()
    {
        if(otherCar != null)
        {
            float leftOrRight = AngleDir(transform.forward, otherCar.transform.position - transform.position, transform.up);
            if (leftOrRight >= 0)
                controller.horizontalInput = -1f;
            else
                controller.horizontalInput = 1f;
        }
    }

    private void HandleThrottleAndBreak()
    {
        if(controller.currentSpeedSqr < CurrentMaxSpeed * CurrentMaxSpeed)
        {
            controller.verticalInput = 1f;
            controller.isBreaking = false;
        }
        else
        {
            controller.verticalInput = 0;
            controller.isBreaking = true;
        }
    }
    #endregion

    #region Utils
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

    private int FindClosestPathPositionIndexInUnitFront(Path path)
    {
        int currentClosestIndex = 0;
        Transform ClosestPathPosition = null;
        for (int i = 0; i < path.pathPositions.Count; i++)
        {
            Transform CurrentIndexPosition = path.pathPositions[i];
            if (Vector3.Dot(transform.forward, (CurrentIndexPosition.position - transform.position).normalized) > 0)
            {
                if (ClosestPathPosition == null)
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
    #endregion

    #region Environment Interaction
    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.CompareTag("Car"))
        {
            Debug.Log("We have car collision!");
            //float carDirection = Vector3.Angle(transform.right, (collision.collider.transform.position - transform.position).normalized);
            currentState = StateType.Astern;
        }
    }
    //private void OnCollisionExit(Collision collision)
    //{
    //    currentState = StateType.Moving;
    //}

    private void OnTriggerEnter(Collider other)
    {
        #region Original Solution
        //if (other.CompareTag("Car") && !other.isTrigger)
        //{
        //    float direction = Vector3.Angle(transform.forward, other.transform.forward);
        //    float carDirection = Vector3.Angle(transform.right, (other.transform.position - transform.position).normalized);
        //    if (direction < 65)
        //    {
        //        currentState = StateType.MovingBehindCar;
        //        otherCar = other.GetComponentInParent<VehicleController>();
        //    }
        //    if (direction > 50 && direction < 100 && carDirection < 100 && carDirection > 45)
        //    {
        //        currentState = StateType.StopByCar;
        //    }

        //}
        //else if (other.CompareTag("Car") && other.isTrigger && Vector3.Angle(transform.forward, other.transform.forward) > 100)
        //{
        //    if(other.GetComponent<VehicleController>().currentSpeedSqr > controller.currentSpeedSqr)
        //    {
        //        currentState = StateType.StopByCar;
        //    }
        //    else
        //    {
        //        currentState = StateType.Avoidance;
        //        otherCar = other.GetComponentInParent<VehicleController>();
        //    }
        //}
        #endregion

        if (other.CompareTag("TrafficLight") && !(currentState == StateType.StopByTrafficLight))
        {
            TrafficLight trafic = other.GetComponent<TrafficLight>();
            if (Vector3.Angle(-trafic.transform.forward, transform.forward) < 25)
            {
                if (!trafic.isGreen)
                {
                    currentState = StateType.StopByTrafficLight;
                    trafic.lightChange += StartMoving;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Car") && currentState != StateType.Astern)
        {
            float direction = Vector3.Angle(transform.forward, other.transform.forward);
            float carDirection = Vector3.Angle(transform.right, (other.transform.position - transform.position).normalized);

            if (direction < 45 && !other.isTrigger)
            {
                currentState = StateType.MovingBehindCar;
                otherCar = other.GetComponentInParent<VehicleController>();
            }
            if (direction > 45 && direction < 135 && carDirection < 135 && carDirection > 45 && !other.isTrigger)
            {
                currentState = StateType.StopByCar;
                otherCar = other.GetComponentInParent<VehicleController>();
            }
            if (direction > 135 && direction < 45)
            {
                currentState = StateType.Avoidance;
                otherCar = other.GetComponentInParent<VehicleController>();
                //if (other.GetComponent<VehicleController>().currentSpeedSqr > controller.currentSpeedSqr && !other.isTrigger)
                //{
                //    currentState = StateType.StopByCar;
                //}
                //else
                //{
                //    currentState = StateType.Avoidance;
                //}
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        #region Original Solution
        //if (other.CompareTag("Car") && !other.isTrigger)
        //{
        //    switch (currentState)
        //    {
        //        case StateType.Moving:
        //            break;
        //        case StateType.MovingBehindCar:
        //            currentState = StateType.Moving;
        //            otherCar = null;
        //            break;
        //        case StateType.StopByCar:
        //            StopCoroutine(StartMovingAfterWait(0.3f));
        //            StartCoroutine(StartMovingAfterWait(0.3f));
        //            break;
        //        default:
        //            break;
        //    }
        //}
        //else if(other.CompareTag("Car") && other.isTrigger)
        //{
        //    switch (currentState)
        //    {
        //        case StateType.StopByCar:
        //            StopCoroutine(StartMovingAfterWait(0.3f));
        //            StartCoroutine(StartMovingAfterWait(0.3f));
        //            break;
        //        case StateType.Avoidance:
        //            StopCoroutine(StartMovingAfterWait(0.3f));
        //            StartCoroutine(StartMovingAfterWait(0.3f));
        //            break;
        //        default:
        //            break;
        //    }
        //}
        #endregion

        if (other.CompareTag("Car"))
        {
            if(otherCar != null && otherCar.name == other.name)
            {
                switch (currentState)
                {
                    case StateType.Moving:
                        break;
                    case StateType.MovingBehindCar:
                        currentState = StateType.Moving;
                        break;
                    case StateType.StopByCar:
                        StopCoroutine(StartMovingAfterWait(0.3f));
                        StartCoroutine(StartMovingAfterWait(0.3f));
                        break;
                    case StateType.Avoidance:
                        StopCoroutine(StartMovingAfterWait(0.3f));
                        StartCoroutine(StartMovingAfterWait(0.3f));
                        break;
                    default:
                        break;
                }
                otherCar = null;
            }
        }
        else if (other.CompareTag("TrafficLight"))
        {
            TrafficLight trafic = other.GetComponent<TrafficLight>();
            trafic.lightChange -= StartMoving;
            // Demo
            currentState = StateType.Moving;
            //--
        }
    }

    void StartMoving(bool isGreen)
    {
        if (isGreen)
        {
            currentState = StateType.Moving;
        }
    }

    IEnumerator StartMovingAfterWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        currentState = StateType.Moving;
        otherCar = null;
    }
    #endregion

    #region Editor Debugging
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = currentPathIndex; i < path.Count; i++)
            {

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
                }

            }
        }
    }
    #endregion
}
