using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{		
		public void HandleEquipItem(Player player, C_EquipItem equipPacket)
		{
			if (player == null)
				return;

			player.HandleEquipItem(equipPacket);
		}
        public void GetDropItem(Player player, C_GetDropItem dropItemPacket)
        {
            if (player == null) return;

            DropItem dropItem = null;
            if (_dropItem.TryGetValue(dropItemPacket.DropItemId, out dropItem) == false) return;

            if (dropItem.Owner != null && dropItem.Owner != player) return;
            // 템 자석핵 견제
            Vector3 dropItemPos = Utils.PositionsToVector3(dropItem.Pos);
            Vector3 playerPos = Utils.PositionsToVector3(player.Pos);
            if (Vector3.Distance(dropItemPos, playerPos) > 5f)
            {
                S_Banish banPacket = new S_Banish();
                player.Session.Send(banPacket);
            }
            S_MotionOrEffect motionPacket = new S_MotionOrEffect();
            motionPacket.ObjectId = player.Id;
            motionPacket.ActionName = "Drop";
            Broadcast(player.Pos, motionPacket);
            if (dropItem._rewardData.itemId == 1000)
                DbTransaction.GetItemPlayer(player, dropItem._rewardData, this, dropItem);
            else
            {
                ItemData itemData = null;
                if (DataManager.ItemDict.TryGetValue(dropItem._rewardData.itemId, out itemData) == false) return;
                if (itemData.itemType == ItemType.Consumable && ((ConsumableData)itemData).maxCount > 1)
                    DbTransaction.GetConsumableItemPlayer(player, dropItem._rewardData, this, dropItem);
                else
                    DbTransaction.GetItemPlayer(player, dropItem._rewardData, this, dropItem);
            }
        }
        public void HandleUseItem(Player player, C_UseItem useItemPacket)
        {
            if (player == null)
                return;
            player.HandleUseItem(useItemPacket);
        }
        public void HandleChangeItemSlot(Player player, C_ChangeItemSlot itemSlotPacket)
        {
            if (player == null) return;
            player.HandleChangeItemSlot(itemSlotPacket);
        }
        public void HandleAddItem(Player player, C_AddItem addItemPacket)
        {
            if (player == null) return;
            if(addItemPacket.TemplateId == 1000)
            {
                RewardData rewardData = new RewardData();
                rewardData.itemId = 1000;
                rewardData.count = addItemPacket.Count;
                DbTransaction.GetItemPlayer(player, rewardData, this);
                return;
            }
            ItemData itemData;
            if (DataManager.ItemDict.TryGetValue(addItemPacket.TemplateId, out itemData) == false) return;
            if(itemData.itemType == ItemType.Consumable)
            {
                if (addItemPacket.IsBuy)
                {
                    int minusMoney = addItemPacket.Count * itemData.sellGold;
                    if (minusMoney > player.Inven.Money) return;
                    RewardData rewardData = new RewardData();
                    rewardData.itemId = addItemPacket.TemplateId;
                    rewardData.count = addItemPacket.Count;
                    DbTransaction.GetConsumableItemPlayer(player, rewardData, this, minusMoney: minusMoney);
                }
                else
                {
                    RewardData rewardData = new RewardData();
                    rewardData.itemId = addItemPacket.TemplateId;
                    rewardData.count = addItemPacket.Count;
                    DbTransaction.GetConsumableItemPlayer(player, rewardData, this);
                }
            }
            else
            {
                if (addItemPacket.IsBuy)
                {
                    int minusMoney = addItemPacket.Count * itemData.sellGold;
                    if (minusMoney > player.Inven.Money) return;
                    RewardData rewardData = new RewardData();
                    rewardData.itemId = addItemPacket.TemplateId;
                    rewardData.count = addItemPacket.Count;
                    DbTransaction.GetItemPlayer(player, rewardData, this, minusMoney: minusMoney);
                }
                else
                {
                    RewardData rewardData = new RewardData();
                    rewardData.itemId = addItemPacket.TemplateId;
                    rewardData.count = addItemPacket.Count;
                    DbTransaction.GetItemPlayer(player, rewardData, this);
                }
            }
        }
        public void HandleRemoveItem(Player player, C_RemoveItem removeItemPacket)
        {
            if (player == null) return;
            ItemData itemData;
            if (DataManager.ItemDict.TryGetValue(removeItemPacket.TemplateId, out itemData) == false) return;
            Item item = player.Inven.Get(removeItemPacket.ItemDbId);
            if (item == null) return;
            if (item.Count - removeItemPacket.Count < 0) return;
            if(removeItemPacket.IsSell)
                DbTransaction.RemoveItem(player, this, removeItemPacket, plusMoney:(itemData.sellGold /2) * removeItemPacket.Count);
            else
                DbTransaction.RemoveItem(player, this, removeItemPacket);
        }
    }
}
