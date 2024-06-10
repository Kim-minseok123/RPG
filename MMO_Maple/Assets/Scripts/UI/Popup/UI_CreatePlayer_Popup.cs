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

public class UI_CreatePlayer_Popup : UI_Popup
{
    public bool isClick = false;
    enum Buttons
    {
        CreatePlayerMaleBtn,
        CreatePlayerFeMaleBtn,
        BackLoginBtn,
    }
    enum GameObjects
    {
        NickNameInput,
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
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        GetButton((int)Buttons.CreatePlayerMaleBtn).gameObject.BindEvent(OnClickCreatePlayerMaleBtn);
        GetButton((int)Buttons.CreatePlayerFeMaleBtn).gameObject.BindEvent(OnClickCreatePlayerFeMaleBtn);
        GetButton((int)Buttons.BackLoginBtn).gameObject.BindEvent(OnClickBackLoginBtn);
        
        
        ServerInfo info = Managers.Network.ServInfo;
        GetText((int)Texts.ServerNameText).text = info.Name;
        GetImage((int)Images.ServerIconImage).sprite = Managers.Resource.Load<Sprite>($"UI/ServerIcon/{info.Name}");
        isClick = false;
    }


    public void OnClickCreatePlayerMaleBtn(PointerEventData data)
    {
        if (isClick) return;
        isClick = true;
        CreatePlayer(isMale: true);
    }
    public void OnClickCreatePlayerFeMaleBtn(PointerEventData data)
    {
        if (isClick) return;
        isClick = true;
        Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("현재는 남성 캐릭터만 생성 가능합니다.", () => { isClick = false; });
        //CreatePlayer(isMale: false);
    }
    public void OnClickBackLoginBtn(PointerEventData data)
    {
        if (isClick) return;

        // 캐릭터 선택 화면으로
        StartCoroutine(FadeAndChagneSelectUI());
    }
    void CreatePlayer(bool isMale)
    {
        string name = GetObject((int)GameObjects.NickNameInput).GetComponent<TMP_InputField>().text;

        if(name == "" || name.Length < 2)
        {
            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("닉네임이 두글자 보다 작습니다.", () => { isClick = false; });
            return;
        }
        else
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = name;
            createPacket.IsMale = isMale;
            Managers.Network.Send(createPacket);
        }
    }
    public GameObject Male;
    public GameObject Female;
    IEnumerator FadeAndChagneSelectUI()
    {
        Male.SetActive(false);
        Female.SetActive(false);

        CanvasGroup cg = GetComponent<CanvasGroup>();
        Tweener tw = cg.DOFade(0f, 0.5f);
        yield return tw.WaitForCompletion();
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UI_SelecetPlayer_Popup>();
    }
    public void EndCreate()
    {
        StartCoroutine(FadeAndChagneSelectUI());
    }
}