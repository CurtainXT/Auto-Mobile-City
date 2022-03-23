using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ATMC;

public class TankController : ATMC_UnitController
{
    //坦克左边的所有轮子
    public GameObject[] LeftWheels;
    //坦克右边的所有轮子
    public GameObject[] RightWheels;

    //坦克左边的履带
    public GameObject LeftTrack;
    //坦克右边的履带
    public GameObject RightTrack;

    public float wheelSpeed = 2f;
    public float trackSpeed = 2f;
    public float rotateSpeed = 10f;
    public float moveSpeed = 2f;

    public AudioSource movementAudioPlayer;
    public AudioClip move;
    public AudioClip idle;

    private void Update()
    {
        PlaySound();
        PerformTrackMovementVFX();
        currentSpeedSqr = defaultMaxSpeed;
    }

    protected override void HandleMotor()
    {
        // 限制倒车的速度
        verticalInput = Mathf.Clamp(verticalInput, -0.3f, 1f);
        if (horizontalInput != 0)
            verticalInput = 0;
    }



    protected override void ApplyBreaking()
    {
        if (isBreaking)
            verticalInput = 0;
    }

    protected override void HandleSteering()
    {
        // 坦克本体的旋转
        Quaternion turnRotation = Quaternion.Euler(0f, horizontalInput * rotateSpeed * Time.deltaTime, 0f);
        unitRigidbody.MoveRotation(unitRigidbody.rotation * turnRotation);
    }

    protected override void MoveUnit()
    {
        // 坦克本体的移动
        unitRigidbody.MovePosition(unitRigidbody.position + transform.forward * moveSpeed * verticalInput * Time.deltaTime);
    }

    void PlaySound()
    {
        // 音效播放
        if (horizontalInput == 0 && verticalInput == 0)
        {
            movementAudioPlayer.clip = idle;
            if (!movementAudioPlayer.isPlaying)
            {
                movementAudioPlayer.volume = 0.2f;
                movementAudioPlayer.Play();
            }
        }
        else
        {
            movementAudioPlayer.clip = move;
            if (!movementAudioPlayer.isPlaying)
            {
                movementAudioPlayer.volume = 0.6f;
                movementAudioPlayer.Play();
            }
        }
    }

    void PerformTrackMovementVFX()
    {
        // 这些都是为了让履带和轮子看上去在动
        //坦克左右两边车轮转动
        foreach (var wheel in LeftWheels)
        {
            wheel.transform.Rotate(new Vector3(wheelSpeed * verticalInput, 0f, 0f));
            wheel.transform.Rotate(new Vector3(wheelSpeed * 0.6f * horizontalInput, 0f, 0f));
        }
        foreach (var wheel in RightWheels)
        {
            wheel.transform.Rotate(new Vector3(wheelSpeed * verticalInput, 0f, 0f));
            wheel.transform.Rotate(new Vector3(wheelSpeed * 0.6f * -horizontalInput, 0f, 0f));
        }
        //履带滚动效果
        // 前后
        LeftTrack.transform.GetComponent<MeshRenderer>().material.mainTextureOffset += new Vector2(0, -trackSpeed * verticalInput * Time.deltaTime);
        RightTrack.transform.GetComponent<MeshRenderer>().material.mainTextureOffset += new Vector2(0, -trackSpeed * verticalInput * Time.deltaTime);
        // 左右
        LeftTrack.transform.GetComponent<MeshRenderer>().material.mainTextureOffset += new Vector2(0, 0.6f * -trackSpeed * horizontalInput * Time.deltaTime);
        RightTrack.transform.GetComponent<MeshRenderer>().material.mainTextureOffset += new Vector2(0, 0.6f * trackSpeed * horizontalInput * Time.deltaTime);
    }
}
