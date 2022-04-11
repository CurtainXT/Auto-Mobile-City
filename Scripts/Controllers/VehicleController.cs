using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ATMC;

public class VehicleController : ATMCBaseUnitController
{
    public float motorForce;
    public float breakForce;
    public float defaultMaxSteerAngle;

    private float currentMotorForce;
    private float currentSteerAngle;
    private float currentBreakForce;
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
    private Transform CenterOfMass;
    

    private void Start()
    {
        unitRigidbody.centerOfMass = CenterOfMass.localPosition;
    }


    private void Update()
    {
        UpdateWheelsVisual();
        currentSpeedSqr = unitRigidbody.velocity.sqrMagnitude;
    }

    protected override void HandleMotor()
    {
        currentMotorForce = verticalInput > 0? verticalInput * motorForce : verticalInput * motorForce * 0.8f;
    }

    protected override void ApplyBreaking()
    {
        currentBreakForce = isBreaking ? breakForce : 0;
    }

    protected override void HandleSteering()
    {
        currentSteerAngle = defaultMaxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    protected override void MoveUnit()
    {
        rearLeftWheelCollider.motorTorque = currentMotorForce;
        rearRightWheelCollider.motorTorque = currentMotorForce;

        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        rearLeftWheelCollider.brakeTorque = currentBreakForce;
        rearRightWheelCollider.brakeTorque = currentBreakForce;
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
