using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class test : MonoBehaviour
{
    public float aspectWidth = 16f;
    public float aspectHeight = 9f;

    void Start()
    {
        UpdateResolution();
    }

    void Update()
    {
        if (Screen.width != (int)(Screen.height * (aspectWidth / aspectHeight)))
        {
            UpdateResolution();
        }
    }

    void UpdateResolution()
    {
        int width = Screen.width;
        int height = (int)(width / (aspectWidth / aspectHeight));
        Screen.SetResolution(width, height, false);
    }

}
