using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class Utils
    {
        public static Vector3 PositionsToVector3(Positions positions)
        {
            return new Vector3(positions.PosX, positions.PosY, positions.PosZ);
        }
        public static Positions Vector3ToPositions(Vector3 vector)
        {
            return new Positions() { PosX = (float)vector.x, PosY = (float)vector.y, PosZ = (float)vector.z };
        }
    }
}
