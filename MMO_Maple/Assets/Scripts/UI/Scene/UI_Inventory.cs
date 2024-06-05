using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Inventory : UI_Base
{
    enum Buttons
    {
        ExitButton
    }
    enum Texts
    {
        CoinText
    }
    public List<UI_InvenSlot> Items { get; } = new List<UI_InvenSlot>();
    public GameObject grid;
    public override void Init()
    {
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => { var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("UI_Inventory"); });

        Items.Clear();

        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < 24; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/SubItem/UI_InvenSlot", grid.transform);
            UI_InvenSlot item = go.GetOrAddComponent<UI_InvenSlot>();
            Items.Add(item);
        }

        RefreshUI();
    }
    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        List<Item> items = Managers.Inven.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });
        
        /*foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 24)
                continue;

            Items[item.Slot].SetItem(item);
        }*/
        
        int slot = 0;
        for (int i = 0; i < Items.Count; i++)
        {
            if (slot < items.Count && items[slot].Slot == i)
            {
                Items[i].SetItem(items[slot]);
                slot++;
            }
            else
            {
                Items[i].SetItem(null);
            }
        }
            
        GetText((int)Texts.CoinText).text = Managers.Inven.Money.ToString("N0");
    }

    public override void InfoRemove()
    {
        foreach (var item in Items)
        {
            item.InfoRemoveSlot();
        }
    }
}
