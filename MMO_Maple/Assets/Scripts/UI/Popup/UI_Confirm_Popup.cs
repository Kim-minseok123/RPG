using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class UI_Confirm_Popup : UI_Popup
{
    Action _afterAction = null;
    enum Texts
    {
        ConfirmText
    }
    string _text;
    bool _init = false;
    public override void Init()
    {
        base.Init();

        BindText(typeof(Texts));
        _init = true;
        UpdateUI();

        StartCoroutine(CoDestroyUI());
    }

    private void UpdateUI()
    {
        if (_init == false) return;
        GetText((int)Texts.ConfirmText).text = _text;
    }

    public void Setting(string text, Action action = null)
    {
        _text = text;
        _afterAction = action;
        UpdateUI();
    }
    IEnumerator CoDestroyUI()
    {
        yield return new WaitForSeconds(2.5f);
        _afterAction?.Invoke();
        Managers.UI.ClosePopupUI(this);
    }
}
