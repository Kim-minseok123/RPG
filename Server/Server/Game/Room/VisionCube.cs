using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game.Room
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        public int VisionCell = 5;
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();
        public VisionCube(Player owner)
        {
            Owner = owner;
        }
        public void SetVisionCell(int visionCell)
        {
            this.VisionCell = visionCell;
        }
        public HashSet<GameObject> GatherObjects()
        {
            if (Owner == null || Owner.Room == null)
                return null;

            HashSet<GameObject> objects = new HashSet<GameObject>();

            List<Zone> zones = Owner.Room.GetAdjacentZones(Owner.Pos, Owner.Session.Master);

            Positions pos = Owner.Pos;
            foreach (Zone zone in zones)
            {
                foreach (Player player in zone.Players)
                {
                    float dx = pos.PosX - player.Pos.PosX;
                    float dz = pos.PosZ - player.Pos.PosZ;
                    if (Math.Abs(dx) > VisionCell)
                        continue;
                    if (Math.Abs(dz) > VisionCell)
                        continue;
                    objects.Add(player);
                }
                foreach (Monster monster in zone.Monsters)
                {
                    float dx = pos.PosX - monster.Pos.PosX;
                    float dz = pos.PosZ - monster.Pos.PosZ;
                    if (Math.Abs(dx) > VisionCell)
                        continue;
                    if (Math.Abs(dz) > VisionCell)
                        continue;
                    objects.Add(monster);
                }
                foreach (DropItem dropItem in zone.DropItems)
                {
                    float dx = pos.PosX - dropItem.Pos.PosX;
                    float dz = pos.PosZ - dropItem.Pos.PosZ;
                    if (Math.Abs(dx) > VisionCell)
                        continue;
                    if (Math.Abs(dz) > VisionCell)
                        continue;
                    objects.Add(dropItem);
                }
                foreach (Npc npc in zone.Npcs)
                {
                    float dx = pos.PosX - npc.Pos.PosX;
                    float dz = pos.PosZ - npc.Pos.PosZ;
                    if (Math.Abs(dx) > VisionCell)
                        continue;
                    if (Math.Abs(dz) > VisionCell)
                        continue;
                    objects.Add(npc);
                }
            }
            return objects;
        }
        public void Update()
        {
            if (Owner == null || Owner.Room == null)
                return;
            if (Owner.isCanVision)
            {
                HashSet<GameObject> currentObjects = GatherObjects();

                List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();

                if (added.Count > 0)
                {
                    S_Spawn spawnPacket = new S_Spawn();

                    foreach (GameObject gameObject in added)
                    {
                        if (gameObject.ObjectType == GameObjectType.Player)
                        {
                            Player player = (Player)gameObject;
                            if (player.Session.Master == true) continue;
                        }
                        ObjectInfo info = new ObjectInfo();
                        info.MergeFrom(gameObject.Info);
                        spawnPacket.Objects.Add(info);
                    }
                    Owner.Session.Send(spawnPacket);
                    foreach (GameObject gameObject in added)
                    {
                        if (gameObject.ObjectType == GameObjectType.Player)
                        {
                            Player player = (Player)gameObject;
                            if (player.Session.Master == true) continue;
                            S_EquipItemList equipItemLists = new S_EquipItemList();
                            equipItemLists.ObjectId = player.Id;
                            for (int i = 0; i < player.Inven.EquipItems.Length; i++)
                            {
                                if (player.Inven.EquipItems[i] != null)
                                    equipItemLists.TemplateIds.Add(player.Inven.EquipItems[i].TemplateId);
                            }
                            Owner.Session.Send(equipItemLists);
                        }
                        if (gameObject.isMoving)
                        {
                            S_Move resMovePacket = new S_Move();
                            resMovePacket.ObjectId = gameObject.Info.ObjectId;
                            resMovePacket.DestPosInfo = new PositionInfo();
                            resMovePacket.DestPosInfo.Pos = gameObject.DestPos;
                            if (gameObject.DestPos != null)
                                Owner.Session.Send(resMovePacket);
                        }
                    }
                }
                List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
                if (removed.Count > 0)
                {
                    S_Despawn despawnPacket = new S_Despawn();

                    foreach (GameObject gameObject in removed)
                    {
                        despawnPacket.ObjectIds.Add(gameObject.Id);
                    }
                    Owner.Session.Send(despawnPacket);
                }

                PreviousObjects = currentObjects;
            }
           
            Owner.Room.PushAfter(100, Update);
        }
    }
}
