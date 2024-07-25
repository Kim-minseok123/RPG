using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_Login_Popup : UI_Popup
{
    enum GameObjects
    {
        LoginInput,
        PasswordInput,
        BaseObj,
    }
    enum Buttons
    {
        LoginBtn,
        RegisterBtn,
    }
    public bool _click { get; set; } = false;
    public override void Init()
    {
        base.Init();
        GetComponent<CanvasGroup>().alpha = 0f;
        GetComponent<CanvasGroup>().DOFade(1f, 1f);

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.LoginBtn).gameObject.BindEvent(OnClickLoginBtn);
        GetButton((int)Buttons.RegisterBtn).gameObject.BindEvent(OnClickRegisterBtn);
    }

    public void OnClickLoginBtn(PointerEventData data)
    {
        if (_click == true) return;
        _click = true;
        
        Managers.Sound.Play("ButtonClick");

        string account = GetObject((int)GameObjects.LoginInput).GetComponent<TMP_InputField>().text;
        string password = GetObject((int)GameObjects.PasswordInput).GetComponent<TMP_InputField>().text;

        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Passwrod = password
        };
        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, res =>
        {
            Debug.Log(res.LoginOk);
            GetObject((int)GameObjects.LoginInput).GetComponent<TMP_InputField>().text = "";
            GetObject((int)GameObjects.PasswordInput).GetComponent<TMP_InputField>().text = "";

            if (res.LoginOk)
            {
                Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;


                GetObject((int)GameObjects.BaseObj).transform.DOLocalMoveY(-1500f, 1f).SetEase(Ease.OutQuad);
                UI_ServerSelect_Popup popup = Managers.UI.ShowPopupUI<UI_ServerSelect_Popup>();
                popup.ServerSetting(res.ServerList);
            }
            else
            {
                // 에러 팝업 띄우기
                Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("아이디 또는 비밀번호를\n확인해주세요.");
            }
            _click = false;
        });
    }
    public void OnClickRegisterBtn(PointerEventData data)
    {
        Managers.Sound.Play("ButtonClick");
        StartCoroutine(FadeDestroy());
    }
    IEnumerator FadeDestroy()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        Tween tw = cg.DOFade(0f, 0.5f);
        yield return tw.WaitForCompletion();
        _click = false;
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UI_Register_Popup>();
    }
}
