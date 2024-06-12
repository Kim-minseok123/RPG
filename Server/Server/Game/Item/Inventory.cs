using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
	public class Inventory
	{
		public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
		public Item[] EquipItems { get; } = new Item[8];
		public int Money { get; set; }
		public void Add(Item item)
		{
			Items.Add(item.ItemDbId, item);
		}
		public void Remove(Item item)
		{
			Items.Remove(item.ItemDbId);
		}

		public Item Get(int itemDbId)
		{
			Item item = null;
			Items.TryGetValue(itemDbId, out item);
			return item;
		}

		public Item Find(Func<Item, bool> condition)
		{
			foreach (Item item in Items.Values)
			{
				if (condition.Invoke(item))
					return item;
			}

			return null;
		}

		public int? GetEmptySlot()
		{
			for (int slot = 0; slot < 24; slot++)
			{
				Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
				if (item == null)
					return slot;
			}

			return null;
		}
        public void EquipAdd(int i, Item item)
        {
            EquipItems[i - 1] = item;
        }
        public Item EquipGet(int i)
        {
            if (EquipItems[i - 1] == null)
                return null;
            return EquipItems[i - 1];
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

		public int? AlreadyHaveConsumable(int templateId)
		{
			ItemData itemData = null;
			if(DataManager.ItemDict.TryGetValue(templateId, out itemData) == false) return -1;
			ConsumableData consumableItem = (ConsumableData)itemData;
			for (int slot = 0; slot < 24; slot++)
			{
				Item item = Items.Values.FirstOrDefault(i => i.Slot == slot && i.TemplateId == templateId && i.Count < consumableItem.maxCount);
				if (item != null)
				{
					return item.ItemDbId;
				}
			}
			return null;
		}
    }
}
