using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class bootstrap : MonoBehaviour
{
    public string startLevel;
    public int startTime;

    void Update()
    {
        if (Time.timeSinceLevelLoad > startTime)
        {
            SceneManager.LoadScene(startLevel);
        }
    }
}