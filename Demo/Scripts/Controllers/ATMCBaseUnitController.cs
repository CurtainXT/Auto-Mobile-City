using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATMC
{
    public abstract class ATMCBaseUnitController : MonoBehaviour
    {
        public float defaultMaxSpeed;

        public bool isBreaking;
        public float horizontalInput;
        public float verticalInput;
        public float currentSpeedSqr = 0;
        
        protected Rigidbody unitRigidbody;

        private void Awake()
        {
            unitRigidbody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            HandleMotor();

            HandleSteering();

            ApplyBreaking();

            MoveUnit();
        }

        protected abstract void HandleMotor();

        protected abstract void ApplyBreaking();

        protected abstract void HandleSteering();

        protected abstract void MoveUnit();
    }
}
