using Google.Protobuf.Protocol;
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
            hitBox = new HitBox(10, 6, 12);
        }
        protected override void UpdateIdle()
        {

        }
        public override void OnDamaged(GameObject attacker, int damage)
        {
            if (State == CreatureState.Dead) return;
            if (Room == null)
                return;

            damage = Math.Max(damage - Stat.Defense, 0);
            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            changePacket.ChangeHp = damage;
            changePacket.IsHeal = false;
            Room.Broadcast(Pos, changePacket);
            //Room.Broadcast(CellPos, changePacket);
            Console.WriteLine(attacker + "에 의한 HP 감소");

            if (Stat.Hp <= 0)
            {
                State = CreatureState.Dead;
                OnDead(attacker);
            }
            else
            {
                Console.WriteLine("맞음");
            }
        }
    }
}
