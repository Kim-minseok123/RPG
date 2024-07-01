using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class UI_InvenSlot : UI_Base
{
    [SerializeField]
    Image _icon;
    [SerializeField]
    TextMeshProUGUI _countText;
    ItemData itemData;
    GameObject description;
    GameObject dragIcon;
    GameObject dragObj;
    public int ItemDbID { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public bool Equipped { get; private set; }
    bool satisfiedClass = false;
    bool satisfiedLevel = false;

    public override void Init()
    {
        _icon.gameObject.BindEvent((e) =>
        {
            if (e.clickCount < 2) return;
            if (_icon.color.a == 0f)
                return;

            if (itemData == null)
                return;
            
            if (itemData.itemType == ItemType.Consumable)
            {
                C_UseItem useItemPacket = new C_UseItem();
                useItemPacket.ItemDbId = ItemDbID;
                useItemPacket.Count = 1;
                Managers.Network.Send(useItemPacket);
                return;
            }
            if (!satisfiedClass || !satisfiedLevel)
                return;
            if (description != null)
                Managers.Resource.Destroy(description);
            if (Managers.Object.MyPlayer.State != CreatureState.Idle) return;
            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = ItemDbID;
            equipPacket.Equipped = true;
            equipPacket.ObjectId = Managers.Object.MyPlayer.Id;
            Managers.Network.Send(equipPacket);
        });
        {
            _icon.gameObject.BindEvent((e) =>
            {
                if (itemData == null) return;
                description = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                description.GetComponent<UI_ItemInfoCanvas>().Setting(itemData, satisfiedClass, satisfiedLevel);
            }, Define.UIEvent.PointerEnter);

            _icon.gameObject.BindEvent((e) =>
            {
                if (description == null) return;
                Managers.Resource.Destroy(description);
            }, Define.UIEvent.PointerExit);
        }
        {
            _icon.gameObject.BindEvent((e) =>
            {
                if (itemData == null) return;
                dragObj = Managers.Resource.Instantiate("UI/UI_DragObj");
                dragIcon = Util.FindChild(dragObj, "Icon");
                if (dragIcon == null) return;
                dragIcon.GetComponent<Image>().sprite = _icon.sprite;
            }, Define.UIEvent.DragEnter);
            _icon.gameObject.BindEvent((e) =>
            {
                if (itemData == null) return;
                if (dragObj == null) return;
                dragIcon.transform.position = e.position;
            }, Define.UIEvent.Drag);
            _icon.gameObject.BindEvent((e) =>
            {
                if (itemData == null) return;
                if (dragObj == null) return;
                Managers.Resource.Destroy(dragObj);
                string name = e.pointerCurrentRaycast.gameObject.name;
                if (name == null) return;
                // 아이템 창끼리 인벤 교환
                if (name.Contains("InventorySlot_"))
                {
                    RequestChangeInvenSlot(ExtractNumberFromName(name));
                    return;
                }
                if(itemData.itemType == ItemType.Consumable && name != null)
                    (Managers.UI.SceneUI as UI_GameScene).RequestQuickSlotUI(name, TemplateId, false);
            }, Define.UIEvent.DragEnd);
        }
    }
    public void SetName(string name)
    {
        _icon.gameObject.name = name;
    }
    public void SetItem(Item item)
    {
        if(item == null)
        {
            ItemDbID = 0;
            TemplateId = 0;
            Count = 0;
            Equipped = false;
            itemData = null;
            _icon.color = new Color(1, 1, 1, 0);
            _countText.gameObject.SetActive(false);
        }
        else
        {
            ItemDbID = item.ItemDbId;
            TemplateId = item.TemplateId;
            Count = item.Count;
            Equipped = item.Equipped;

            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
            if (itemData == null) return;

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            _icon.sprite = icon;
            Color color = _icon.color;
            color.a = 1f;
            _icon.color = color;
            _icon.gameObject.SetActive(true);
            _icon.color = new Color(1, 1, 1, 1);
            if (item.ItemType == ItemType.Consumable)
            {
                _countText.gameObject.SetActive(true);
                _countText.text = item.Count.ToString();
            }
            else
            {
                _countText.text = "";

                ClassTypes? classTypes = Item.GetItemRequiredClassType(item);
                if (classTypes == null) return;

                if ((int)classTypes == Managers.Object.MyPlayer.ClassType) satisfiedClass = true;
                else satisfiedClass = false;
                if (item.RequirementLevel <= Managers.Object.MyPlayer.Stat.Level) satisfiedLevel = true;
                else satisfiedLevel = false;
            }
        }
    }
    public void InfoRemoveSlot()
    {
        if (description != null)
            Managers.Resource.Destroy(description);
    }
    public void RequestChangeInvenSlot(int changeSlotNum) 
    {
        if (changeSlotNum == -1) return;
        Item item = Managers.Inven.Get(ItemDbID);
        if (item == null) return;
        int curSlot = item.Slot;
        if (changeSlotNum == curSlot)
            return;

        C_ChangeItemSlot changeSlot = new C_ChangeItemSlot()
        { 
            ItemDbId = ItemDbID,
            CurSlot = curSlot,
            ChangeItemSlot = changeSlotNum
        };
        Managers.Network.Send(changeSlot);
    }
    public int ExtractNumberFromName(string name)
    {
        Match match = Regex.Match(name, @"\d+");

        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        return -1;
    }
}
