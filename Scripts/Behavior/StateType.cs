using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATMC{
    public enum StateType
    {
        Moving,
        MovingBehindCar,
        StopByTrafficLight,
        StopByCar,
        StartAvoidance,
        Avoidance,
        StartAstern,
        Astern,
        NoTarget
    }
}