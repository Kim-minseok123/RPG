using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameEnd_Popup : UI_Popup
{
    enum Buttons
    {
        YesBtn,
        NoBtn,
    }
    public override void Init()
    {
        base.Init();
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.YesBtn).gameObject.BindEvent((point) =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
        });
        GetButton((int)Buttons.NoBtn).gameObject.BindEvent((point) =>
        {
            (Managers.UI.SceneUI as UI_GameScene).isGameQuitPopup = false;
            Managers.UI.ClosePopupUI();
        });
    }
}
