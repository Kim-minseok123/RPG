using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{
		public const int VisionCells = 5;

		public int RoomId { get; set; }

		Dictionary<int, Player> _players = new Dictionary<int, Player>();
		Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
		Dictionary<int, DropItem> _dropItem = new Dictionary<int, DropItem>();
		Dictionary<int, Npc> _npc = new Dictionary<int, Npc>();
		public int PlayerCount { get { return _players.Count; } }
		public void Init(int mapId, int zoneCells)
		{
			for (int i = 0; i < 1; i++)
			{
				SpawnMob();
            }
			Npc npc = ObjectManager.Instance.Add<Npc>();
			npc.Init(1);
			EnterGame(npc);
		}
		public void SpawnMob()
		{
            Skeleton skeleton = ObjectManager.Instance.Add<Skeleton>();
            skeleton.Init(1);
            EnterGame(skeleton);
        }
		public void Update()
		{
			Flush();
		}
		Random _rand = new Random();
		public void EnterGame(GameObject gameObject)
		{
			if (gameObject == null)
				return;

			GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

			if (type == GameObjectType.Player)
			{
				Player player = gameObject as Player;
				_players.Add(gameObject.Id, player);
				player.Room = this;

				//player.RefreshAdditionalStat();

				//Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
				//GetZone(player.CellPos).Players.Add(player);

				// 본인한테 정보 전송
				{
					S_EnterGame enterPacket = new S_EnterGame();
					enterPacket.Player = player.Info;
					player.Session.Send(enterPacket);
					{
						S_EquipItemList equipItemList = new S_EquipItemList();
						equipItemList.ObjectId = player.Id;
						for (int i = 0; i < player.Inven.EquipItems.Length; i++)
						{
							if (player.Inven.EquipItems[i] != null)
								equipItemList.TemplateIds.Add(player.Inven.EquipItems[i].TemplateId);
						}
						player.Session.Send(equipItemList);

						S_SkillList skillListPacket = new S_SkillList();

						foreach (var skillinfo in player.HaveSkillData)
						{
							SkillInfo info = new SkillInfo();
							info.SkillId = skillinfo.Key;
							info.Level = skillinfo.Value;
							skillListPacket.Skills.Add(info);
						}

						player.Session.Send(skillListPacket);

						S_QuickSlot quickSlotPacket = new S_QuickSlot();
						foreach (var info in player.QuickSlot)
						{
							QuickSlotInfo quickSlotInfo = new QuickSlotInfo() { SlotName = info.Key, TemplateId = info.Value };
							quickSlotPacket.Info.Add(quickSlotInfo);
						}
						player.Session.Send(quickSlotPacket);
					}
                    //player.Vision.Update();

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p && !p.Session.Master)
                            spawnPacket.Objects.Add(p.Info);
                        
                    }
					foreach (Monster m in _monsters.Values)
					{
						spawnPacket.Objects.Add(m.Info);
					}
                    foreach (Npc n in _npc.Values)
                    {
                        spawnPacket.Objects.Add(n.Info);
                    }
                    player.Session.Send(spawnPacket);
					foreach (Player p in _players.Values)
					{
						if (player != p && !p.Session.Master) 
						{
                            S_EquipItemList equipItemLists = new S_EquipItemList();
							equipItemLists.ObjectId = p.Id;
                            for (int i = 0; i < p.Inven.EquipItems.Length; i++)
                            {
                                if (p.Inven.EquipItems[i] != null)
                                    equipItemLists.TemplateIds.Add(p.Inven.EquipItems[i].TemplateId);
                            }
                            player.Session.Send(equipItemLists);
                        }
					}
                }
			}
			else if (type == GameObjectType.Monster)
			{
				Monster monster = gameObject as Monster;
				_monsters.Add(gameObject.Id, monster);
				monster.Room = this;
                monster.Update();
			}
			else if (type == GameObjectType.Dropitem)
			{
				DropItem dropItem = gameObject as DropItem;
				_dropItem.Add(gameObject.Id, dropItem);
                dropItem.Room = this;

                dropItem.Update();
			}
			else if(type == GameObjectType.Npc)
			{
				Npc npc = gameObject as Npc;
				_npc.Add(gameObject.Id, npc);
				npc.Room = this;
			}

			// 타인한테 정보 전송
			{
				S_Spawn spawnPacket = new S_Spawn();
				spawnPacket.Objects.Add(gameObject.Info);
				Broadcast(spawnPacket, gameObject.Id);
				if (type == GameObjectType.Player)
				{
					Player player = gameObject as Player;
                    S_EquipItemList equipItemList = new S_EquipItemList();
                    equipItemList.ObjectId = player.Id;
                    for (int i = 0; i < player.Inven.EquipItems.Length; i++)
                    {
                        if (player.Inven.EquipItems[i] != null)
                            equipItemList.TemplateIds.Add(player.Inven.EquipItems[i].TemplateId);
                    }
                    Broadcast(equipItemList, gameObject.Id);
                }
            }
		}
		public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;
				List<Monster> monsters = _monsters.Values.ToList();
				foreach (var monster in monsters)
				{
					if (monster.Target != null && monster.Target == player)
						monster.Target = null;
				}
				player.OnLeaveGame();
				player.Room = null;

				// 본인한테 정보 전송
				{
					S_LeaveGame leavePacket = new S_LeaveGame();
					player.Session.Send(leavePacket);
				}
			}
			else if (type == GameObjectType.Monster)
			{
				Monster monster = null;
				if (_monsters.Remove(objectId, out monster) == false)
					return;

				monster.Room = null;
				if (_monsters.Count <= 0) SpawnMob();
            }
			else if (type == GameObjectType.Dropitem)
			{
				DropItem dropItem = null;
				if (_dropItem.Remove(objectId, out dropItem) == false)
					return;

                dropItem.Room = null;
			}
			else
			{
				return;
			}

			// 타인한테 정보 전송
			{
				S_Despawn despawnPacket = new S_Despawn();
				despawnPacket.ObjectIds.Add(objectId);
				Broadcast(despawnPacket);
			}
		}
		Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in _players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}
		public void Broadcast(IMessage packet, int id = -1)
		{
			foreach(Player p in _players.Values)
			{
				if (id != -1 && p.Id == id) continue;
				p.Session.Send(packet);
			}
		}
	}
}
