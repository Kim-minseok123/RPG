using EasyTransition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_ServerList_Item : UI_Base
{
    bool _init = false;
    public ServerInfo Info;
    bool _isClick = false;
    enum Images
    {
        BackgroundImage,
        ServerIconImage,
    }
    enum Texts
    {
        ServerNameText,
        BusyScoreText,
    }
    public override void Init()
    {
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        _init = true;

        GetImage((int)Images.BackgroundImage).gameObject.BindEvent(OnClickServer);
    }
    public void UpdateUI()
    {
        if (_init == false)
            return;
        GetImage((int)Images.ServerIconImage).sprite = Managers.Resource.Load<Sprite>($"UI/ServerIcon/{Info.Name}");
        GetText((int)Texts.ServerNameText).text = Info.Name;

        string _serverState;
        if (Info.BusyScore >= 7) _serverState = "<color=red>혼잡</color>";
        else if (Info.BusyScore >= 4) _serverState = "<color=yellow>보통</color>";
        else _serverState = "<color=green>원활</color>";

        GetText((int)Texts.BusyScoreText).text = _serverState;
    }
    public void OnClickServer(PointerEventData data)
    {
        if (_isClick == true) return;
        _isClick = true;

        Managers.Sound.Play("ButtonClick");

        // 서버 접속
        Managers.Network.ConnectToGame(Info);
    }
    public void Setting(ServerInfo info)
    {
        Info = info;
        UpdateUI();
    }
}
