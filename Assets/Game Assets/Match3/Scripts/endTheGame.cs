using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class endTheGame : MonoBehaviour
{
    public GameObject btnYes;
    public GameObject btnNo;

    void Update()
    {
        if (btnYes.activeSelf == true)
        {
            SceneManager.LoadScene("Match3");
        }
        if (btnNo.activeSelf == true)
        {
            Application.Quit();
        }
    }
}