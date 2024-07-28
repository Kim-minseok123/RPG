using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBarCanvas : MonoBehaviour
{
    Transform lootAtTransform;
    // Start is called before the first frame update
    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        lootAtTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + lootAtTransform.rotation * Vector3.back,
            lootAtTransform.transform.rotation * Vector3.up);
    }
}
