using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using TMPro;

public class UI_TitleScene : UI_Scene
{
    enum Images
    {
        TouchImage
    }
    enum Texts
    {
        TouchScreenText
    }
    enum Buttons
    {
        SettingBtn
    }
    Coroutine _coTouchText { get; set; }
    public override void Init()
    {
        base.Init();

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;

        GetComponent<CanvasGroup>().alpha = 0f;
        GetComponent<CanvasGroup>().DOFade(1f, 1f);

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        _coTouchText = StartCoroutine(CoTouchTextFade());
        GetImage((int)Images.TouchImage).gameObject.BindEvent(OnClickTouchImage);

        GetButton((int)Buttons.SettingBtn).gameObject.BindEvent((pointData) => { Managers.Sound.Play("ButtonClick");  Managers.Resource.Instantiate("UI/Popup/UI_Setting_Popup"); });
    }
    void OnClickTouchImage(PointerEventData data)
    {
        StopCoroutine(_coTouchText);

        GetImage((int)Images.TouchImage).gameObject.SetActive(false);

        Managers.Sound.Play("ButtonClick");

        Managers.UI.ShowPopupUI<UI_Login_Popup>();
    }

    IEnumerator CoTouchTextFade()
    {
        TextMeshProUGUI text = GetText((int)Texts.TouchScreenText);

        while (true)
        {
            Tween tw = text.DOFade(0f, 1f);
            yield return tw.WaitForCompletion();
            tw = text.DOFade(1f, 1f);
            yield return tw.WaitForCompletion();
        }
    }
}
