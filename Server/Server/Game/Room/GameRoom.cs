using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

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
		Player MasterPlayer = null;
		public int PlayerCount { get { return _players.Count; } }
		public float mapMinX = 300;
		public float mapMinZ = 300;
		public float mapMaxX = 390;
		public float mapMaxZ = 390;
		public float mapSizeX { get { return mapMaxX - mapMinX; } }
		public float mapSizeZ { get { return mapMaxZ - mapMinZ; } }

		public Zone[,] Zones { get; private set; }
		public int ZoneCells { get; private set; }
		public Zone GetZone(Positions pos)
		{
			int x = (int)((pos.PosX - mapMinX) / ZoneCells);
			int z = (int)((mapMaxZ - pos.PosZ ) / ZoneCells);
			return GetZone(z, x);
		}
		public Zone GetZone(int z, int x)
		{
			if (x < 0 || x >= Zones.GetLength(1))
				return null;
			if (z < 0 || z >= Zones.GetLength(0))
				return null;
			return Zones[z, x];
		}
		public void Init(int mapId, int zoneCells)
		{

			ZoneCells = zoneCells;

			int countZ = (int)((mapSizeZ + zoneCells - 1) / zoneCells);
			int countX = (int)((mapSizeX + zoneCells - 1) / zoneCells);
			Zones = new Zone[countZ, countX];

			for (int z = 0; z < countZ; z++)
			{
				for (int x = 0; x < countX; x++)
				{
					Zones[z, x] = new Zone(z, x);
				}
			}
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
                if (player.Session.Master == true)
				{
					MasterPlayer = player;
				}

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

					GetZone(player.Pos).Players.Add(player);
					player.curZone = GetZone(player.Pos);
					if (player.Session.Master != true)
						player.Vision.Update();
					else
						SpawnMaster();
                }
            }
			else if (type == GameObjectType.Monster)
			{
				Monster monster = gameObject as Monster;
				_monsters.Add(gameObject.Id, monster);
				monster.Room = this;

				Zone zone = GetZone(monster.Pos);
				zone.Monsters.Add(monster);
				monster.curZone = zone;

                monster.Update();
			}
			else if (type == GameObjectType.Dropitem)
			{
				DropItem dropItem = gameObject as DropItem;
				_dropItem.Add(gameObject.Id, dropItem);
                dropItem.Room = this;

                Zone zone = GetZone(dropItem.Pos);
                zone.DropItems.Add(dropItem);
                dropItem.curZone = zone;

                dropItem.Update();
			}
			else if(type == GameObjectType.Npc)
			{
				Npc npc = gameObject as Npc;
				_npc.Add(gameObject.Id, npc);
				npc.Room = this;

                Zone zone = GetZone(npc.Pos);
                zone.Npcs.Add(npc);
                npc.curZone = zone;
            }

			// 타인한테 정보 전송
			{
				S_Spawn spawnPacket = new S_Spawn();
				spawnPacket.Objects.Add(gameObject.Info);
				Broadcast(gameObject.Pos, spawnPacket, gameObject.Id, true);
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
                    Broadcast(player.Pos, equipItemList, gameObject.Id, true);
                }
            }
		}
		public void SpawnMaster()
		{
			S_Spawn spawnPacket = new S_Spawn();
			foreach (var player in _players.Values)
			{
				spawnPacket.Objects.Add(player.Info);
			}
			foreach (var monster in _monsters.Values)
			{
                spawnPacket.Objects.Add(monster.Info);
            }
			foreach (var npc in _npc.Values)
			{
                spawnPacket.Objects.Add(npc.Info);
            }
			foreach (var dropItem in _dropItem.Values)
			{
                spawnPacket.Objects.Add(dropItem.Info);
            }
			MasterPlayer.Session.Send(spawnPacket);
        }
		public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);
			Positions pos = new Positions();
			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;
				pos = player.Pos;
				List<Monster> monsters = _monsters.Values.ToList();
				foreach (var monster in monsters)
				{
					if (monster.Target != null && monster.Target == player)
						monster.Target = null;
				}
				player.OnLeaveGame();
				player.curZone.Remove(player);
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
				pos = monster.Pos;
                monster.curZone.Remove(monster);

                monster.Room = null;
				if (_monsters.Count <= 0) SpawnMob();
            }
			else if (type == GameObjectType.Dropitem)
			{
				DropItem dropItem = null;
				if (_dropItem.Remove(objectId, out dropItem) == false)
					return;
				pos = dropItem.Pos;
                dropItem.curZone.Remove(dropItem);

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
				Broadcast(pos ,despawnPacket);
			}
		}
		public void Broadcast(Positions pos, IMessage packet, int id = -1, bool includeMaster = true)
		{
			if (includeMaster)
				BroadcastMaster(packet);

            Broadcast(pos, packet, id);
        }
		public void Broadcast(Positions pos, IMessage packet, int id)
		{
            List<Zone> zones = GetAdjacentZones(pos);
            foreach (Player p in zones.SelectMany(z => z.Players))
            {
                if ((id != -1 && p.Id == id) || p.Session.Master) continue;
                int dx = (int)(p.Pos.PosX - pos.PosX);
                int dz = (int)(p.Pos.PosZ - pos.PosZ);
                if (Math.Abs(dx) > GameRoom.VisionCells)
                    continue;
                if (Math.Abs(dz) > GameRoom.VisionCells)
                    continue;
                p.Session.Send(packet);
            }
        }
		public void BroadcastMaster(IMessage packet)
		{
			MasterPlayer?.Session.Send(packet);
		}
		public List<Player> GetAdjacentPlayers(Positions pos, int range = GameRoom.VisionCells) 
		{
            List<Zone> zones = GetAdjacentZones(pos, range);
            return zones.SelectMany(z => z.Players).ToList();
        }
        public List<Zone> GetAdjacentZones(Positions pos, int range = GameRoom.VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxZ = (int)(pos.PosZ + range);
            int minZ = (int)(pos.PosZ - range);
            int maxX = (int)(pos.PosX + range);
            int minX = (int)(pos.PosX - range);

            int leftTopIndexY = (int)((mapMaxZ - maxZ) / ZoneCells);
            int leftTopIndexX = (int)((minX - mapMinX) / ZoneCells);
            int rightBotIndexY = (int)((mapMaxZ - minZ) / ZoneCells);
            int rightBotIndexX = (int)((maxX - mapMinX) / ZoneCells);

            int startIndexY = Math.Min(leftTopIndexY, rightBotIndexY);
            int endIndexY = Math.Max(leftTopIndexY, rightBotIndexY);
            int startIndexX = Math.Min(leftTopIndexX, rightBotIndexX);
            int endIndexX = Math.Max(leftTopIndexX, rightBotIndexX);

            // Iterate through the indices and collect zones
            for (int y = startIndexY; y <= endIndexY; y++)
            {
                for (int x = startIndexX; x <= endIndexX; x++)
                {
                    Zone zone = GetZone(y, x);
                    if (zone != null)
                    {
                        zones.Add(zone);
                    }
                }
            }

            return zones.ToList();
        }

    }
}
