using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpeditionNpc : NPCController
{
    public override void OpenNpc()
    {
        C_Expedition expedition = new C_Expedition();
        expedition.RoomId = 2;
        Managers.Network.Send(expedition);
    }
}
