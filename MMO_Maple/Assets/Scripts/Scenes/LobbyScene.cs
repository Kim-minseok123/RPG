using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        Managers.UI.ShowPopupUI<UI_SelectPlayer_Popup>();
    }

    public override void Clear()
    {
        
    }
}
