using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_NpcSellInfo : UI_Base
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
        SellCoinText
    }
    public override void Init()
    {
        BindImage(typeof(Images));
        BindText(typeof(Texts));


    }
    public void Setting(int templateId, UI_NpcSell_Popup sellPopup)
    {
        this.templateId = templateId;
        popup = sellPopup;
        if (Managers.Data.ItemDict.TryGetValue(templateId, out itemData) == false) return;
        RefreshUI();
    }
    public void RefreshUI()
    {
        GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
        
        GetText((int)Texts.ItemNameText).text = itemData.name;
        GetText((int)Texts.SellCoinText).text = itemData.sellGold + " 골드";


        if (itemData.itemType == ItemType.Consumable)
        {
            ConsumableData cmData = (ConsumableData)itemData;
            if (cmData.maxCount > 1)
            {
                // 몇개를 구매하시겠습니까? 출력 팝업
                GetImage((int)Images.IconImage).gameObject.BindEvent((e) => 
                {
                    popup.InfoRemove();
                    if (e.clickCount < 2) return;
                    Managers.Sound.Play("ButtonClick");

                    Managers.UI.ShowPopupUI<UI_SelectConfirm_Popup>().Setting("몇개를 구매하시겠습니까?", true, (count) => 
                    {
                        if (count <= 0) return;
                        int canMoney = Managers.Inven.Money - (itemData.sellGold * count);
                        if (canMoney < 0)
                        {
                            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("골드가 부족합니다.");
                            return;
                        }
                        Managers.Sound.Play("ItemGet");
                        C_AddItem addItemPacket = new C_AddItem();
                        addItemPacket.TemplateId = itemData.id;
                        addItemPacket.Count = count;
                        addItemPacket.IsBuy = true;
                        Managers.Network.Send(addItemPacket);
                    });
                });
            }
            else
            {
                // 정말 구매하시겠습니까? 출력 팝업
                GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
                {
                    popup.InfoRemove();

                    if (e.clickCount < 2) return;
                    Managers.Sound.Play("ButtonClick");

                    Managers.UI.ShowPopupUI<UI_SelectConfirm_Popup>().Setting("정말 구매하시겠습니까?", false, (count) =>
                    {
                        if (count != -1) return;
                        int canMoney = Managers.Inven.Money - (itemData.sellGold * 1);
                        if(canMoney < 0)
                        {
                            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("골드가 부족합니다.");
                            return;
                        }
                        Managers.Sound.Play("ItemGet");
                        C_AddItem addItemPacket = new C_AddItem();
                        addItemPacket.TemplateId = itemData.id;
                        addItemPacket.Count = 1;
                        addItemPacket.IsBuy = true;
                        Managers.Network.Send(addItemPacket);
                    });
                });
            }
        }
        else
        {
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

            // 정말 구매하시겠습니까? 출력 팝업
            GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
            {
                popup.InfoRemove();

                if (e.clickCount < 2) return;

                Managers.Sound.Play("ButtonClick");

                Managers.UI.ShowPopupUI<UI_SelectConfirm_Popup>().Setting("정말 구매하시겠습니까?", false, (count) =>
                {
                    if (count != -1) return;
                    int canMoney = Managers.Inven.Money - (itemData.sellGold * 1);
                    if (canMoney < 0)
                    {
                        Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("골드가 부족합니다.");
                        return;
                    }
                    Managers.Sound.Play("ItemGet");
                    C_AddItem addItemPacket = new C_AddItem();
                    addItemPacket.TemplateId = itemData.id;
                    addItemPacket.Count = 1;
                    addItemPacket.IsBuy = true;
                    Managers.Network.Send(addItemPacket);
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
