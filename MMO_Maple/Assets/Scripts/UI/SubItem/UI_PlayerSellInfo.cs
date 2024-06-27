using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_PlayerSellInfo : UI_Base
{
    public int templateId;
    ItemData itemData;
    GameObject description;

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
    public void Setting(int templateId)
    {
        this.templateId = templateId;
        if (Managers.Data.ItemDict.TryGetValue(templateId, out itemData) == false) return;
        RefreshUI();
    }
    public void RefreshUI()
    {
        GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);

        GetText((int)Texts.ItemNameText).text = itemData.name;
        GetText((int)Texts.SellCoinText).text = itemData.sellGold + " 골드";
        GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
        {
            if (itemData == null) return;
            description = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
            description.GetComponent<UI_ItemInfoCanvas>().Setting(itemData);
        }, Define.UIEvent.PointerEnter);
        GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
        {
            if (itemData == null) return;
            if (description != null)
                Managers.Resource.Destroy(description);
        }, Define.UIEvent.PointerExit);

        if (itemData.itemType == ItemType.Consumable)
        {
            ConsumableData cmData = (ConsumableData)itemData;
            if(cmData.maxCount > 1)
            {
                // 몇개를 판매하시겠습니까? 출력 팝업
            }
            else
            {
                // 정말 판매하시겠습니까? 출력 팝업
            }
        }
        else
        {
            // 정말 판매하시겠습니까? 출력 팝업
        }
    }
    public void RemoveInfo()
    {
        if (description != null)
            Managers.Resource.Destroy(description);
    }
}
