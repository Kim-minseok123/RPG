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
    public List<UI_InvenSlot> Items { get; } = new List<UI_InvenSlot>();
    public GameObject grid;
    public override void Init()
    {
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => { var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("Inven"); });

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

        foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 24)
                continue;

            Items[item.Slot].SetItem(item);
        }
    }
}
