using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCutScene : MonoBehaviour
{
    public void End()
    {
        gameObject.SetActive(false);
        C_EndCutScene endCutScene = new C_EndCutScene();
        endCutScene.CutSceneEnd = true;
        Managers.Network.Send(endCutScene);
    }
}
