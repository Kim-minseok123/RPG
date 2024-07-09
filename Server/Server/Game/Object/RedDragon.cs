using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class RedDragon : Monster
    {
        public void Init()
        {
            Pos.PosX = -37f;
            Pos.PosZ = -5f;
            Pos.PosY = 0f;
            PosInfo.Rotate.RotateY = 180;
        }
    }
}
