using EasyTransition;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_PlayerInfoCanvas_Item : UI_Base
{
    GameObject NonPlayer;
    GameObject OnPlayer;
    public Sprite Beginner;
    public Sprite Warrior;
    public Sprite Archer;
    LobbyPlayerInfo playerInfo;
    enum Images
    {
        PlayerClassImage,
        PlayerInfoBackground,
    }
    enum Texts
    {
        PlayerNickNameText,
        PlayerClassText,
        PlayerLevelText,
    }
    public override void Init()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        transform.localPosition = Vector3.zero;

        BindText(typeof(Texts));
        NonPlayer = transform.Find("NonPlayerUIObj").gameObject;
        OnPlayer = transform.Find("PlayerUIObj").gameObject;
        BindImage(typeof(Images));

    }
    public void Setting(LobbyPlayerInfo info)
    {
        transform.localPosition = Vector3.zero;
        playerInfo = info;
        if (info == null)
        {
            OnPlayer.SetActive(false);
        }
        else
        {
            NonPlayer.SetActive(false);

            switch (info.ClassType)
            {
                case (int)ClassTypes.Beginner:
                    GetText((int)Texts.PlayerClassText).text = "초보자";
                    GetImage((int)Images.PlayerClassImage).sprite = Beginner;
                    break;
                case (int)ClassTypes.Warrior:
                    GetText((int)Texts.PlayerClassText).text = "전사";
                    GetImage((int)Images.PlayerClassImage).sprite = Warrior;
                    break;
                case (int)ClassTypes.Archer:
                    GetText((int)Texts.PlayerClassText).text = "궁수";
                    GetImage((int)Images.PlayerClassImage).sprite = Archer;
                    break;
            }
            GetText((int)Texts.PlayerLevelText).text = $"Lv. {info.Level}";
            GetText((int)Texts.PlayerNickNameText).text = info.Name;
            GetImage((int)Images.PlayerInfoBackground).gameObject.BindEvent(EnterGame);
        }
    }
    public void EnterGame(PointerEventData data)
    {
        TransitionSettings ts = Managers.Resource.Load<TransitionSettings>("Trans/LinearWipe");
        TransitionManager.Instance().Transition(Define.Scene.Game, ts, 0, () => {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = playerInfo.Name;
            Managers.Network.Send(enterGamePacket);
            Managers.UI.CloseAllPopupUI();
        });
       
    }
}
