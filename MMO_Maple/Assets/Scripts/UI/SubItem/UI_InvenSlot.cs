using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InvenSlot : UI_Base
{
    [SerializeField]
    Image _icon;
    ItemData itemData;
    public int ItemDbID { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public bool Equipped { get; private set; }

    public override void Init()
    {
        _icon.gameObject.BindEvent((e) =>
        {
            if (_icon.color.a == 0f)
                return;
            Debug.Log("Click Item");

            if (itemData == null)
                return;
            // TODO : C_USE_ITEM
            if (itemData.itemType == ItemType.Consumable)
                return;

           /* C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = ItemDbID;
            equipPacket.Equipped = !Equipped;

            Managers.Network.Send(equipPacket);*/
        });

        _icon.gameObject.BindEvent((e) =>
        {

        }, Define.UIEvent.PointerEnter);

        _icon.gameObject.BindEvent((e) =>
        {

        }, Define.UIEvent.PointerExit);
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
        }
    }
}
