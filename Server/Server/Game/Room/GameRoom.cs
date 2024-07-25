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
		public int VisionCells = 5;

		public int RoomId { get; set; }
		public int mapId { get; set; }

		Dictionary<int, Player> _players = new Dictionary<int, Player>();
		Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
		Dictionary<int, DropItem> _dropItem = new Dictionary<int, DropItem>();
		Dictionary<int, Npc> _npc = new Dictionary<int, Npc>();
		Player MasterPlayer = null;
		public int PlayerCount { get { return _players.Count; } }
		public float mapMinX;
		public float mapMinZ;
		public float mapMaxX;
		public float mapMaxZ;
		public float mapSizeX { get { return mapMaxX - mapMinX; } }
		public float mapSizeZ { get { return mapMaxZ - mapMinZ; } }
		public List<Player> moveMapPlayer = new List<Player>();
		public Zone[,] Zones { get; private set; }
		public int ZoneCells { get; private set; }
		bool isRaid = false;
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
		public void Init(int mapId, int zoneCells, int minX, int minY, int maxX, int maxY, int visionCell)
		{
			this.mapId = mapId;
			ZoneCells = zoneCells;
            mapMinX = minX;
			mapMinZ = minY;
			mapMaxX = maxX;
			mapMaxZ = maxY;
			VisionCells = visionCell;
			moveMapPlayer.Clear();

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
			if(mapId == 1)
			{
                for (int i = 0; i < 3; i++)
                {
                    SpawnMob();
                }
                Npc Alli = ObjectManager.Instance.Add<Npc>();
                Alli.Init(1);
                EnterGame(Alli);
                Npc Robin = ObjectManager.Instance.Add<Npc>();
                Robin.Init(2);
                EnterGame(Robin);
            }	
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
					if (player.Session.Master == true)
					{
						player.Vision.SetVisionCell(100);
                    }
					else
                        player.Vision.SetVisionCell(VisionCells);
                    player.isCanVision = true;
                    player.Vision.Update();
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
				moveMapPlayer.Remove(player);
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
				if (_monsters.Count <= 3 && RoomId == 1) SpawnMob();
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
                if (Math.Abs(dx) > VisionCells)
                    continue;
                if (Math.Abs(dz) > VisionCells)
                    continue;
                p.Session.Send(packet);
            }
        }
		public void BroadcastMaster(IMessage packet)
		{
			MasterPlayer?.Session.Send(packet);
		}
		public List<Player> GetAdjacentPlayers(Positions pos) 
		{
            List<Zone> zones = GetAdjacentZones(pos);
            return zones.SelectMany(z => z.Players).ToList();
        }
        public List<Zone> GetAdjacentZones(Positions pos)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxZ = (int)(pos.PosZ + VisionCells);
            int minZ = (int)(pos.PosZ - VisionCells);
            int maxX = (int)(pos.PosX + VisionCells);
            int minX = (int)(pos.PosX - VisionCells);

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
		public void ChangeTheRoom(int roomId, Player player, string mapName)
		{
			GameRoom room = GameLogic.Instance.Find(roomId);
			if (room == null) return;
			if (player == null) return;

			int objectId = player.Id;
			_players.Remove(objectId);
            List<Monster> monsters = _monsters.Values.ToList();
            foreach (var monster in monsters)
            {
                if (monster.Target != null && monster.Target == player)
                    monster.Target = null;
            }
            player.curZone.Remove(player);
            player.Room = null;

            S_Despawn despawnPacket = new S_Despawn();
            despawnPacket.ObjectIds.Add(objectId);
            Broadcast(player.Pos, despawnPacket);

			player.Room = room;

            room.cutSceneCount = 0;
            if (player != null && room.RoomId == 2)
                room.moveMapPlayer.Add(player);
			player.IsMaster = false;
            // 맵 이동 패킷 보내기
            S_ChangeMap changeMap = new S_ChangeMap();
			changeMap.MapName = mapName;
			player.Session.Send(changeMap);
        }
		public void HandleExpedition(Player player, C_Expedition expeditionPacket)
		{
			if(GameLogic.Instance.Find(expeditionPacket.RoomId).isRaid)
			{
                S_Message message = new S_Message();
                message.Message = $"누군가 레드 드래곤 토벌 중 입니다.";
				player.Session.Send(message);
				return;
            }
			if(moveMapPlayer.Count == 0)
			{
				S_Message message = new S_Message();
				message.Message = $"누군가 레드 드래곤 원정대를 생성했습니다.\n 5분뒤 원정대가 출발합니다.";
				foreach (Player players in _players.Values)
				{
					players.Session.Send(message);
				}
				PushAfter(3000, AllPlayerEnterNextMap, expeditionPacket.RoomId, "Boss");
			}
			else
			{
				if (moveMapPlayer.Find(p => p.Info.Name == player.Info.Name) != null)
				{
                    S_Message messages = new S_Message();
                    messages.Message = "이미 원정대에 가입했습니다.";
                    player.Session.Send(messages);
					return;
                }
                S_Message message = new S_Message();
                message.Message = "누군가 레드 드래곤 원정대에 참여했습니다.";
                foreach (Player players in _players.Values)
                {
                    players.Session.Send(message);
                }
            }
            moveMapPlayer.Add(player);
        }
		int cutSceneCount = 0;
        public void HandleEndCutScene(Player players, C_EndCutScene endCutScene)
		{
			cutSceneCount++;
			if(cutSceneCount == moveMapPlayer.Count)
			{
                S_SetMasterClient setMasterClient = new S_SetMasterClient();

                if (moveMapPlayer[0] != null)
				{
					moveMapPlayer[0].IsMaster = true;
					setMasterClient.ObjectId = moveMapPlayer[0].Id;
				}
                foreach (Player player in moveMapPlayer)
                {
                    // 위치 조절
                    player.Pos.PosX = 90f;
                    player.Pos.PosZ = 40f;
                    player.Pos.PosY = 0.5f;
                    EnterGame(player);
					player.Session.Send(setMasterClient);
                }
                // 드래곤 소환
                if (moveMapPlayer[0] != null)
				{
                    RedDragon redDragon = ObjectManager.Instance.Add<RedDragon>();
                    redDragon.Init(2, moveMapPlayer[0], _players);
                    EnterGame(redDragon);
                }
                GameRoom room = GameLogic.Instance.Find(1);
				if (room != null) { room.moveMapPlayer.Clear(); }
				moveMapPlayer.Clear();
            }
		}
		public void AllPlayerEnterNextMap(int roomId, string mapName)
		{
			foreach (var player in moveMapPlayer)
			{
				ChangeTheRoom(roomId, player, mapName);
			}
			if(roomId == 2)
			{
				GameLogic.Instance.Find(roomId).isRaid = true;
            }
			moveMapPlayer.Clear();
		}
		public void ChangeMaster(Player player)
		{
			player.IsMaster = false;
			if(_players.Count == 0)
			{
				foreach (var monster in _monsters.Values)
				{
					LeaveGame(monster.Id);
					
				}
				isRaid = false;
                cutSceneCount = 0;
                endBossCutSceneCnt = 0;
                return;
			}
			foreach (var player2 in _players.Values)
			{
				if(player2.Id != player.Id)
				{
                    foreach (var monster in _monsters.Values)
						(monster as RedDragon).Master = player2;
                    S_SetMasterClient setMasterClient = new S_SetMasterClient();
					setMasterClient.ObjectId = player2.Id;
                    Broadcast(player2.Pos,setMasterClient);
                    player2.IsMaster = true;
					break;
				}
			}
		}
		int endBossCutSceneCnt = 0;
		public void HandleBossItemCutScene(Player player)
		{
            if (!player.IsMaster) return;
			S_BossItemCutScene cutScene = new S_BossItemCutScene();
			Broadcast(player.Pos, cutScene);
            endBossCutSceneCnt = 0;

        }
        public void HandleEndBossItemCutScene(Player player)
        {
            endBossCutSceneCnt++;
			if(endBossCutSceneCnt == _players.Count)
			{
				List<Player> move = _players.Values.ToList();
				foreach (Player itemPlayer in _players.Values)
				{
					if (itemPlayer == null) continue;
					C_AddItem addItem = new C_AddItem();
					addItem.Count = 1;
					addItem.TemplateId = 4;
					addItem.IsBuy = false;
					HandleAddItem(itemPlayer, addItem);
				}
				foreach (Player itemPlayer in move)
				{
					itemPlayer.isCanVision = false;
					ChangeTheRoom(1, itemPlayer, "Game");
				}
                foreach (Player itemPlayer in move)
                {
					Positions positions = Utils.Vector3ToPositions(new Vector3(340, 7.5f, 340));
					itemPlayer.Pos = positions;
                    PushAfter(3500, SpawnPlayer, 1, itemPlayer);
                }
            }
        }
		public void SpawnPlayer(int roomId, Player player)
		{
			GameRoom findRoom = GameLogic.Instance.Find(roomId);
			if (findRoom == null) return;
			if(findRoom._players.ContainsKey(player.Id)) findRoom._players.Remove(player.Id);
			findRoom.EnterGame(player);
        }
        public void RemovePlayerForMonster(Player player)
		{
			foreach (var monster in _monsters.Values)
			{
				RedDragon redDragon = (RedDragon)monster;
				if (redDragon == null) continue;
				var targetList = redDragon.priorityTarget.ToList();
				foreach (var target  in targetList)
					if(target.target.Id == player.Id)
						redDragon.priorityTarget.Remove(target);
				redDragon.players.Remove(player.Id);
			}
		}
		public void HandleLeaveGame(Player player, C_RequestLeaveGame leaveGamePacket)
		{
			if(player != null)
				LeaveGame(leaveGamePacket.ObjectId);
		}
    }
}
