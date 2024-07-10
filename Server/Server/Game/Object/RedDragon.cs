using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class RedDragon : Monster
    {
        public override void Init(int templateId)
        {
            base.Init(templateId);
            Pos.PosX = 89;
            Pos.PosZ = 58f;
            Pos.PosY = 0f;
            PosInfo.Rotate.RotateY = 180;
        }
        protected override void UpdateIdle()
        {

        }
    }
}
