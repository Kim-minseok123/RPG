using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    MyPlayerController player;
    public float Yaxis;
    public float Xaxis;
    public void Awake()
    {

    }

    private float rotSensitive = 3f;
    public float dis = 15f;
    public float RotationMin = 20f;
    public float RotationMax = 50f;
    private float smoothTime = 0.12f;


    private Vector3 targetRotation;
    private Vector3 currentVel;

    void LateUpdate()
    {
        if (player == null) return;
        if (Input.GetMouseButton(2))
        {
            Yaxis += Input.GetAxis("Mouse X") * rotSensitive;
            Xaxis -= Input.GetAxis("Mouse Y") * rotSensitive;

            Xaxis = Mathf.Clamp(Xaxis, RotationMin, RotationMax);

            targetRotation = Vector3.SmoothDamp(targetRotation, new Vector3(Xaxis, Yaxis), ref currentVel, smoothTime);
            this.transform.eulerAngles = targetRotation;
        }
        transform.position = player.transform.position - transform.forward * dis;
    }
    public void SettingPlayer(MyPlayerController myPlayer)
    {
        player = myPlayer;
    }
}
