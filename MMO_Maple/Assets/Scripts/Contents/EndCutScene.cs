using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCutScene : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject room2;
    public void End()
    {
        mainCamera.SetActive(true);
        room2.SetActive(false);

        C_EndCutScene endCutScene = new C_EndCutScene();
        endCutScene.CutSceneEnd = true;
        Managers.Network.Send(endCutScene);
        gameObject.SetActive(false);
    }
    public void EndRaid()
    {
        C_EndBossItemCutScene c_EndBossItemCutScene
            = new C_EndBossItemCutScene();
        Managers.Network.Send(c_EndBossItemCutScene);
    }
}
