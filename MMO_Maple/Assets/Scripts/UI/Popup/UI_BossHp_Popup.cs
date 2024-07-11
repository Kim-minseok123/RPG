using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BossHp_Popup : UI_Popup
{
    bool _init = false;
    GameObject boss;
    enum Sliders
    {
        HpSlider
    }
    enum Texts
    {
        NameText
    }
    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        BindText(typeof(Texts));

        _init = true;
        RefreshUI();
    }
    public void Setting(GameObject gameObject)
    {
        boss = gameObject;
        RefreshUI();
    }
    public void RefreshUI()
    {
        if (_init == false || boss == null)
            return;

        GetText((int)Texts.NameText).text = boss.name;
    }
    public void ChangeHp(float hp)
    {
        if (_init == false)
            return;

        Get<Slider>((int)Sliders.HpSlider).DOValue(hp, 0.5f).SetEase(Ease.OutExpo);
    }
}
