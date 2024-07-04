using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_NpcSell_Popup : UI_Popup
{
    int templateId;
    NpcData npcData;
    enum GameObjects
    {
        NpcSellContent,
        PlayerSellContent
    }
    enum Texts
    {
        CoinText
    }
    public List<UI_NpcSellInfo> npcSellItems = new List<UI_NpcSellInfo>();
    public List<UI_PlayerSellInfo> playerSellItems = new List<UI_PlayerSellInfo>();
    public override void Init()
    {
        base.Init();

        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));
        
        if (Managers.Data.NpcDict.TryGetValue(templateId, out npcData) == false)
            return;

        RefreshUI();
    }
    public void Setting(int templateId)
    {
        this.templateId = templateId;

        if (Managers.Data.NpcDict.TryGetValue(templateId, out npcData) == false)
            return;

        RefreshUI();
    }
    public void RefreshUI()
    {
        if (npcData == null)
            return;

        npcSellItems.Clear();
        playerSellItems.Clear();

        foreach (Transform child in GetObject((int)GameObjects.NpcSellContent).transform)
            Destroy(child.gameObject);
        foreach (Transform child in GetObject((int)GameObjects.PlayerSellContent).transform)
            Destroy(child.gameObject);

        foreach (NpcSellList item in npcData.npcSellLists)
        {
            GameObject go = Managers.Resource.Instantiate("UI/SubItem/UI_NpcSellInfo", GetObject((int)GameObjects.NpcSellContent).transform);
            go.GetComponent<UI_NpcSellInfo>().Setting(item.TemplateId, this);
        }
        List<Item> items = Managers.Inven.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });
        foreach (Item item in items)
        {
            GameObject go = Managers.Resource.Instantiate("UI/SubItem/UI_PlayerSellInfo", GetObject((int)GameObjects.PlayerSellContent).transform);
            go.GetComponent<UI_PlayerSellInfo>().Setting(item.TemplateId, item.ItemDbId, this);
        }
        GetText((int)Texts.CoinText).text = Managers.Inven.Money.ToString();
    }
    public override void InfoRemove()
    {
        foreach (var item in npcSellItems)
        {
            item.RemoveInfo();
        }
        foreach (var item in playerSellItems)
        {
            item.RemoveInfo();
        }
    }
}
