using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossItemBox : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if(other == Managers.Object.MyPlayer)
        {
            if (Managers.Object.MyPlayer.isMaster)
            {
                C_BossItemCutScene cutScene = new C_BossItemCutScene();
                Managers.Network.Send(cutScene);
            }
        }
    }
}
