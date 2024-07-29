using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Google.Protobuf.Protocol;

public class UI_Setting_Popup : MonoBehaviour
{
    public Slider bgm;
    public Slider eft;
    public TextMeshProUGUI text;
    string SceneName;
    public void Start()
    {
        bgm.value = Managers.Instance.data.bgmVolume;
        eft.value = Managers.Instance.data.eftVolume;
        SceneName = SceneManager.GetActiveScene().name;
        if (SceneName.Equals("Game") || SceneName.Equals("Boss"))
        {
            text.text = "캐릭터 선택창";
        }
        else
        {
            text.text = "게임 종료";
        }
        GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
    }
    IEnumerator FadeOutCanvas()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        Tween tw = cg.DOFade(0f, 0.5f);
        yield return tw.WaitForCompletion();
        Destroy(gameObject);
    }
    public void ExitBtn()
    {
        Managers.Sound.Play("ButtonClick");
        StartCoroutine(FadeOutCanvas());
    }
    public void ChangeBgmValue()
    {
        float value = bgm.value;
        Managers.Sound.SetAudioVolume(Define.Sound.Bgm, value);
    }
    public void ChangeEftValue()
    {
        float value = eft.value;
        Managers.Sound.SetAudioVolume(Define.Sound.Effect, value);
    }
    public void GameEndBtn()
    {
        if(SceneName.Equals("Game") || SceneName.Equals("Boss"))
        {
            C_RequestLeaveGame leaveGame = new C_RequestLeaveGame();
            leaveGame.ObjectId = Managers.Object.MyPlayer.Id;
            Managers.Network.Send(leaveGame);
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
        }
    }
}
