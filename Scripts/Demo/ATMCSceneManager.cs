using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ATMC
{
    public class ATMCSceneManager : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            if(Input.GetKeyUp(KeyCode.LeftShift))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}