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
    enum Images
    {
        MonsterImage
    }
    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

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
        MonsterController monsterController = boss.GetComponent<MonsterController>();
        GetText((int)Texts.NameText).text = "LV. " + monsterController.Stat.Level + " " + boss.name;
        GetImage((int)Images.MonsterImage).sprite = Managers.Resource.Load<Sprite>($"Textures/{boss.name}");
    }
    public void ChangeHp(float hp)
    {
        if (_init == false)
            return;

        Get<Slider>((int)Sliders.HpSlider).DOValue(hp, 0.5f).SetEase(Ease.OutExpo);
    }
}
