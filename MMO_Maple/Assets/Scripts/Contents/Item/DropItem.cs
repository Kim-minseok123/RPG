using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : BaseController
{
    public string itemName { get; set; }
    // Æ®¸®°Å


    public void OnTriggerStay(Collider other)
    {
        if(other.gameObject == Managers.Object.MyPlayer.gameObject)
        { 
            if (Input.GetKeyDown(KeyCode.Z) && Managers.Object.MyPlayer.State == CreatureState.Idle)
            {
                Managers.Object.MyPlayer.State = CreatureState.Wait;
                C_GetDropItem getDropItemPacket = new C_GetDropItem();
                getDropItemPacket.DropItemId = Id;
                Managers.Network.Send(getDropItemPacket);
                Managers.Resource.Destroy(gameObject);
            }
        }
    }
}
