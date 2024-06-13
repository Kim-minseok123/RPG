using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ItemInfoCanvas : UI_Base
{
    ItemData itemData;
    enum Texts
    {
        ItemInfoNameText,
        RequirementClassText,
        RequirementLevelText,
        DescriptionText
    }
    enum Images
    {
        ItemInfoImage
    }
    enum GameObjects
    {
        ItemInfoObj
    }
    string classColor = "white";
    string levelColor = "white";
    public override void Init()
    {
        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        RefreshUI();

        var rectTransform = GetObject((int)GameObjects.ItemInfoObj).GetComponent<RectTransform>();

        Vector2 mousePosition = Input.mousePosition;
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, mousePosition, null, out movePos);

        rectTransform.anchoredPosition = movePos;
    }
    public void Setting(ItemData data, bool classType = true, bool level = true)
    {
        itemData = data;

        if (!classType) classColor = "red";
        if (!level) levelColor = "red";

        RefreshUI();
    }
    private void RefreshUI()
    {
        if (itemData == null) return;
        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        GetImage((int)Images.ItemInfoImage).sprite = icon;
        GetText((int)Texts.ItemInfoNameText).text = itemData.name;

        switch (itemData.itemType)
        {
            case ItemType.Weapon:
                WeaponData weapon = (WeaponData)itemData;
                GetText((int)Texts.RequirementLevelText).text = $"<color={levelColor}>REQ LEV :\t{weapon.requirementLevel}</color>";
                GetText((int)Texts.RequirementClassText).text = $"<color={classColor}>REQ ClS :\t{weapon.requirementClass}</color>";
                GetText((int)Texts.DescriptionText).text = "공격력 : +" + weapon.damage;
                break;
            case ItemType.Armor:
                ArmorData armor = (ArmorData)itemData;
                GetText((int)Texts.RequirementLevelText).text = $"<color={levelColor}>REQ LEV :\t{armor.requirementLevel}</color>";
                GetText((int)Texts.RequirementClassText).text = $"<color={classColor}>REQ ClS :\t{armor.requirementClass}</color>";
                GetText((int)Texts.DescriptionText).text = "방어력 : +" + armor.defence;
                break;
            case ItemType.Consumable:
                ConsumableData consumable = (ConsumableData)itemData;
                GetText((int)Texts.RequirementLevelText).text = "";
                GetText((int)Texts.RequirementClassText).text = "";
                GetText((int)Texts.DescriptionText).text = consumable.description;
                break;
        }
    }
    public void Update()
    {
        var rectTransform = GetObject((int)GameObjects.ItemInfoObj).GetComponent<RectTransform>();

        Vector2 mousePosition = Input.mousePosition;
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, mousePosition, null, out movePos);

        rectTransform.anchoredPosition = movePos;
    }
}
