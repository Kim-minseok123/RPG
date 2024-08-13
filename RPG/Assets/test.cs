using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class test : MonoBehaviour
{
    public TextMeshProUGUI chatText; // TextMeshPro »ç¿ë½Ã

    void Start()
    {
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(chatText.textInfo.lineCount);
        }
        
    }
    
}
