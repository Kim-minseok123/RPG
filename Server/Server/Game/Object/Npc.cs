using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Npc : GameObject
    {
        public List<NpcSellList> npcSellLists = new List<NpcSellList>();
        protected Vector3 SpawnPos;
        public NpcData npcData;
        public Npc()
        {
            ObjectType = GameObjectType.Npc;
        }
        public void Init(int templateId)
        {
            if (DataManager.NpcDict.TryGetValue(templateId, out npcData) == false) return;
            Info.Name = npcData.name;

            ObjectType = GameObjectType.Npc;
            if(npcData.id == 1)
                SpawnPos = new Vector3(343.72f, 6.19f, 348.31f);
            else if(npcData.id == 2)
                SpawnPos = new Vector3(326.25f, 6.19f, 347.52f);
            PosInfo.Rotate = new RotateInfo() { RotateX = 0, RotateY = 180, RotateZ = 0};
            PosInfo.Pos.PosX = SpawnPos.x;
            PosInfo.Pos.PosY = SpawnPos.y;
            PosInfo.Pos.PosZ = SpawnPos.z;
        }
    }
}
