using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public Vector3 OrthOffset;
    public Vector3 perspOffset;
    public bool isFollowMode = true;
    public Transform targetTrans;
    public float moveSpeed = 1f;
    public float rotateSpeed = 1f;
    public float sizeSpeed = 1f;

    public Vector2 WorldMoveRangeVertical = new Vector2(-134f, 146f);
    public Vector2 WorldMoveRangeHorizontal = new Vector2(-85f, 68f);
    public Vector2 CameraSizeRange = new Vector2(10f, 20f);
    public Vector2 CameraFOVRange = new Vector2(10f, 20f);

    private Camera camera;

    private void Awake()
    {
        if(camera == null)  
            camera = Camera.main;
    }

    private void Start()
    {
        if (camera.orthographic)
            camera.transform.localPosition = OrthOffset;
        else
            camera.transform.localPosition = perspOffset;
        camera.transform.LookAt(this.transform);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(cameraRay, out hit))
            {
                if(hit.collider.CompareTag("Car") || hit.collider.CompareTag("TrafficLight"))
                {
                    targetTrans = hit.collider.transform;
                    isFollowMode = true;
                }
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            isFollowMode = false;
            targetTrans = null;
        }
    }

    void LateUpdate()
    {
        if (isFollowMode)
        {
            if(targetTrans != null && targetTrans.gameObject.activeSelf)
            {
                transform.position = targetTrans.position;
            }
            else
            {
                isFollowMode = false;
            }
        }
        else
        {
            float verticalInput = Input.GetAxis("Vertical");
            float horizontalInput = Input.GetAxis("Horizontal");

            Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);
            if (movement.sqrMagnitude > 1)
            {
                movement.Normalize();
            }

            Vector3 newPosition = this.transform.position + transform.TransformDirection(movement * moveSpeed * Time.deltaTime);
            if (!IsOutOfMap(newPosition))
            {
                this.transform.position = newPosition;
            }
        }

        float cameraZoomInput = Input.GetAxis("CameraZoom");
        float CameraRotateInput = Input.GetAxis("CameraRotate");

        if(camera.orthographic)
        {
            camera.orthographicSize += cameraZoomInput * sizeSpeed * Time.deltaTime;
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, CameraSizeRange.x, CameraSizeRange.y);
        }
        else
        {
            camera.fieldOfView += cameraZoomInput * sizeSpeed * Time.deltaTime;
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, CameraFOVRange.x, CameraFOVRange.y);
        }


        transform.Rotate(transform.up, CameraRotateInput * rotateSpeed * Time.deltaTime);
    }

    private bool IsOutOfMap(Vector3 _position)
    {
        if (_position.z > WorldMoveRangeVertical.y || _position.z < WorldMoveRangeVertical.x ||
            _position.x > WorldMoveRangeHorizontal.y || _position.x < WorldMoveRangeHorizontal.x)
            return true;
        else
            return false;
    }
}

