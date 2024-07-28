using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_PlayerSellInfo : UI_Base
{
    public int templateId;
    UI_NpcSell_Popup popup;
    public int itemDbId;
    ItemData itemData;
    GameObject description;
    bool satisfiedClass = false;
    bool satisfiedLevel = false;

    enum Images
    {
        IconImage
    }
    enum Texts
    {
        ItemNameText,
        SellCoinText,
        CurHaveText
    }
    public override void Init()
    {
        BindImage(typeof(Images));
        BindText(typeof(Texts));
    }
    public void Setting(int templateId, int itemDbId, UI_NpcSell_Popup popup)
    {
        this.templateId = templateId;
        this.itemDbId = itemDbId;
        this.popup = popup;
        if (Managers.Data.ItemDict.TryGetValue(templateId, out itemData) == false) return;
        RefreshUI();
    }
    public void RefreshUI()
    {
        GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);

        GetText((int)Texts.ItemNameText).text = itemData.name;
        GetText((int)Texts.SellCoinText).text = (itemData.sellGold / 2) + " 골드";

        Item item = Managers.Inven.Get(itemDbId);
        if (item == null) return;
        if (itemData.itemType == ItemType.Consumable)
        {
            GetText((int)Texts.CurHaveText).text = item.Count.ToString();
            ConsumableData cmData = (ConsumableData)itemData;
            if(cmData.maxCount > 1)
            {
                // 몇개를 판매하시겠습니까? 출력 팝업
                GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
                {
                    popup.InfoRemove();
                    if (e.clickCount < 2) return;

                    Managers.Sound.Play("ButtonClick");

                    Managers.UI.ShowPopupUI<UI_SelectConfirm_Popup>().Setting("몇개를 판매하시겠습니까?", true, (count) =>
                    {
                        if (count <= 0) return;
                        Item item = Managers.Inven.Get(itemDbId);
                        if (item == null) return;
                        if (item.Count - count < 0)
                        {
                            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("판매할 개수보다 가지고 있는 아이템의 개수가 적습니다.");
                            return;
                        }
                        Managers.Sound.Play("ItemGet");
                        C_RemoveItem removeItemPacket = new C_RemoveItem();
                        removeItemPacket.TemplateId = itemData.id;
                        removeItemPacket.ItemDbId = itemDbId;
                        removeItemPacket.Count = count;
                        removeItemPacket.IsSell = true;
                        Managers.Network.Send(removeItemPacket);
                    });
                });
            }
            else
            {
                GetText((int)Texts.CurHaveText).gameObject.SetActive(false);

                // 정말 판매하시겠습니까? 출력 팝업
                GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
                {
                    popup.InfoRemove();
                    if (e.clickCount < 2) return;

                    Managers.Sound.Play("ButtonClick");

                    Managers.UI.ShowPopupUI<UI_SelectConfirm_Popup>().Setting("정말 판매하시겠습니까?", false, (count) =>
                    {
                        if (count != -1) return;
                        Item item = Managers.Inven.Get(itemDbId);
                        if (item == null) return;
                        Managers.Sound.Play("ItemGet");
                        C_RemoveItem removeItemPacket = new C_RemoveItem();
                        removeItemPacket.TemplateId = itemData.id;
                        removeItemPacket.ItemDbId = itemDbId;
                        removeItemPacket.Count = 1;
                        removeItemPacket.IsSell = true;
                        Managers.Network.Send(removeItemPacket);
                    });
                });
            }
        }
        else
        {
            GetText((int)Texts.CurHaveText).gameObject.SetActive(false);

            if (itemData.itemType == ItemType.Weapon)
            {
                WeaponData wp = (WeaponData)itemData;
                if (wp.requirementLevel > Managers.Object.MyPlayer.Stat.Level)
                    satisfiedLevel = false;
                else satisfiedLevel = true;
                if (wp.requirementClass.Equals(Util.ChagneClassType((ClassTypes)Managers.Object.MyPlayer.ClassType)))
                    satisfiedClass = true;
                else satisfiedClass = false;
            }
            else
            {
                ArmorData ar = (ArmorData)itemData;
                if (ar.requirementLevel > Managers.Object.MyPlayer.Stat.Level)
                    satisfiedLevel = false;
                else satisfiedLevel = true;
                if (ar.requirementClass.Equals(Util.ChagneClassType((ClassTypes)Managers.Object.MyPlayer.ClassType)))
                    satisfiedClass = true;
                else satisfiedClass = false;
            }
            // 정말 판매하시겠습니까? 출력 팝업
            GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
            {
                popup.InfoRemove();
                if (e.clickCount < 2) return;

                Managers.Sound.Play("ButtonClick");

                Managers.UI.ShowPopupUI<UI_SelectConfirm_Popup>().Setting("정말 판매하시겠습니까?", false, (count) =>
                {
                    if (count != -1) return;
                    Item item = Managers.Inven.Get(itemDbId);
                    if (item == null) return;
                    Managers.Sound.Play("ItemGet");
                    C_RemoveItem removeItemPacket = new C_RemoveItem();
                    removeItemPacket.TemplateId = itemData.id;
                    removeItemPacket.ItemDbId = itemDbId;
                    removeItemPacket.Count = 1;
                    removeItemPacket.IsSell = true;
                    Managers.Network.Send(removeItemPacket);
                });
            });
        }
        GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
        {
            if (itemData == null) return;
            description = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
            description.GetComponent<UI_ItemInfoCanvas>().Setting(itemData, satisfiedClass, satisfiedLevel);
        }, Define.UIEvent.PointerEnter);
        GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
        {
            if (itemData == null) return;
            if (description != null)
                Managers.Resource.Destroy(description);
        }, Define.UIEvent.PointerExit);
    }
    public void RemoveInfo()
    {
        if (description != null)
            Managers.Resource.Destroy(description);
    }
}
