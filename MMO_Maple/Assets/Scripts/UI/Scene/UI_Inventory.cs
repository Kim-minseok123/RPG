using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : UI_Base
{
    public List<UI_InvenSlot> Items { get; } = new List<UI_InvenSlot>();

    public override void Init()
    {
        Items.Clear();

        GameObject grid = transform.Find("InventoryGrid").gameObject;

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
        
    }
}
