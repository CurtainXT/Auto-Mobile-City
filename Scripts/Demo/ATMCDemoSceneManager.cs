using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PolyPerfect.City;

namespace ATMC
{
    public class ATMCDemoSceneManager : MonoBehaviour
    {
        public string nextSceneName;
        public bool DebugShowUnitState;
        public List<Tile> specificTargets = new List<Tile>();

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                DebugShowUnitState = !DebugShowUnitState;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}