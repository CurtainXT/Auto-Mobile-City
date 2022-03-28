using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyPerfect.City;
using System;

namespace ATMC
{
    // 可以将Unit视为车辆驾驶员
    public class Unit : MonoBehaviour, IPooledObject
    {
        public bool randomDestination = true;
        public bool waitForTrafficLight = true;
        public bool WillAvoidanceFrontCar = true;

        public float AsternTime = 2.5f;
        public float StopWaitToAsternTime = 2f;

        public Transform target;
        public ATMCBaseUnitController controller;
        List<Path> path = new List<Path>();
        int currentPathIndex;
        int currentPathPositionIndex;
        Vector3 currentTargetWaypoint;
        [HideInInspector]
        public bool bNeedPath = true;
        [HideInInspector]
        public Vector2 defaultMaxSpeedRange;

        [HideInInspector]
        public float maxPathSpeed = 5f;
        [SerializeField]
        private float currentMaxSpeed;
        // Demo
        public float LifeCount = 30;
        public bool isShow = true;
        private float currentStopByCarTimer = 0;
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
                if (value < controller.defaultMaxSpeed)
                    currentMaxSpeed = value;
                else
                    currentMaxSpeed = controller.defaultMaxSpeed;
            }
        }

        [SerializeField]
        StateType currentState = StateType.NoTarget;
        [SerializeField]
        private ATMCBaseUnitController otherCar;
        [SerializeField]
        private bool isOnCollision;

        // Editor
        private Vector3 randomGizmosColor;
        //--

        #region Spawn&Despawn
        private void Start()
        {
            controller = GetComponent<ATMCBaseUnitController>();
        }

        public void OnObjectSpawn()
        {
            // 给个初始目标
            if (controller == null)
                controller = GetComponent<ATMCBaseUnitController>();

            LifeCount = UnityEngine.Random.Range(30, 60); // Demo
            controller.defaultMaxSpeed = UnityEngine.Random.Range(defaultMaxSpeedRange.x, defaultMaxSpeedRange.y);
            currentState = StateType.NoTarget;
            FindNextTarget();
            path.Clear();
            randomGizmosColor = new Vector3(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
        }

        public void DeActive()
        {
            switch (currentState)
            {
                case StateType.Moving:
                    {
                        StopCoroutine("FollowPath");
                    }
                    break;
                case StateType.StopByCar:
                    {
                        StopCoroutine("StartMovingAfterWait");
                    }
                    break;
                case StateType.Avoidance:
                    {
                        StopCoroutine("DoAvoidance");
                    }
                    break;

                case StateType.Astern:
                    {
                        StopCoroutine("DoAstern");
                    }
                    break;
                default:
                    break;
            }
            otherCar = null;
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

            if (isShow)
            {
                if (LifeCount < 10f)
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
                while (true)
                {
                    Tile t = Tile.tiles[UnityEngine.Random.Range(0, Tile.tiles.Count - 1)];
                    if (t != null)
                    {
                        target = t.transform;
                        if (Vector3.Distance(transform.position, target.position) > 90f)
                        {
                            bNeedPath = true;
                            break;
                        }
                    }
                }
            }
        }


        private void FixCircling()
        {
            // 如果目标点在正左或正右反向 车辆可能会一直转圈
            float targetAngle = Vector3.Angle(transform.forward, currentTargetWaypoint - transform.position);
            if (targetAngle > 60 && targetAngle < 110)
            {
                Vector3 nextTargetWaypoint = new Vector3();
                int nextPathPositionIndex = currentPathPositionIndex;
                int nextPathIndex = currentPathIndex;

                // 获取接下来的若干个路径点
                int iterNum = 2;
                for (int i = 0; i < iterNum; i++)
                {
                    nextPathPositionIndex++;
                    if (nextPathPositionIndex >= path[nextPathIndex].pathPositions.Count)
                    {
                        nextPathPositionIndex = 0;
                        nextPathIndex++;
                        if (nextPathIndex >= path.Count)
                        {
                            //StopCoroutine(FollowPath());
                            //currentState = StateType.NoTarget;
                            //FindNextTarget();
                            break;
                        }
                    }

                    nextTargetWaypoint = path[nextPathIndex].pathPositions[nextPathPositionIndex].position;
                    Vector3 selfToNextTarget = (nextTargetWaypoint - transform.position);
                    if (IsInSameDirection(selfToNextTarget))
                    {

                        currentTargetWaypoint = nextTargetWaypoint;
                        currentPathIndex = nextPathIndex;
                        currentPathPositionIndex = nextPathPositionIndex;
                        return;
                    }
                }
            }
        }

        #endregion

        #region State Handler
        private void HandleStateBehavior()
        {
            switch (currentState)
            {
                case StateType.Moving:
                    {
                        CurrentMaxSpeed = maxPathSpeed;
                        SteeringToTarget();
                        HandleThrottleAndBreak();

                        FixCircling();
                    }
                    break;
                case StateType.MovingBehindCar:
                    {
                        if (otherCar != null && otherCar.gameObject.activeSelf)
                        {
                            CurrentMaxSpeed = Mathf.Sqrt(otherCar.currentSpeedSqr);
                            if (/*(transform.position - otherCar.transform.position).sqrMagnitude > 0.01f * controller.currentSpeedSqr ||*/
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
                            currentStopByCarTimer += Time.deltaTime;
                            if (currentStopByCarTimer > StopWaitToAsternTime)
                            {
                                currentState = StateType.StartAstern;
                                currentStopByCarTimer = 0;
                            }
                        }
                        else
                        {
                            currentState = StateType.Moving;
                            currentStopByCarTimer = 0;
                        }
                    }
                    break;
                case StateType.StartAvoidance:
                    {
                        if (this.gameObject.activeSelf)
                        {
                            CurrentMaxSpeed = maxPathSpeed;
                            float avoidTimer = Mathf.Clamp(16f / controller.currentSpeedSqr, 0.2f, 0.6f);
                            StopCoroutine(DoAvoidance(avoidTimer));
                            StartCoroutine(DoAvoidance(avoidTimer));
                        }
                    }
                    break;
                case StateType.Avoidance:
                    {
                        SteeringToAvoidance();
                        HandleThrottleAndBreak();
                    }
                    break;
                case StateType.StartAstern:
                    {
                        if(this.gameObject.activeSelf)
                        {
                            StopCoroutine(DoAstern(AsternTime));
                            StartCoroutine(DoAstern(AsternTime));
                        }
                    }
                    break;
                case StateType.Astern:
                    {
                        controller.horizontalInput = 0;
                        controller.verticalInput = -1f;
                        controller.isBreaking = false;
                    }
                    break;
                case StateType.NoTarget:
                    {
                        currentMaxSpeed = 0;
                    }
                    break;
                default:
                    break;
            }
        }

        private IEnumerator DoAstern(float Time)
        {
            currentState = StateType.Astern;

            yield return new WaitForSeconds(Time);
            currentState = StateType.Moving;
        }

        private IEnumerator DoAvoidance(float Time)
        {
            currentState = StateType.Avoidance;

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
            if (otherCar != null && otherCar.gameObject.activeSelf)
            {
                float leftOrRight = AngleDir(transform.forward, transform.position - otherCar.transform.position, transform.up);
                if (leftOrRight > 0)
                    controller.horizontalInput = 1f;
                else
                    controller.horizontalInput = -1f;
            }
        }

        private void HandleThrottleAndBreak()
        {
            if (controller.currentSpeedSqr < CurrentMaxSpeed * CurrentMaxSpeed)
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

            if (dir > 0.5f)
            {
                return 1f;
            }
            else if (dir < -0.5f)
            {
                return -1f;
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

        private bool IsInFrontSight(Transform other)
        {

            float carDirection = Vector3.Angle(transform.right, (other.transform.position - transform.position).normalized);
            float frontOrRear = AngleDir(transform.right, (transform.position - other.transform.position), Vector3.up);

            return (carDirection < 145 && carDirection > 55 && frontOrRear > 0);
        }
        
        private bool IsInRearSight(Transform other)
        {

            float carDirection = Vector3.Angle(transform.right, (other.transform.position - transform.position).normalized);
            float frontOrRear = AngleDir(transform.right, (transform.position - other.transform.position), Vector3.up);

            return (carDirection < 145 && carDirection > 55 && frontOrRear < 0);
        }

        private bool IsInSameDirection(Vector3 otherForward)
        {
            float direction = Vector3.Angle(transform.forward, otherForward);

            return direction < 45;
        }

        #endregion

        #region Environment Interaction
        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.CompareTag("Car") && (currentState == StateType.Moving || currentState == StateType.MovingBehindCar))
            {
                Debug.Log("We have car collision!");

                if (IsInFrontSight(collision.transform)/* && !IsInSameDirection(collision.transform.forward)*/)
                {
                    currentState = StateType.StartAstern;
                }
                else if (!IsInFrontSight(collision.transform) && !IsInRearSight(collision.transform))
                {
                    otherCar = collision.collider.GetComponentInParent<ATMCBaseUnitController>();
                    if(otherCar.currentSpeedSqr < controller.currentSpeedSqr)
                        currentState = StateType.StartAvoidance;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("TrafficLight") && waitForTrafficLight && currentState == StateType.Moving)
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
            if (other.CompareTag("Car") && currentState == StateType.Moving)
            {
                otherCar = other.GetComponentInParent<ATMCBaseUnitController>();

                if (IsInFrontSight(other.transform) && IsInSameDirection(other.transform.forward) && !other.isTrigger)
                {
                    currentState = StateType.MovingBehindCar;
                }
                else if (IsInFrontSight(other.transform) && !IsInSameDirection(other.transform.forward) && !other.isTrigger)
                {
                    currentState = StateType.StopByCar;
                }
                else if (IsInFrontSight(other.transform) && WillAvoidanceFrontCar/* && other.isTrigger*/)
                {
                    if (otherCar.currentSpeedSqr > controller.currentSpeedSqr)
                    {
                        currentState = StateType.StopByCar;
                    }
                    else
                    {
                        float otherLeftOrRight = AngleDir(transform.forward, other.transform.position - transform.position, transform.up);
                        float targetLeftOrRight = AngleDir(transform.forward, currentTargetWaypoint - transform.position, transform.up);
                        if(otherLeftOrRight * targetLeftOrRight >= 0)
                        {
                            currentState = StateType.StartAvoidance;
                        }
                        else
                        {
                            currentState = StateType.Moving;
                        }
                    }
                }

            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Car"))
            {
                if (otherCar != null && otherCar.name == other.name)
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
                        else if (i == currentPathIndex && j == currentPathPositionIndex)
                        {
                            Gizmos.DrawCube(path[i].pathPositions[j].position, Vector3.one * 0.4f);
                            Gizmos.DrawLine(transform.position, path[i].pathPositions[j].position);
                        }
                    }

                }
            }
        }


        private void OnGUI()
        {
            if(isShow)
            {

                Vector2 position = Camera.main.WorldToScreenPoint(transform.position);
                Vector2 stateSize = GUI.skin.label.CalcSize(new GUIContent(currentState.ToString()));
                switch (currentState)
                {
                    case StateType.Moving:
                        GUI.color = Color.green;
                        break;
                    case StateType.MovingBehindCar:
                        GUI.color = Color.yellow;
                        break;
                    case StateType.StopByTrafficLight:
                        GUI.color = Color.red;
                        break;
                    case StateType.StopByCar:
                        GUI.color = Color.black;
                        break;
                    case StateType.Avoidance:
                        GUI.color = Color.blue;
                        break;
                    case StateType.Astern:
                        GUI.color = Color.grey;
                        break;
                    default:
                        break;
                }
                GUI.Label(new Rect(position.x, Screen.height - position.y + stateSize.y, stateSize.x, stateSize.y), currentState.ToString());

                position = Camera.main.WorldToScreenPoint(transform.position);
                Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(gameObject.name));
                GUI.color = Color.cyan;
                //GUI.Label(new Rect(position.x - (nameSize.x / 2), position.y - nameSize.y, nameSize.x, nameSize.y), gameObject.name);
                GUI.Label(new Rect(position.x, Screen.height - position.y, nameSize.x, nameSize.y), gameObject.name);
            }
        }
        #endregion
    }
}