using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class test : MonoBehaviour
{
    public LayerMask groundLayerMask;
    public Camera cm = null;
    public  NavMeshAgent _agent;

    public void Start()
    {
    }
    public void Update()
    {
        if (Input.GetMouseButton(1))
            {
            Ray ray = cm.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit , 100f , groundLayerMask))
            {
                Debug.Log(hit.point);
                _agent.SetDestination(hit.point);
            }
        }
        
    }
}
