using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Skeleton : Monster
    {
        public override void Init(int templateId)
        {
            base.Init(templateId);
            PosInfo.Pos.PosX = 340f;
            PosInfo.Pos.PosY = 7.5f;
            PosInfo.Pos.PosZ = 340f;
        }
    }
}
