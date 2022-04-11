using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ATMC
{
    public class ATMCDemoSceneManager : MonoBehaviour
    {
        public bool DebugShowUnitState;

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
            }
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                DebugShowUnitState = !DebugShowUnitState;
            }
        }
    }
}