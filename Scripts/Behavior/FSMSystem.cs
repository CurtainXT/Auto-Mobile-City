using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATMC
{
    public class FSMSystem : MonoBehaviour
    {
        private FSMState currentState;
        private List<FSMState> states = new List<FSMState>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            currentState.Reason();
            currentState.Act();
        }
    }
}