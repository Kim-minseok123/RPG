using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public LayerMask layerMask;
    public void Update()
    {
        Collider[] colliders;
        GameObject enemy = null;
        colliders = Physics.OverlapSphere(transform.position + new Vector3(0f, 0.8f, 0f), 2f, layerMask, QueryTriggerInteraction.Collide);
        if (colliders.Length > 0)
        {
            float short_distance = 1000f;
            foreach (Collider col in colliders)
            {
                Debug.Log(col.gameObject.name);
                float short_distance2 = Vector3.Distance(transform.position, col.transform.position);
                if (short_distance > short_distance2)
                {
                    short_distance = short_distance2;
                    enemy = col.gameObject;
                }
            }
        }
        Debug.Log(enemy);
    }

}
