using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InvenSlot : UI_Base
{
    [SerializeField]
    Image _icon;
    [SerializeField]
    TextMeshProUGUI _countText;
    ItemData itemData;
    GameObject description;
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
                dragObj = Managers.Resource.Instantiate("UI/UI_SkillDrag");
                dragObj = Util.FindChild(dragObj, "Icon");
                if (dragObj == null) return;
                dragObj.GetComponent<Image>().sprite = _icon.sprite;
            }, Define.UIEvent.DragEnter);
            _icon.gameObject.BindEvent((e) =>
            {
                if (itemData == null) return;
                if (dragObj == null) return;
                dragObj.transform.position = e.position;
            }, Define.UIEvent.Drag);
            _icon.gameObject.BindEvent((e) =>
            {
                if (itemData == null) return;
                if (dragObj == null) return;
                Managers.Resource.Destroy(dragObj);
                if(itemData.itemType == ItemType.Consumable)
                    (Managers.UI.SceneUI as UI_GameScene).RequestQuickSlotUI(e.pointerCurrentRaycast.gameObject.name, TemplateId, false);
            }, Define.UIEvent.DragEnd);
        }
    }
    public void SetItem(Item item)
    {
        if(item == null)
        {
            ItemDbID = 0;
            TemplateId = 0;
            Count = 0;
            Equipped = false;

            _icon.gameObject.SetActive(false);
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
            if(item.ItemType == ItemType.Consumable)
            {
                _countText.gameObject.SetActive(true);
                _countText.text = item.Count.ToString();
            }
            else
            {
                ClassTypes? classTypes = Item.GetItemRequiredClassType(item);
                if (classTypes == null) return;

                if ((int)classTypes == Managers.Object.MyPlayer.ClassType) satisfiedClass = true;
                if (item.RequirementLevel <= Managers.Object.MyPlayer.Stat.Level) satisfiedLevel = true;
            }
        }
    }
    public void InfoRemoveSlot()
    {
        if (description != null)
            Managers.Resource.Destroy(description);
    }
}
