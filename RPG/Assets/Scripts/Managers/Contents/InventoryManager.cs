using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager 
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
    public Item[] EquipItems { get; } = new Item[8];
    public int Money { get; set; }
    public void Add(Item item)
    {
        Items.Add(item.ItemDbId, item);
    }
    public Item Get(int itemDbId)
    {
        Item item = null;
        Items.TryGetValue(itemDbId, out item);
        return item;
    }
    public Item Find(Func<Item, bool> condition)
    {
        foreach (Item item in Items.Values) {
            if (condition.Invoke(item))
            {
                return item;
            }
        }
        return null;
    }
    public int FindItemCount(int templateId)
    {
        int result = 0;
        foreach (Item item in Items.Values)
        {
            if (item.TemplateId == templateId)
                result += item.Count;
        }
        return result;
    }
    public void Clear()
    {
        Items.Clear();
    }
    public void Remove(Item item)
    {
        Items.Remove(item.ItemDbId);
    }
    public void EquipAdd(int i, Item item)
    {
        EquipItems[i - 1] = item;
    }
    public Item EquipGet(int i)
    {
        if (EquipItems[i] == null)
            return null;
        return EquipItems[i];
    }
    
    public void EquipClear()
    {
        for (int i = 0; i < EquipItems.Length; i++)
        {
            EquipItems[i] = null;
        }
    }
    public void EquipRemove(int i)
    {
        EquipItems[i - 1] = null;
    }
    public void AddMoney(int add)
    {
        Money += add;
        if(Money < 0)
            Money = 0;
    }
    public Item EquipFind(Func<Item, bool> condition)
    {
        for (int i = 0; i < EquipItems.Length; i++)
        {
            if (EquipItems[i] != null)
                if (condition.Invoke(EquipItems[i]))
                    return EquipItems[i];
        }
        return null;
    }
}
