using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    GameObject go;
   
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Spawn();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            DeSpawn();
        }
    }
    public void Spawn()
    {
        go = Managers.Resource.Instantiate("Effect/AngerEffect", transform);
        go.transform.position = transform.position;
    }
    public void DeSpawn()
    {
        Managers.Resource.Destroy(go);
    }
}
