using Data;
using DG.Tweening;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    public UI_Inventory InvenUI { get; private set; }
    public UI_Stat StatUI { get; private set; }
    public UI_Equip EquipUI { get; private set; }
    public UI_Skill SkillUI { get; private set; }
    public Dictionary<string, int> QuickSlotSkill = new();
    public Dictionary<string, int> QuickSlotItem = new();
    List<UI_Base> _playerPopup = new();
    int _curPopupSortOrder = 1;
    GameObject dragObj;
    GameObject dragIcon;
    public Sprite Beginner;
    public Sprite Warrior;
    public Sprite Archer;
    public bool NpcTrigger = false;
    MyPlayerController _myPlayer;
    enum Images
    {
        QuickSlotIconImageQ,
        QuickSlotIconImageW,
        QuickSlotIconImageE,
        QuickSlotIconImageR,
        QuickSlotIconImage1,
        QuickSlotIconImage2,
        QuickSlotIconImage3,
        QuickSlotIconImage4,
        PlayerImage,
        PlayerClassImage,
    }
    enum Texts
    {
        PlayerClassText,
        PlayerNameText,
        LevelText,
        QuickSlotNumText1,
        QuickSlotNumText2,
        QuickSlotNumText3,
        QuickSlotNumText4
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
        PlayerInfo,
        QuickSlot,
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
        QuickSlotChange(0, true, "Q");
        QuickSlotChange(1, true, "W");
        QuickSlotChange(2, true, "E");
        QuickSlotChange(3, true, "R");
        QuickSlotChange(4, false, "1");
        QuickSlotChange(5, false, "2");
        QuickSlotChange(6, false, "3");
        QuickSlotChange(7, false, "4");
        ChangeHpOrMp();
        ChangeExp();
    }
    void QuickSlotChange(int i, bool isSkill, string str)
    {
        Image _icon = GetImage(i);
        _icon.gameObject.BindEvent((e) =>
        {
            var quickSlot = isSkill ? QuickSlotSkill : QuickSlotItem;
            if (quickSlot.TryGetValue(str, out int templateId) == false) return;
            dragObj = Managers.Resource.Instantiate("UI/UI_DragObj");
            dragIcon = Util.FindChild(dragObj, "Icon");
            if (dragIcon == null) return;
            dragIcon.GetComponent<Image>().sprite = _icon.sprite;
        }, Define.UIEvent.DragEnter);
        _icon.gameObject.BindEvent((e) =>
        {
            if (dragObj == null) return;
            if (dragIcon == null) return;
            dragIcon.transform.position = e.position;
        }, Define.UIEvent.Drag);
        _icon.gameObject.BindEvent((e) =>
        {
            if (dragIcon == null) return;
            if (dragObj == null) return;
            Managers.Resource.Destroy(dragObj);
            var quickSlot = isSkill ? QuickSlotSkill : QuickSlotItem;
            if (quickSlot.TryGetValue(str, out int templateId) == false) return;
            if (e.pointerCurrentRaycast.gameObject.name == null) return;
            (Managers.UI.SceneUI as UI_GameScene).RequestQuickSlotUI(e.pointerCurrentRaycast.gameObject.name, templateId, isSkill);
        }, Define.UIEvent.DragEnd);
    }
    public void RefreshUI()
    {
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
        DrawQuickSlot();
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
        RefreshUI();
    }
    public void OpenUI(string uiName = null)
    {
        if (NpcTrigger) return;
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
    public void RequestQuickSlotUI(string pos, int templateId, bool isSkill)
    {
        if (NpcTrigger) return;
        switch (pos)
        {
            case "QuickSlotIconImageQ":
                if(isSkill)
                    RegisterQuickSlot("Q", templateId, true);
                break;
            case "QuickSlotIconImageW":
                if(isSkill)
                    RegisterQuickSlot("W", templateId, true);
                break;
            case "QuickSlotIconImageE":
                if(isSkill)
                    RegisterQuickSlot("E", templateId, true);
                break;
            case "QuickSlotIconImageR":
                if(isSkill)
                    RegisterQuickSlot("R", templateId, true);
                break;
            case "QuickSlotIconImage1":
                if(!isSkill)
                    RegisterQuickSlot("1", templateId, false);
                break;
            case "QuickSlotIconImage2":
                if(!isSkill)
                    RegisterQuickSlot("2", templateId, false);
                break;
            case "QuickSlotIconImage3":
                if(!isSkill)
                    RegisterQuickSlot("3", templateId, false);
                break;
            case "QuickSlotIconImage4":
                if(!isSkill)
                    RegisterQuickSlot("4", templateId, false);
                break;
            default:
                return;
        }
    }
    public void RegisterQuickSlot(string keyName, int templateId, bool isSkill)
    {
        if (NpcTrigger) return;

        var quickSlot = isSkill ? QuickSlotSkill : QuickSlotItem;

        if (quickSlot.TryGetValue(keyName, out int curTemplateId) && curTemplateId == templateId)
            return;

        quickSlot[keyName] = templateId;

        var keysToRemove = quickSlot
            .Where(slot => slot.Key != keyName && slot.Value == templateId)
            .Select(slot => slot.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            quickSlot.Remove(key);
        }
        DrawQuickSlot();
    }
    public void DrawQuickSlot()
    {
        {
            Sprite nullSprite = Managers.Resource.Load<Sprite>("UI/Content/Mini_background");
            GetImage((int)Images.QuickSlotIconImageQ).sprite = nullSprite;
            GetImage((int)Images.QuickSlotIconImageW).sprite = nullSprite;
            GetImage((int)Images.QuickSlotIconImageE).sprite = nullSprite;
            GetImage((int)Images.QuickSlotIconImageR).sprite = nullSprite;
            GetImage((int)Images.QuickSlotIconImage1).sprite = nullSprite;
            GetImage((int)Images.QuickSlotIconImage2).sprite = nullSprite;
            GetImage((int)Images.QuickSlotIconImage3).sprite = nullSprite;
            GetImage((int)Images.QuickSlotIconImage4).sprite = nullSprite;
            GetText((int)Texts.QuickSlotNumText1).text = "";
            GetText((int)Texts.QuickSlotNumText2).text = "";
            GetText((int)Texts.QuickSlotNumText3).text = "";
            GetText((int)Texts.QuickSlotNumText4).text = "";
        }
        foreach (var Slot in QuickSlotSkill)
        {
            if (Managers.Data.SkillDict.TryGetValue(Slot.Value, out Skill skill) == false) return;
            Sprite sprite = Managers.Resource.Load<Sprite>($"Textures/Skill/{skill.name}");
            switch (Slot.Key)
            {
                case "Q":
                    GetImage((int)Images.QuickSlotIconImageQ).sprite = sprite;
                    break;
                case "W":
                    GetImage((int)Images.QuickSlotIconImageW).sprite = sprite;
                    break;
                case "E":
                    GetImage((int)Images.QuickSlotIconImageE).sprite = sprite;
                    break;
                case "R":
                    GetImage((int)Images.QuickSlotIconImageR).sprite = sprite;
                    break;
            }
        }
        foreach (var Slot in QuickSlotItem)
        {
            if (Managers.Data.ItemDict.TryGetValue(Slot.Value, out ItemData itemData) == false) return;
            Sprite sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
            int itemCount = Managers.Inven.FindItemCount(Slot.Value);
            switch (Slot.Key)
            {
                case "1":
                    GetImage((int)Images.QuickSlotIconImage1).sprite = sprite;
                    GetText((int)Texts.QuickSlotNumText1).text = itemCount.ToString();
                    break;
                case "2":
                    GetImage((int)Images.QuickSlotIconImage2).sprite = sprite;
                    GetText((int)Texts.QuickSlotNumText2).text = itemCount.ToString();
                    break;
                case "3":
                    GetImage((int)Images.QuickSlotIconImage3).sprite = sprite;
                    GetText((int)Texts.QuickSlotNumText3).text = itemCount.ToString();
                    break;
                case "4":
                    GetImage((int)Images.QuickSlotIconImage4).sprite = sprite;
                    GetText((int)Texts.QuickSlotNumText4).text = itemCount.ToString();
                    break;
            }
        }
    }
    public void InvokeSkillQuickSlot(string keyName)
    {
        if (NpcTrigger) return;

        if (QuickSlotSkill.TryGetValue(keyName, out int templateId) == false)
            return;
        if (Managers.Data.SkillDict.TryGetValue(templateId, out Skill skill) == false) 
            return;
        _myPlayer.QuickAction(skill);
        DrawQuickSlot();

    }
    public void InvokeItemQuickSlot(string keyName)
    {
        if (NpcTrigger) return;

        if (QuickSlotItem.TryGetValue(keyName, out int templateId) == false)
            return;
        Item quickItem = Managers.Inven.Find(item => item.TemplateId == templateId && item.ItemType == ItemType.Consumable);
        if (quickItem == null)
            return;
        C_UseItem useItemPacket = new C_UseItem();
        useItemPacket.ItemDbId = quickItem.ItemDbId;
        useItemPacket.Count = 1;
        Managers.Network.Send(useItemPacket);
        DrawQuickSlot();
    }
    public void CloseAllUI()
    {
        for (int i = 0; i < _playerPopup.Count; i++)
        {
            CloseUI();
        }
    }
    public void CloseInfoAndSlot()
    {
        GetObject((int)GameObjects.PlayerInfo).SetActive(false);
        GetObject((int)GameObjects.QuickSlot).SetActive(false);
    }
    public void OpenInfoAndSlot()
    {
        GetObject((int)GameObjects.PlayerInfo).SetActive(true);
        GetObject((int)GameObjects.QuickSlot).SetActive(true);
    }
}
