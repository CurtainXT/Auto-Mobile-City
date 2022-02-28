using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField]
    private float motorForce;
    [SerializeField]
    private float breakForce;
    [SerializeField]
    private float maxSteerAngle;
    [SerializeField]
    private float m_maxSpeed;

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private bool isBreaking;
    private float currentBreakForce;
    private Vector3 nextWaypoint;
    private bool bcanDirve = false;
    [SerializeField]
    private WheelCollider frontLeftWheelCollider;
    [SerializeField]
    private WheelCollider frontRightWheelCollider;
    [SerializeField]
    private WheelCollider rearLeftWheelCollider;
    [SerializeField]
    private WheelCollider rearRightWheelCollider;

    [SerializeField]
    private Transform frontLeftWheelTransform;
    [SerializeField]
    private Transform frontRightWheelTransform;
    [SerializeField]
    private Transform rearLeftWheelTransform;
    [SerializeField]
    private Transform rearRightWheelTransform;

    [SerializeField]
    private Rigidbody VehicleRig;
    [SerializeField]
    private Transform CenterOfMass;
    

    private void Start()
    {
        VehicleRig.centerOfMass = CenterOfMass.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(bcanDirve)
        {
            CalculateInput();

            HandleMotor();

            HandleSteering();

            UpdateWheelsVisual();
        }

    }

    public void GetNextWaypoint(Vector3 waypoint)
    {
        nextWaypoint = waypoint;
        bcanDirve = true;
    }

    private void CalculateInput()
    {
        float leftOrRight = AngleDir(transform.forward, nextWaypoint - transform.position, transform.up);

        if (leftOrRight > 0)
            horizontalInput = 0.8f;
        else
            horizontalInput = -0.8f;
        if (leftOrRight == 0)
            horizontalInput = 0;

        if (VehicleRig.velocity.sqrMagnitude > m_maxSpeed*m_maxSpeed || (transform.position - nextWaypoint).sqrMagnitude < 20f)
        {
            verticalInput = 0;
        }
        else
        {
            verticalInput = 0.6f;
        }

        //isBreaking = true;
    }

    private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 1f)
        {
            return 1.0f;
        }
        else if (dir < -1f)
        {
            return -1.0f;
        }
        else
        {
            return 0.0f;
        }
    }

    private void HandleMotor()
    {
        rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
        rearRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentBreakForce = isBreaking ? breakForce : 0;
        if (isBreaking)
        {
            ApplyBreaking();
        }
    }

    private void ApplyBreaking()
    {
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        rearLeftWheelCollider.brakeTorque = currentBreakForce;
        rearRightWheelCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheelsVisual()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider _wheelCollider, Transform _wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        _wheelCollider.GetWorldPose(out pos, out rot);
        _wheelTransform.rotation = rot;
        _wheelTransform.position = pos;
    }

}
