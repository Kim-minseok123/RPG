using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSelectInit : MonoBehaviour
{
    int _index = -1;
    public Transform Playerparent;

    public void Setting(int index)
    {
        _index = index;

        List<LobbyPlayer> lobbyPlayerInfos = Managers.Network.LobbyPlayerInfos;

        // 플레이어가 있음
        if(index < lobbyPlayerInfos.Count)
        {
            GameObject go =  Managers.Resource.Instantiate("Creature/Player/PlayerLobby", Playerparent);
            UI_PlayerInfoCanvas_Item item = Managers.UI.MakeSubItem<UI_PlayerInfoCanvas_Item>(transform);
            item.Setting(lobbyPlayerInfos[index].Player);
            go.GetComponent<PlayerLobby>().Setting(lobbyPlayerInfos[index].Item.ToList());
        }
        else
        {
            UI_PlayerInfoCanvas_Item item = Managers.UI.MakeSubItem<UI_PlayerInfoCanvas_Item>(transform);
            item.Setting(null);
        }
    }
}
