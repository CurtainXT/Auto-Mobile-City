using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATMC
{
    public class VisibleOrNot : MonoBehaviour
    {
        Unit selfUnit;

        private void Start()
        {
            selfUnit = GetComponentInParent<Unit>();
        }

        public void OnBecameVisible()
        {
            selfUnit.isShow = true;
        }


        public void OnBecameInvisible()
        {
            selfUnit.isShow = false;
        }
    }
}