using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform targetTrans;
    private Vector3 offset;
    private void Awake()
    {
        offset = transform.position - targetTrans.position;
        //offset = new Vector3(0, offset.y, offset.z);
    }

    void LateUpdate()
    {
        //transform.position = Vector3.Lerp(transform.position, targetTrans.position + offset, Time.deltaTime * 10);
        transform.position = targetTrans.position + offset;
    }
}

