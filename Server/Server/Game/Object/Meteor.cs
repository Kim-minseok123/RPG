using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Meteor : GameObject
    {
        RedDragon parent;
        AreaPos area;
        Dictionary<int, Player> players = new Dictionary<int, Player> ();
        public Meteor(Positions position, RedDragon dragon, AreaPos areaPos, Dictionary<int, Player> players)
        {
            Pos = position;
            parent = dragon;
            this.area = areaPos;
            this.players = players;
        }
        public override void Update()
        {
            foreach (var player in this.players.Values)
            {
                parent?.AttackArea(area, player, 20);
            }
        }
    }
}
