using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class QuestNpc : Npc
    {
        public List<int> QuestList { get; private set; } = new List<int>();
        public QuestNpc()
        {
            ObjectType = GameObjectType.Npc;
        }
        public override void Init(int templateId)
        {
            if (DataManager.NpcDict.TryGetValue(templateId, out npcData) == false) return;
            Info.Name = npcData.name;
            QuestList = npcData.npcQuestLists;
            ObjectType = GameObjectType.Npc;
            if (npcData.id == 3)
            {
                SpawnPos = new Vector3(378.89f, 6.19f, 338.43f);
                PosInfo.Rotate = new RotateInfo() { RotateX = 0, RotateY = 90, RotateZ = 0 };
            }
            else if (npcData.id == 4)
            {
                SpawnPos = new Vector3(346.77f, 6.19f, 332.12f);
                PosInfo.Rotate = new RotateInfo() { RotateX = 0, RotateY = 0, RotateZ = 0 };
            }
            else if (npcData.id == 5)
            {
                SpawnPos = new Vector3(358.27f, 6.19f, 348.68f);
                PosInfo.Rotate = new RotateInfo() { RotateX = 0, RotateY = 180, RotateZ = 0 };
            }
            PosInfo.Pos.PosX = SpawnPos.x;
            PosInfo.Pos.PosY = SpawnPos.y;
            PosInfo.Pos.PosZ = SpawnPos.z;
        }
    }
}
