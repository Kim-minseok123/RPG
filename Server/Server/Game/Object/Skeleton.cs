using Google.Protobuf.Protocol;
using Server.Data;
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
        
        protected override void UpdateSkill()
        {
            if (_skillTick + 1000 > Environment.TickCount64)
                return;
            if (_coolTick == 0)
            {
                if (isCanAttack)
                {
                    Console.WriteLine("공격 중에 있음");
                    Skill skillData = null;
                    DataManager.SkillDict.TryGetValue(4, out skillData);
                    Vector3 attacker = Utils.PositionsToVector3(Pos);
                    Vector3 target = Utils.PositionsToVector3(Target.Pos);
                    if(Room.IsObjectInRange(attacker, target, forwardMonster, skillData.skillDatas[0].range))
                    {
                        Target.OnDamaged(this, skillData.skillDatas[0].damage + TotalAttack);
                    }
                    int coolTick = (int)(1000 * skillData.cooldown);
                    _coolTick = Environment.TickCount64 + coolTick;
                    isCanAttack = false;
                    isMotion = false;
                }
                else
                {
                    if (_MoveTick > Environment.TickCount64)
                        return;
                    _MoveTick = Environment.TickCount64 + 1500;
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
    }
}
