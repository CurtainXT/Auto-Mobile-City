using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ATMC;

public class UnitDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Unit target = null;
        if (target = other.GetComponent<Unit>())
        {
            target.DeActive();
        }
    }
}
