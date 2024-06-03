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

		public Zone[,] Zones { get; private set; }
		public int ZoneCells { get; private set; }
		public int PlayerCount { get { return _players.Count; } }
		public Map Map { get; private set; } = new Map();

		// ㅁㅁㅁ
		// ㅁㅁㅁ
		// ㅁㅁㅁ
		public Zone GetZone(Vector2Int cellPos)
		{
			int x = (cellPos.x - Map.MinX) / ZoneCells;
			int y = (Map.MaxY - cellPos.y) / ZoneCells;
			return GetZone(y, x);
		}

		public Zone GetZone(int indexY, int indexX)
		{
			if (indexX < 0 || indexX >= Zones.GetLength(1))
				return null;
			if (indexY < 0 || indexY >= Zones.GetLength(0))
				return null;

			return Zones[indexY, indexX];
		}

		public void Init(int mapId, int zoneCells)
		{
			//Map.LoadMap(mapId);

			// Zone
			ZoneCells = zoneCells; // 10
			// 1~10 칸 = 1존
			// 11~20칸 = 2존
			// 21~30칸 = 3존
			int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
			int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
			Zones = new Zone[countY, countX];
			for (int y = 0; y < countY; y++)
			{
				for (int x = 0; x < countX; x++)
				{
					Zones[y, x] = new Zone(y, x);
				}
			}

			
			for (int i = 0; i < 1; i++)
			{
				SpawnMob();

            }
		}
		public void SpawnMob()
		{
            Skeleton skeleton = ObjectManager.Instance.Add<Skeleton>();
            skeleton.Init(1);
            EnterGame(skeleton);
        }
		// 누군가 주기적으로 호출해줘야 한다
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
					S_EquipItemList equipItemList = new S_EquipItemList();
					equipItemList.ObjectId = player.Id;
					for (int i = 0; i < player.Inven.EquipItems.Length; i++)
					{
						if (player.Inven.EquipItems[i] != null)
							equipItemList.TemplateIds.Add(player.Inven.EquipItems[i].TemplateId);
					}
					player.Session.Send(equipItemList);

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
                            player.Session.Send(equipItemList);
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

				//player.OnLeaveGame();
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

		// 살짝 부담스러운 함수
		public Player FindClosestPlayer(Vector2Int pos, int range)
		{
			List<Player> players = GetAdjacentPlayers(pos, range);

			players.Sort((left, right) =>
			{
				int leftDist = (left.CellPos - pos).cellDistFromZero;
				int rightDist = (right.CellPos - pos).cellDistFromZero;
				return leftDist - rightDist;
			});

			foreach (Player player in players)
			{
				List<Vector2Int> path = Map.FindPath(pos, player.CellPos, checkObjects: true);
				if (path.Count < 2 || path.Count > range)
					continue;

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
		public void Broadcast(Vector2Int pos, IMessage packet)
		{
			List<Zone> zones = GetAdjacentZones(pos);

			foreach (Player p in zones.SelectMany(z => z.Players))
			{
				int dx = p.CellPos.x - pos.x;
				int dy = p.CellPos.y - pos.y;
				if (Math.Abs(dx) > GameRoom.VisionCells)
					continue;
				if (Math.Abs(dy) > GameRoom.VisionCells)
					continue;

				p.Session.Send(packet);
			}
		}

		public List<Player> GetAdjacentPlayers(Vector2Int pos, int range)
		{
			List<Zone> zones = GetAdjacentZones(pos, range);
			return zones.SelectMany(z => z.Players).ToList();
		}

		// ㅁㅁㅁㅁㅁㅁ
		// ㅁㅁㅁㅁㅁㅁ
		// ㅁㅁㅁㅁㅁㅁ
		// ㅁㅁㅁㅁㅁㅁ
		public List<Zone> GetAdjacentZones(Vector2Int cellPos, int range = GameRoom.VisionCells)
		{
			HashSet<Zone> zones = new HashSet<Zone>();

			int maxY = cellPos.y + range;
			int minY = cellPos.y - range;
			int maxX = cellPos.x + range;
			int minX = cellPos.x - range;

			// 좌측 상단
			Vector2Int leftTop = new Vector2Int(minX, maxY);
			int minIndexY = (Map.MaxY - leftTop.y) / ZoneCells;
			int minIndexX = (leftTop.x - Map.MinX) / ZoneCells;
			
			// 우측 하단
			Vector2Int rightBot = new Vector2Int(maxX, minY);
			int maxIndexY = (Map.MaxY - rightBot.y) / ZoneCells;
			int maxIndexX = (rightBot.x - Map.MinX) / ZoneCells;

			for (int x = minIndexX; x <= maxIndexX; x++)
			{
				for (int y = minIndexY; y <= maxIndexY; y++)
				{
					Zone zone = GetZone(y, x);
					if (zone == null)
						continue;

					zones.Add(zone);
				}
			}

			return zones.ToList();
		}
	}
}
