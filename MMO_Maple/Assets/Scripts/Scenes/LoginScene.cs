﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoginScene : BaseScene
{

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        Managers.Web.BaseUrl = "https://localhost:5001/api";

        Screen.SetResolution(1920, 1080, false);
#if UNITY_SERVER
        Managers.Network.Master = true;
        LoginMaster();
#endif
        Managers.UI.ShowSceneUI<UI_TitleScene>();
    }

    public override void Clear()
    {
        
    }
#if UNITY_SERVER
    public void LoginMaster()
    {

        string account = "Master";
        string password = "13582357812";

        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Passwrod = password
        };
        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, res =>
        {

            if (res.LoginOk)
            {
                Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;

                Managers.Network.ConnectToGame(res.ServerList[0]);
            }
            else
            {
                RegisterMaster(account, password);
            }
        });
    }
    public void RegisterMaster(string account, string password)
    {
        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Passwrod = password
        };
        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, res =>
        {
            Debug.Log(res.CreateOk);
            if (res.CreateOk)
            {
                LoginMaster();
            }
            else
            {
                Debug.Log("마스터 회원 가입 실패");
            }
        });
    }
#endif
}