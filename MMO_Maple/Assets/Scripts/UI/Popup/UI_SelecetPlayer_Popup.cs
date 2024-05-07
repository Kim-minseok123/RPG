using DG.Tweening;
using EasyTransition;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SelecetPlayer_Popup : UI_Popup
{
    bool isClick = false;
    enum Buttons
    {
        CreatePlayerBtn,
        BackLoginBtn,
    }
    enum Images
    {
        ServerIconImage,
    }
    enum Texts
    {
        ServerNameText,
    }
    public override void Init()
    {
        base.Init();

        CanvasGroup cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        Tweener tw = cg.DOFade(1f, 0.5f);

        transform.position = new Vector3(0, 1, 90);
        transform.localScale = new Vector3(0.1069167f, 0.1069167f, 0.1069167f);
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = -1;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.BackLoginBtn).gameObject.BindEvent(OnClickBackLoginBtn);
        GetButton((int)Buttons.CreatePlayerBtn).gameObject.BindEvent(OnClickCreatePlayerBtn);

        lobbyPlayers.Clear();
        for (int i = 0; i < 3; i++)
        {
            Transform parent = transform.Find($"SelecetPlayerSpawnObj{i + 1}");
            GameObject obj = Managers.Resource.Instantiate("UI/SubItem/SelectPlayer_Item", parent);
            obj.GetComponent<PlayerSelectInit>().Setting(i);
            lobbyPlayers.Add(obj);
        }
        ServerInfo info = Managers.Network.ServInfo;
        GetText((int)Texts.ServerNameText).text = info.Name;
        GetImage((int)Images.ServerIconImage).sprite = Managers.Resource.Load<Sprite>($"UI/ServerIcon/{info.Name}");

        isClick = false;
    }
    List<GameObject> lobbyPlayers = new List<GameObject>();

    public void OnClickCreatePlayerBtn(PointerEventData data)
    {
        if (isClick) return;
        isClick = true;
        StartCoroutine(FadeAndChagneCreateUI());
    }
    public void OnClickBackLoginBtn(PointerEventData data)
    {
        if (isClick) return;
        isClick = true;

        Managers.Network._session.Disconnect();

        TransitionSettings ts = Managers.Resource.Load<TransitionSettings>("Trans/LinearWipe");
        TransitionManager.Instance().Transition(Define.Scene.Login, ts, 0);
    }
    IEnumerator FadeAndChagneCreateUI()
    {
        foreach (GameObject obj in lobbyPlayers)
            obj.SetActive(false);
        CanvasGroup cg = GetComponent<CanvasGroup>();
        Tweener tw = cg.DOFade(0f, 0.5f);
        yield return tw.WaitForCompletion();
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UI_CreatePlayer_Popup>();
    }
}