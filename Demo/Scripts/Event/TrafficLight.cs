using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isGreen;

    public delegate void LightChange(bool isGreen);
    public LightChange lightChange;
}
