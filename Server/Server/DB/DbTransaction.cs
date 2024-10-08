﻿using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.DB
{
	public partial class DbTransaction : JobSerializer
	{
		public static DbTransaction Instance { get; } = new DbTransaction();
		public static void SavePlayerStat(Player player, GameRoom room)
		{
            if (player == null || room == null)
                return;

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
			playerDb.MaxHp = player.Stat.MaxHp;
			playerDb.MaxMp = player.Stat.MaxMp;
            playerDb.Hp = player.Stat.Hp;
			playerDb.Mp = player.Stat.Mp;
			playerDb.StatPoint = player.Stat.StatPoint;
			playerDb.Str = player.Stat.Str;
			playerDb.Dex = player.Stat.Dex;
			playerDb.Luk = player.Stat.Luk;
			playerDb.Int = player.Stat.Int;
			playerDb.posX = player.Pos.PosX;
			playerDb.posY = player.Pos.PosY;
			playerDb.posZ = player.Pos.PosZ;
			playerDb.Level = player.Stat.Level;
			playerDb.PlayerClass = player.classType;
            playerDb.SkillPoint = player.Stat.SkillPoint;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.MaxHp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Mp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.MaxMp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.StatPoint)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Str)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Dex)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Luk)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Int)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.posX)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.posY)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.posZ)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Level)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.PlayerClass)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.SkillPoint)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                    }
                }
            });
        }
		public static void GetItemPlayer(Player player, RewardData rewardData, GameRoom room, DropItem dropItem = null, int minusMoney = 0)
		{
			if (player == null || rewardData == null || room == null)
				return;

			// TODO : 살짝 문제가 있긴 하다...
			int? slot = player.Inven.GetEmptySlot();
			if (slot == null)
				return;

			ItemDb itemDb = new ItemDb()
			{
				TemplateId = rewardData.itemId,
				Count = rewardData.count,
				Slot = slot.Value,
				OwnerDbId = player.PlayerDbId
			};

            PlayerDb playerDb = new PlayerDb();
            if (itemDb.TemplateId == 1000)
			{
                playerDb.PlayerDbId = player.PlayerDbId;
                Random random = new Random();
                if(dropItem != null)
                {
                    int randVal = rewardData.count / 10;
                    playerDb.Money = player.Inven.Money + rewardData.count + random.Next(-randVal, randVal + 1);
                }
                else
                {
                    playerDb.Money = player.Inven.Money + rewardData.count;
                }
                
            }
                
            // You
            Instance.Push(() =>
			{
				using (AppDbContext db = new AppDbContext())
				{
					if(itemDb.TemplateId == 1000)
					{
                        db.Entry(playerDb).State = EntityState.Unchanged;
                        db.Entry(playerDb).Property(nameof(PlayerDb.Money)).IsModified = true;
                        bool success = db.SaveChangesEx();
                        if (success)
                        {
							room.Push(() =>
							{
								if (itemDb.TemplateId == 1000)
								{
									player.Inven.Money += itemDb.Count;
									S_AddItem itemPacket = new S_AddItem();
									itemPacket.Money = itemDb.Count;
									player.Session.Send(itemPacket);
									dropItem?.DisappearItem();
                                    player.QuestInven.UpdateQuestProgress(QuestType.Collection, new CollectionQuestGoals { collectionId = 1000, count = itemDb.Count });
								}
							});
                        }
                    }
					else
					{
                        if(minusMoney > 0)
                        {
                            playerDb.PlayerDbId = player.PlayerDbId;
                            playerDb.Money = Math.Max(player.Inven.Money - minusMoney, 0);
                            db.Entry(playerDb).State = EntityState.Unchanged;
                            db.Entry(playerDb).Property(nameof(PlayerDb.Money)).IsModified = true;
                        }
                        db.Items.Add(itemDb);
                        bool success = db.SaveChangesEx();
                        if (success)
						{
                            room.Push(() =>
                            {
                                Item newItem = Item.MakeItem(itemDb);
                                player.Inven.Add(newItem);
                                // Client Noti
                                {
                                    S_AddItem itemPacket = new S_AddItem();
                                    ItemInfo itemInfo = new ItemInfo();
                                    itemInfo.MergeFrom(newItem.Info);
                                    itemPacket.Items.Add(itemInfo);
                                    if (minusMoney > 0)
                                    {
                                        itemPacket.Money = -minusMoney;
                                        player.Inven.Money -= minusMoney;
                                    }
                                    player.Session.Send(itemPacket);
                                    Console.WriteLine("플레이어 " + player.Id + " 소지한 골드 : " + player.Inven.Money);
                                    player.QuestInven.UpdateQuestProgress(QuestType.Collection, new CollectionQuestGoals { collectionId = newItem.TemplateId, count = itemDb.Count });
                                    dropItem?.DisappearItem();
                                }
                            });
                        }

                    }
				}
			});
		}
        public static void GetConsumableItemPlayer(Player player, RewardData rewardData, GameRoom room, DropItem dropItem = null, int minusMoney = 0)
        {
            if (player == null || rewardData == null || room == null)
                return;
            ItemData itemData = null;
            if (DataManager.ItemDict.TryGetValue(rewardData.itemId, out itemData) == false) return;
            ConsumableData cmData = (ConsumableData)itemData;
            if (rewardData.count > cmData.maxCount) return;
            int? findDbId = player.Inven.AlreadyHaveConsumable(rewardData.itemId);

            if (findDbId == -1)
                return;
            else if(findDbId == null)
            {
                int? slot = player.Inven.GetEmptySlot();
                if (slot == null)
                    return;

                ItemDb itemDb = new ItemDb()
                {
                    TemplateId = rewardData.itemId,
                    Count = rewardData.count,
                    Slot = slot.Value,
                    OwnerDbId = player.PlayerDbId
                };
                Instance.Push(() =>
                {
                    using (AppDbContext db = new AppDbContext())
                    {
                        if (minusMoney > 0)
                        {
                            PlayerDb playerDb = new PlayerDb();
                            playerDb.PlayerDbId = player.PlayerDbId;
                            playerDb.Money = Math.Max(player.Inven.Money - minusMoney, 0);
                            db.Entry(playerDb).State = EntityState.Unchanged;
                            db.Entry(playerDb).Property(nameof(PlayerDb.Money)).IsModified = true;
                        }
                        db.Items.Add(itemDb);
                        bool success = db.SaveChangesEx();
                        if (success)
                        {
                            room.Push(() =>
                            {
                                Item newItem = Item.MakeItem(itemDb);
                                player.Inven.Add(newItem);
                                {
                                    S_AddItem itemPacket = new S_AddItem();
                                    ItemInfo itemInfo = new ItemInfo();
                                    itemInfo.MergeFrom(newItem.Info);
                                    itemPacket.Items.Add(itemInfo);
                                    if (minusMoney > 0)
                                    {
                                        itemPacket.Money = -minusMoney;
                                        player.Inven.Money -= minusMoney;
                                    }
                                    player.Session.Send(itemPacket);
                                    dropItem?.DisappearItem();
                                    player.QuestInven.UpdateQuestProgress(QuestType.Collection, new CollectionQuestGoals { collectionId = rewardData.itemId, count = rewardData.count });

                                    Console.WriteLine("플레이어 " + player.Id + " 소지한 골드 : " + player.Inven.Money);


                                }
                            });
                        }
                    }
                });
            }
            else
            {
                Item curItem = player.Inven.Get((int)findDbId); 
                if (curItem == null) return;
                int newCount = curItem.Count + rewardData.count;
                if (newCount <= cmData.maxCount)
                {
                    curItem.Count = newCount;

                    ItemDb itemDb = new ItemDb()
                    {
                        ItemDbId = curItem.ItemDbId,
                        Count = curItem.Count,
                    };

                    Instance.Push(() =>
                    {
                        using (AppDbContext db = new AppDbContext())
                        {
                            if (minusMoney > 0)
                            {
                                PlayerDb playerDb = new PlayerDb();
                                playerDb.PlayerDbId = player.PlayerDbId;
                                playerDb.Money = Math.Max(player.Inven.Money - minusMoney, 0);
                                db.Entry(playerDb).State = EntityState.Unchanged;
                                db.Entry(playerDb).Property(nameof(PlayerDb.Money)).IsModified = true;
                            }
                            db.Entry(itemDb).State = EntityState.Unchanged;
                            db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;
                            bool success = db.SaveChangesEx();
                            if (success)
                            {
                                room.Push(() =>
                                {
                                    S_ChangeConsumableItem changeConsumableItem = new S_ChangeConsumableItem
                                    {
                                        ItemDbId = curItem.ItemDbId,
                                        Count = curItem.Count
                                    };
                                    player.Session.Send(changeConsumableItem);
                                    if (minusMoney > 0)
                                    {
                                        S_AddItem itemPacket = new S_AddItem();
                                        itemPacket.Money = -minusMoney;
                                        player.Inven.Money -= minusMoney;
                                        player.Session.Send(itemPacket);
                                        Console.WriteLine("플레이어 " + player.Id + " 소지한 골드 : " + player.Inven.Money);
                                    }
                                    player.QuestInven.UpdateQuestProgress(QuestType.Collection, new CollectionQuestGoals { collectionId = rewardData.itemId, count = rewardData.count });
                                    dropItem?.DisappearItem();
                                });
                            }
                        }
                    });
                }
                else
                {
                    int? slot = player.Inven.GetEmptySlot();
                    if (slot == null)
                        return;
                    curItem.Count = cmData.maxCount;
                    int leaveCount = newCount - cmData.maxCount;

                    ItemDb curitemDb = new ItemDb()
                    {
                        ItemDbId = curItem.ItemDbId,
                        Count = curItem.Count,
                    };

                    ItemDb newItemDb = new ItemDb()
                    {
                        TemplateId = rewardData.itemId,
                        Count = leaveCount,
                        Slot = slot.Value,
                        OwnerDbId = player.PlayerDbId
                    };

                    Instance.Push(() =>
                    {
                        using (AppDbContext db = new AppDbContext())
                        {
                            if (minusMoney > 0)
                            {
                                PlayerDb playerDb = new PlayerDb();
                                playerDb.PlayerDbId = player.PlayerDbId;
                                playerDb.Money = Math.Max(player.Inven.Money - minusMoney, 0);
                                db.Entry(playerDb).State = EntityState.Unchanged;
                                db.Entry(playerDb).Property(nameof(PlayerDb.Money)).IsModified = true;
                            }
                            db.Entry(curitemDb).State = EntityState.Unchanged;
                            db.Entry(curitemDb).Property(nameof(ItemDb.Count)).IsModified = true;
                            db.Items.Add(newItemDb);
                            bool success = db.SaveChangesEx();
                            if (success)
                            {
                                room.Push(() =>
                                {
                                    S_ChangeConsumableItem changeConsumableItem = new S_ChangeConsumableItem
                                    {
                                        ItemDbId = curItem.ItemDbId,
                                        Count = curItem.Count
                                    };
                                    player.Session.Send(changeConsumableItem);

                                    Item newItem = Item.MakeItem(newItemDb);
                                    player.Inven.Add(newItem);
                                    {
                                        S_AddItem itemPacket = new S_AddItem();
                                        ItemInfo itemInfo = new ItemInfo();
                                        itemInfo.MergeFrom(newItem.Info);
                                        itemPacket.Items.Add(itemInfo);
                                        if (minusMoney > 0)
                                        {
                                            itemPacket.Money = -minusMoney;
                                            player.Inven.Money -= minusMoney;
                                        }
                                        player.Session.Send(itemPacket);

                                        player.QuestInven.UpdateQuestProgress(QuestType.Collection, new CollectionQuestGoals { collectionId = rewardData.itemId, count = rewardData.count });
                                        Console.WriteLine("플레이어 " + player.Id + " 소지한 골드 : " + player.Inven.Money);


                                        dropItem?.DisappearItem();
                                    }
                                });
                            }
                        }
                    });

                }
            }
        }
        public static void RemoveItem(Player player, GameRoom room, Item item, int count, int plusMoney = 0)
        {
            if (player == null) return;
            if (item == null)
                return;
            int remainCount = item.Count - count;

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                { 
                    if(item.Count - count > 0)
                    {
                        ItemDb itemDb = new ItemDb()
                        {
                            ItemDbId = item.ItemDbId,
                            Count = remainCount
                        };
                        db.Entry(itemDb).State = EntityState.Unchanged;
                        db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;
                    }
                    else
                    {
                        ItemDb itemDb = db.Items.SingleOrDefault(i=> i.ItemDbId == item.ItemDbId && i.OwnerDbId == player.PlayerDbId);
                        if (itemDb == null) return;
                        db.Items.Remove(itemDb);
                    }
                    if(plusMoney > 0)
                    {
                        PlayerDb playerDb = new PlayerDb()
                        {
                            PlayerDbId = player.PlayerDbId,
                            Money = player.Inven.Money + plusMoney
                        };
                        db.Entry(playerDb).State = EntityState.Unchanged;
                        db.Entry(playerDb).Property(nameof(PlayerDb.Money)).IsModified = true;
                    }
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        room.Push(() =>
                        {
                            if(remainCount > 0)
                            {
                                item.Count = remainCount;
                                if (plusMoney > 0)
                                    player.Inven.Money += plusMoney;
                            }
                            else
                            {
                                player.Inven.Remove(item);
                                if (plusMoney > 0)
                                    player.Inven.Money += plusMoney;
                            }
                            S_RemoveItem removeOk = new S_RemoveItem();
                            ItemInfo itemInfo = new ItemInfo();
                            itemInfo.TemplateId = item.TemplateId;
                            itemInfo.ItemDbId = item.ItemDbId;
                            itemInfo.Count = remainCount;
                            removeOk.Items.Add(itemInfo);
                            removeOk.Money = plusMoney;
                            player.Session.Send(removeOk);
                            Console.WriteLine("플레이어 "+player.Id  + " 소지한 골드 : "+ player.Inven.Money);
                        });
                    }
                }
            });
        }
    }
}
