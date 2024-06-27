using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionShopNPC : NPCController
{
    public Animator animator;
    int prevNum = -1;
    [SerializeField]
    int templateId = 1;
    private void Start()
    {
        SetAnim();
    }
    public void SetAnim()
    {
        int animNum = -1;
        do
        {
            animNum = Random.Range(0, 4);
        } while (prevNum == animNum);
        prevNum = animNum;
        Debug.Log(animNum);
        animator.SetInteger("CurAnim", animNum);
    }

    public override void OpenNpc()
    {
        base.OpenNpc();
    }
    public override void CloseNpc()
    {
        base.CloseNpc();
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    public override void CameraSetting()
    {
        Vector3 cameraPos = transform.position + new Vector3(1, 0.9f, -2.2f);
        Vector3 rotate = new Vector3(0, 0, 0);

        Camera.main.GetComponent<CameraController>().PlayerToNpcMove(cameraPos, rotate, gameObject);
    }
    public override void OpenNpcUI()
    {

    }
    public override void CloseNpcUI()
    {

    }
}
