using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
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
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                    }
                }
            });
        }
		

		public static void GetItemPlayer(Player player, RewardData rewardData, GameRoom room, DropItem dropItem = null)
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
                playerDb.Money = player.Inven.Money + rewardData.count;
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
								}
							});
                        }
                    }
					else
					{
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

                                    player.Session.Send(itemPacket);
                                    dropItem?.DisappearItem();
                                }
                            });
                        }

                    }
				}
			});
		}
    }
}
