using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Server.Game
{
    public class Skeleton : Monster
    {
        public override void Init(int templateId)
        {
            base.Init(templateId);
            SpawnPos = new Vector3(340f, 7.5f, 340f);

            PosInfo.Pos.PosX = SpawnPos.x;
            PosInfo.Pos.PosY = SpawnPos.y;
            PosInfo.Pos.PosZ = SpawnPos.z;
        }
        
        protected override void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                if(Target != null && Target.State == CreatureState.Dead)
                {
                    State = CreatureState.Idle;
                    Target = null;
                    isMoving = false;
                    Console.WriteLine("목표 사망");
                    return;
                }
                if (isCanAttack)
                {
                    Console.WriteLine("공격 중에 있음");
                    Skill skillData = null;
                    DataManager.SkillDict.TryGetValue(4, out skillData);
                    Vector3 attacker = Utils.PositionsToVector3(Pos);
                    Vector3 target = Utils.PositionsToVector3(Target.Pos);
                    if(Room != null && Target != null && Room.IsObjectInRange(attacker, target, forwardMonster, skillData.skillDatas[0].range))
                    {
                        if (Room != null && State == CreatureState.Skill)
                        {
                            Target.OnDamaged(this, skillData.skillDatas[0].damage + TotalAttack);
                        }
                    }
                    int coolTick = (int)(1000 * skillData.cooldown);
                    _coolTick = Environment.TickCount64 + coolTick;
                    isCanAttack = false;
                    isMotion = false;
                }
                else
                {
                    if (isMotion) return;
                    if (Target == null || Target.Room != Room)
                    {
                        Target = null;
                        State = CreatureState.Idle;
                        return;
                    }
                    nextPos = Target.Pos;
                    BroadcastMove();
                }
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }
        public override void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;
            State = CreatureState.Dead;
            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(diePacket);

            ItemDrop(attacker);

            Room.PushAfter(5000, DieEvent);
        }
    }
}
