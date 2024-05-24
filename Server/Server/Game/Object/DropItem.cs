using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class DropItem : GameObject
    {
        public Data.RewardData _rewardData { get; set; }
        public GameObject Owner { get; set; }
        public DropItem()
        {
            ObjectType = GameObjectType.Dropitem;
        }
        public override void Update()
        {
            Room.PushAfter(120000, LostOwner);
            Room.PushAfter(600000, LostOwner);
        }
        public override GameObject GetOwner()
        {
            return Owner;
        }
        public void LostOwner()
        {
            Owner = null;
        }
        public void DisappearItem()
        {
            Room.LeaveGame(Id);
        }
    }
}
