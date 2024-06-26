using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionShopNPC : NPCController
{
    public Animator animator;
    int prevNum = -1;
    public void SetAnim()
    {
        int animNum = -1;
        do
        {
            animNum = Random.Range(0, 4);
        } while (prevNum == animNum);
        prevNum = animNum;
        animator.SetInteger("CurAnim", animNum);
    }

    public override void OpenNpc()
    {

    }
}
