using DG.Tweening;
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

        Screen.SetResolution(1920, 1080, false);

        SceneType = Define.Scene.Login;

        StartCoroutine(Loading());
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
    IEnumerator Loading()
    {
        List<string> path = new List<string>();
        path.Add("Sounds/LoginBgm");
        path.Add("Sounds/MainBgm");
        path.Add("Sounds/BossBgm");
        foreach (string s in path)
        {
            ResourceRequest request = Resources.LoadAsync<AudioClip>(s);
            while (!request.isDone)
            {
                yield return null;
            }
            AudioClip loadedAudioClip = request.asset as AudioClip;
            Managers.Sound.SetBgmLoading(s, loadedAudioClip);
        }
        Managers.Instance.LoadGameData();

        Managers.Sound.SetAudioVolume(Define.Sound.Bgm, Managers.Instance.data.bgmVolume);
        Managers.Sound.SetAudioVolume(Define.Sound.Effect, Managers.Instance.data.eftVolume);
        Managers.Sound.Play("LoginBgm", Define.Sound.Bgm);

        Managers.Web.BaseUrl = "https://localhost:5001/api";

#if UNITY_SERVER
        Managers.Network.Master = true;
        LoginMaster();
#endif
        Managers.UI.ShowSceneUI<UI_TitleScene>();
    }
}
