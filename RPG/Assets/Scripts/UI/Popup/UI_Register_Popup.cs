using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_Register_Popup : UI_Popup
{
    enum GameObjects
    {
        LoginInput,
        PasswordInput,
        PasswordCheckInput,
    }
    enum Buttons
    {
        RegisterBtn,
        DeclineBtn,
    }
    public bool _click { get; set; } = false;
    public override void Init()
    {
        base.Init();
        GetComponent<CanvasGroup>().alpha = 0f;
        GetComponent<CanvasGroup>().DOFade(1f, 1f);

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.DeclineBtn).gameObject.BindEvent(OnClickDeclineBtn);
        GetButton((int)Buttons.RegisterBtn).gameObject.BindEvent(OnClickRegisterBtn);
    }

    public void OnClickDeclineBtn(PointerEventData data)
    {
        if (_click) return;

        Managers.Sound.Play("ButtonClick");

        _click = true;
        DestroyRegisterUI();
    }
    public void OnClickRegisterBtn(PointerEventData data)
    {
        if (_click) return;

        Managers.Sound.Play("ButtonClick");

        _click = true;
        string account = GetObject((int)GameObjects.LoginInput).GetComponent<TMP_InputField>().text;
        string password = GetObject((int)GameObjects.PasswordInput).GetComponent<TMP_InputField>().text;
        string passwordCheck = GetObject((int)GameObjects.PasswordCheckInput).GetComponent<TMP_InputField>().text;
        
        if(password.Equals(passwordCheck) == false)
        {
            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("비밀번호가 다릅니다.");
            _click = false;
        }
        else if (account == "" || password == "")
        {
            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("아이디 또는 비밀번호를\n입력하세요.");
            _click = false;
        }
        else if (password.Length < 8)
        {
            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("비밀번호는 8자리 이상으로 입력하세요.");
            _click = false;
        }
        else
        {
            CreateAccountPacketReq packet = new CreateAccountPacketReq()
            {
                AccountName = account,
                Passwrod = password
            };
            Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, res =>
            {
                Debug.Log(res.CreateOk);
                if(res.CreateOk)
                    DestroyRegisterUI();
                else
                {
                    Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("중복된 아이디입니다.");
                    _click = false;
                }
            });
        }
    }
    public void DestroyRegisterUI()
    {
        StartCoroutine(FadeDestroy());
    }
    IEnumerator FadeDestroy()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        Tween tw = cg.DOFade(0f, 0.5f);
        yield return tw.WaitForCompletion();
        _click = false;
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UI_Login_Popup>();
    }
}
