using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class GameLogic : JobSerializer
	{
		public static GameLogic Instance { get; } = new GameLogic();

		Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
		int _roomId = 1;

		public void Update()
		{
			Flush();

			foreach (GameRoom room in _rooms.Values)
			{
				room.Update();
			}
		}

		public GameRoom Add(int mapId, int ZoneCell, int minX, int minY, int maxX, int maxY, int visionCell)
		{
			GameRoom gameRoom = new GameRoom();
			gameRoom.Push(gameRoom.Init, mapId, ZoneCell, minX, minY, maxX, maxY , visionCell);

			gameRoom.RoomId = _roomId;
			_rooms.Add(_roomId, gameRoom);
			_roomId++;

			return gameRoom;
		}

		public bool Remove(int roomId)
		{
			return _rooms.Remove(roomId);
		}

		public GameRoom Find(int roomId)
		{
			GameRoom room = null;
			if (_rooms.TryGetValue(roomId, out room))
				return room;

			return null;
		}
	}
}
