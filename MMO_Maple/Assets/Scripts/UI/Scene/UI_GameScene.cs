using Data;
using DG.Tweening;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    public UI_Inventory InvenUI { get; private set; }
    public UI_Stat StatUI { get; private set; }
    public UI_Equip EquipUI { get; private set; }
    public UI_Skill SkillUI { get; private set; }

    List<UI_Base> _playerPopup = new();
    int _curPopupSortOrder = 1;

    public Sprite Beginner;
    public Sprite Warrior;
    public Sprite Archer;

    MyPlayerController _myPlayer;
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
        ExpInfoSlider,
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

        _curPopupSortOrder = 1;

        InvenUI = GetComponentInChildren<UI_Inventory>();
        _playerPopup.Add(InvenUI);
        StatUI = GetComponentInChildren<UI_Stat>();
        _playerPopup.Add(StatUI);
        EquipUI = GetComponentInChildren<UI_Equip>();
        _playerPopup.Add(EquipUI);
        SkillUI = GetComponentInChildren<UI_Skill>();
        _playerPopup.Add(SkillUI);
        //...//

        InvenUI.gameObject.SetActive(false);
        StatUI.gameObject.SetActive(false);
        EquipUI.gameObject.SetActive(false);
        SkillUI.gameObject.SetActive(false);

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        BindObject(typeof(GameObjects));

        _myPlayer = Managers.Object.MyPlayer;

        GetText((int)Texts.PlayerNameText).text = _myPlayer.objectInfo.Name.ToString();

        switch (_myPlayer.ClassType)
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

        GetText((int)Texts.LevelText).text = _myPlayer.objectInfo.StatInfo.Level.ToString();

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
        ChangeHpOrMp();
        ChangeExp();
    }
    public void Setting()
    {
        
    }
    public void ChangeHpOrMp()
    {
        int MaxHp = _myPlayer.MaxHp;
        int Hp = _myPlayer.Hp;
        int MaxMp = _myPlayer.MaxMp;
        int Mp = _myPlayer.Mp;

        float hpRatio = Mathf.Max((float)Hp / MaxHp, 0f);
        float mpRatio = Mathf.Max((float)Mp / MaxMp, 0f);

        Get<Slider>((int)Sliders.HpInfoSlider).DOValue(hpRatio, 0.5f).SetEase(Ease.OutExpo);
        Get<Slider>((int)Sliders.MpInfoSlider).DOValue(mpRatio, 0.5f).SetEase(Ease.OutExpo);
    }
    public void ChangeExp()
    {
        ExpData expData = null;
        if (Managers.Data.ExpDict.TryGetValue(_myPlayer.Stat.Level, out expData) == false) return;
        int MaxExp = expData.requiredExp;
        int exp = _myPlayer.Exp;

        float expRatio = Mathf.Max((float)exp / MaxExp, 0f);
        Get<Slider>((int)Sliders.ExpInfoSlider).DOValue(expRatio, 0.5f).SetEase(Ease.OutExpo);
        GetText((int)Texts.LevelText).text = _myPlayer.objectInfo.StatInfo.Level.ToString();
    }
    public void OpenUI(string uiName = null)
    {
        switch(uiName)
        {
            case "Inven":
                InvenUI.gameObject.GetComponent<Canvas>().sortingOrder = _curPopupSortOrder++;
                InvenUI.gameObject.SetActive(true);
                InvenUI.RefreshUI();
                break;
            case "Stat":
                StatUI.gameObject.GetComponent<Canvas>().sortingOrder = _curPopupSortOrder++;
                StatUI.gameObject.SetActive(true);
                StatUI.RefreshUI();
                break;
            case "Skill":
                SkillUI.gameObject.GetComponent<Canvas>().sortingOrder = _curPopupSortOrder++;
                SkillUI.gameObject.SetActive(true);
                SkillUI.RefreshUI();
                break;
            case "Equip":
                EquipUI.gameObject.GetComponent<Canvas>().sortingOrder = _curPopupSortOrder++;
                EquipUI.gameObject.SetActive(true);
                EquipUI.RefreshUI();
                break;
        }
    }
    public void CloseUI(string uiName = null)
    {
        if (_curPopupSortOrder <= 1)
            return;

        if (string.IsNullOrEmpty(uiName))
        {
            UI_Base lastOpenedUI = _playerPopup.FirstOrDefault(ui => ui.gameObject.activeSelf && ui.GetComponent<Canvas>().sortingOrder == _curPopupSortOrder - 1);
            if (lastOpenedUI != null)
            {
                lastOpenedUI.InfoRemove();
                lastOpenedUI.gameObject.SetActive(false);
                lastOpenedUI.GetComponent<Canvas>().sortingOrder = 0;
                _curPopupSortOrder--;
            }
        }
        else
        {
            UI_Base uiToClose = _playerPopup.FirstOrDefault(ui => ui.name.Equals(uiName));
            if (uiToClose != null && uiToClose.gameObject.activeSelf)
            {
                uiToClose.InfoRemove();
                uiToClose.gameObject.SetActive(false);
                uiToClose.GetComponent<Canvas>().sortingOrder = 0;
                _curPopupSortOrder--;
            }
        }
    }
}
