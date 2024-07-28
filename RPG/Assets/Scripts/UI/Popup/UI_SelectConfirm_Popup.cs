using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_SelectConfirm_Popup : UI_Popup
{
    Action<int> _afterAction = null;
    string _text;
    bool _isConsumable;
    enum Texts
    {
        ConfirmText
    }
    enum GameObjects
    {
        AmountInput
    }
    enum Buttons
    {
        YesBtn,
        NoBtn,
    }
    public override void Init()
    {
        base.Init();
    }
    public void Setting(string text, bool isConsumable, Action<int> action = null)
    {
        _text = text;
        _afterAction = action;
        _isConsumable = isConsumable;
        RefreshUI();
    }
    public void RefreshUI()
    {
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));

        GetText((int)Texts.ConfirmText).text = _text;
        if (_isConsumable)
        {
            GetObject((int)GameObjects.AmountInput).SetActive(true);
            GetButton((int)Buttons.YesBtn).gameObject.BindEvent((e) => 
            {
                if (GetObject((int)GameObjects.AmountInput).GetComponent<TMP_InputField>().text == "") return;
                int count = int.Parse(GetObject((int)GameObjects.AmountInput).GetComponent<TMP_InputField>().text);
                if (count <= 0) return;
                Managers.UI.ClosePopupUI();
                _afterAction.Invoke(count);
            });
        }
        else
        {
            GetObject((int)GameObjects.AmountInput).SetActive(false);
            GetButton((int)Buttons.YesBtn).gameObject.BindEvent((e) =>
            {
                Managers.UI.ClosePopupUI();
                _afterAction.Invoke(-1);
            });
        }
        GetButton((int)Buttons.NoBtn).gameObject.BindEvent((e) =>
        {
            Managers.UI.ClosePopupUI();
        });
    }
}
