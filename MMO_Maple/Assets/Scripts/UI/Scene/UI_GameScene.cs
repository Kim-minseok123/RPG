using DG.Tweening;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    public UI_Inventory InvenUI { get; private set; }

    public Sprite Beginner;
    public Sprite Warrior;
    public Sprite Archer;
    MyPlayerController MyPlayer;
    enum Images
    {
        PlayerImage,
        QuickSlotIconImageQ,
        QuickSlotIconImageW,
        QuickSlotIconImageE,
        QuickSlotIconImageR,
        QuickSlotIconImage1,
        QuickSlotIconImage2,
        QuickSlotIconImage3,
        QuickSlotIconImage4,
        PlayerClassImage,
    }
    enum Texts
    {
        PlayerClassText,
        PlayerNameText,
        LevelText,
    }
    enum Sliders
    {
        HpInfoSlider,
        MpInfoSlider,
    }
    enum GameObjects
    {
        QuickSlotSetQ,
        QuickSlotSetW,
        QuickSlotSetE,
        QuickSlotSetR,
        QuickSlotSet1,
        QuickSlotSet2,
        QuickSlotSet3,
        QuickSlotSet4,
    }
    public override void Init()
    {
        base.Init();

        InvenUI = GetComponentInChildren<UI_Inventory>();

        InvenUI.gameObject.SetActive(false);

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        BindObject(typeof(GameObjects));

        MyPlayer = Managers.Object.MyPlayer;

        GetText((int)Texts.PlayerNameText).text = MyPlayer.Stat.Level.ToString();

        switch (MyPlayer.ClassType)
        {
            case (int)ClassTypes.Beginner:
                GetText((int)Texts.PlayerClassText).text = "초보자";
                GetImage((int)Images.PlayerClassImage).sprite = Beginner;
                break;
            case (int)ClassTypes.Warrior:
                GetText((int)Texts.PlayerClassText).text = "전사";
                GetImage((int)Images.PlayerClassImage).sprite = Warrior;
                break;
            case (int)ClassTypes.Archer:
                GetText((int)Texts.PlayerClassText).text = "궁수";
                GetImage((int)Images.PlayerClassImage).sprite = Archer;
                break;
        }

        GetText((int)Texts.LevelText).text = MyPlayer.objectInfo.StatInfo.Level.ToString();

        Sprite nullSprite = Managers.Resource.Load<Sprite>("UI/Content/Mini_background"); 
        GetImage((int)Images.QuickSlotIconImageQ).sprite = nullSprite;
        GetImage((int)Images.QuickSlotIconImageW).sprite = nullSprite;
        GetImage((int)Images.QuickSlotIconImageE).sprite = nullSprite;
        GetImage((int)Images.QuickSlotIconImageR).sprite = nullSprite;
        GetImage((int)Images.QuickSlotIconImage1).sprite = nullSprite;
        GetImage((int)Images.QuickSlotIconImage2).sprite = nullSprite;
        GetImage((int)Images.QuickSlotIconImage3).sprite = nullSprite;
        GetImage((int)Images.QuickSlotIconImage4).sprite = nullSprite;

        Setting();
    }
    public void Setting()
    {
        
    }
    public void ChangeHpOrMp()
    {
        int MaxHp = MyPlayer.MaxHp;
        int Hp = MyPlayer.Hp;
        int MaxMp = MyPlayer.MaxMp;
        int Mp = MyPlayer.Mp;

        float hpRatio = Mathf.Max((float)Hp / MaxHp, 0f);
        float mpRatio = Mathf.Max((float)Mp / MaxMp, 0f);

        Get<Slider>((int)Sliders.HpInfoSlider).DOValue(hpRatio, 0.5f).SetEase(Ease.OutExpo);
        Get<Slider>((int)Sliders.MpInfoSlider).DOValue(mpRatio, 0.5f).SetEase(Ease.OutExpo);
    }
}
