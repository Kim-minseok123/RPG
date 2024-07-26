using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class test : MonoBehaviour
{
    public Vector3 testvector;
    public  NavMeshAgent _agent;
    int cnt = 0;

    public void Start()
    {
        Vector3 pos = transform.position + testvector;
        _agent.SetDestination(pos);
        
    }
    public void Update()
    {
        cnt++;
        float dis = Vector3.Distance(_agent.destination, transform.position);
        if (dis < 0.3f || (cnt >= 5 && Vector3.zero == _agent.velocity))
        {
            Debug.Log("end");
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = new Vector3(0,1,0);
            Vector3 pos = transform.position + testvector;
            _agent.SetDestination(pos);
        }
    }
}
