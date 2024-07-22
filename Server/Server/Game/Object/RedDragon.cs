using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
    public class TargetInfo
    {
        public GameObject target;
        public int totalDamage = 0;
        public override bool Equals(object obj)
        {
            if (obj is TargetInfo other)
            {
                return target == other.target;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return target.GetHashCode();
        }
    }
    public class DamageCompare : IComparer<TargetInfo>
    {
        // 문자열의 길이에 따라 오름차순
        public int Compare(TargetInfo x, TargetInfo y)
        {
            return y.totalDamage - x.totalDamage;
        }
    }
    public class RedDragon : Monster
    {
        public SortedSet<TargetInfo> priorityTarget = new SortedSet<TargetInfo>(new DamageCompare());
        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        bool isFirstDamage = true;
        List<Area> attackArea = new List<Area>();
        Area lastArea = null;
        IJob job = null;
        bool isflying = false;
        bool isDefend= false;
        public Player Master { get; set; }
        public int currentSkill = -1;
        public void Init(int templateId, Player master, Dictionary<int, Player> players)
        {
            base.Init(templateId);
            Pos.PosX = 89;
            Pos.PosZ = 58f;
            Pos.PosY = 0f;
            PosInfo.Rotate.RotateY = 180;
            hitBox = new HitBox(12, 10, 14);
            ClawArea clawArea = new ClawArea();
            MagicArea magicArea = new MagicArea();
            FlameArea flameArea = new FlameArea();
            attackArea.Add(clawArea);
            attackArea.Add(magicArea);
            attackArea.Add(flameArea);
            Master = master;
            this.players = players;
        }
        public override void Update()
        {

        }
        protected override void UpdateIdle()
        {

        }
        protected override void UpdateSkill()
        {

        }
        public override void OnDamaged(GameObject attacker, int damage)
        {
            if (State == CreatureState.Dead) return;
            if (Room == null)
                return;
            if (isflying || isDefend) return;
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
                return;
            }
            TargetInfo attackerInfo = new TargetInfo { target = attacker, totalDamage = damage };
            if (!priorityTarget.Contains(attackerInfo))
            {
                priorityTarget.Add(attackerInfo);
            }
            else
            {
                foreach (var targetInfo in priorityTarget)
                {
                    if (targetInfo.target == attacker)
                    {
                        targetInfo.totalDamage += damage;
                        break;
                    }
                }
            }
            if (isFirstDamage)
            {
                isFirstDamage = false;

                S_SkillMotion skillMotion = new S_SkillMotion() { Info = new SkillInfo() };
                skillMotion.ObjectId = Id;
                skillMotion.Info.SkillId = 0;
                Room.Broadcast(Pos, skillMotion);
                job = Room.PushAfter(5000, SelectSkillAndAttack);
            }
        }
        public void SelectSkillAndAttack()
        {
            if (priorityTarget.Count <= 0)
                return;
            isflying = false; isDefend = false;
            currentSkill  = -1;
            lastArea = null;
            GameObject targetObject = priorityTarget.First().target;
            int attackNum = -1;
            foreach (var area in attackArea)
            {
                attackNum = area.CheckObjectInArea(targetObject.Pos);
                lastArea = area;
                if (attackNum != -1)
                    break;
            }
            S_SkillMotion skillMotion = new S_SkillMotion() { Info = new SkillInfo() };
            Random random = new Random();
            int defend = random.Next(0,10);
            if (defend == 0) attackNum = 5;
            switch (attackNum)
            {
                case -1:
                    StartMeteor(0, targetObject);
                    break;
                case 1:
                    skillMotion.ObjectId = Id;
                    skillMotion.Info.SkillId = 1;
                    currentSkill = 1;
                    break;
                case 2:
                    skillMotion.ObjectId = Id;
                    skillMotion.Info.SkillId = 2;
                    currentSkill = 2;
                    break;
                case 3:
                    int rand = random.Next(0, 3);
                    currentSkill = 3;
                    if(rand == 2)
                    {
                        skillMotion.ObjectId = Id;
                        skillMotion.Info.SkillId = 4;
                        isflying = true;
                    }
                    else
                    {
                        skillMotion.ObjectId = Id;
                        skillMotion.Info.SkillId = 3;
                    }
                    break;
                case 5:
                    skillMotion.ObjectId = Id;
                    skillMotion.Info.SkillId = 5;
                    currentSkill = 5;
                    isDefend = true;
                    Room.PushAfter(4000, Normalization);
                    job = Room.PushAfter(7000, SelectSkillAndAttack);
                    break;
            }
            Room.Broadcast(Pos, skillMotion);
        }
        public void Normalization()
        {
            isDefend = false;
            isflying = false;
        }
        public void AttackArea(AreaPos area, GameObject target, int damage)
        {
            Vector3 pos = Utils.PositionsToVector3(target.Pos);
            if(Utils.InRangeObject(area.areaMin,area.areaMax, pos))
            {
                target.OnDamaged(this, damage);
            }
        }
        public void AttackAction(int time, bool isEnd)
        {
            switch (currentSkill)
            {
                case -1:
                    break;
                case 1:
                    foreach (var player in players.Values)
                    {
                        AttackArea(lastArea.areas[0], player, 30);
                        AttackArea(lastArea.areas[1], player, 30);
                    }
                    if (isEnd)
                        job = Room?.PushAfter(5000, SelectSkillAndAttack);
                    break;
                case 2:
                    foreach (var player in players.Values)
                    {
                        AttackArea(lastArea.areas[0], player, 50);
                    }
                    if (isEnd)
                        job = Room?.PushAfter(4000, SelectSkillAndAttack);
                    break;
                case 3:
                    foreach (var player in players.Values)
                    {
                        if(time == 1)
                            AttackArea(lastArea.areas[0], player, 20);
                        else if (time == 2)
                            AttackArea(lastArea.areas[1], player, 20);
                        else if (time == 3)
                            AttackArea(lastArea.areas[2], player, 20);
                    }
                    if (isEnd && isflying == false)
                        job = Room?.PushAfter(5000, SelectSkillAndAttack);
                    if (isEnd && isflying == true)
                        job = Room?.PushAfter(7000, SelectSkillAndAttack);
                    break; 
            }
        }
        IJob meteorJob = null;
        public void StartMeteor(int cnt, GameObject targetObject)
        {
            if (cnt == 4)
            {
                job = Room?.PushAfter(5000, SelectSkillAndAttack);
                return;
            }
            cnt++;
            // 플레이어에게 메테오
            Positions positions = new Positions();
            positions.PosX = targetObject.Pos.PosX;
            positions.PosY = 0;
            positions.PosZ = targetObject.Pos.PosZ;
            MakeMeteor(targetObject, positions);
            MakeMeteor();

            meteorJob =Room?.PushAfter(5000, StartMeteor, cnt, targetObject);
        }
        public void MakeMeteor(GameObject targetObject = null, Positions positions = null)
        {
            if(positions == null && targetObject == null)
            {
                int x = random.Next(77, 106);
                int z = random.Next(32, 65);
                positions = new Positions();
                positions.PosX = x;
                positions.PosY = 0;
                positions.PosZ = z;
                AreaPos area = new AreaPos
                {
                    areaMin = Utils.PositionsToVector3(positions) - new Vector3(4, 0, 4),
                    areaMax = Utils.PositionsToVector3(positions) + new Vector3(4, 0, 4)
                };
                Meteor meteor = new Meteor(positions, this, area, players);
                S_MakeMeteorObject makeMeteorObject = new S_MakeMeteorObject
                {
                    ObjectId = Id,
                    Pos = positions
                };
                if (Room == null) return;
                Room?.Broadcast(Pos, makeMeteorObject);
                Room?.PushAfter(2500, meteor.Update);
            }
            else if(targetObject != null && positions != null)
            {
                AreaPos area = new AreaPos
                {
                    areaMin = Utils.PositionsToVector3(positions) - new Vector3(4, 0, 4),
                    areaMax = Utils.PositionsToVector3(positions) + new Vector3(4, 0, 4)
                };
                Meteor meteor = new Meteor(positions, this, area, players);
                S_MakeMeteorObject makeMeteorObject = new S_MakeMeteorObject
                {
                    ObjectId = Id,
                    Pos = positions
                };
                if (Room == null) return;
                Room?.Broadcast(Pos, makeMeteorObject);
                Room?.PushAfter(2500, meteor.Update);
            }
        }
        public override void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;
            if (job != null)
            {
                job.Cancel = true;
                job = null;
            }
            if(meteorJob != null)
            {
                meteorJob.Cancel = true;
                meteorJob = null;
            }
            State = CreatureState.Dead;
            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room?.Broadcast(Pos, diePacket);

            S_Message message = new S_Message();
            message.Message = "레드 드래곤을 토벌하였습니다.\n 공대장은 위의 상자에서 보상을 획득하세요.";
            Room?.Broadcast(Pos, diePacket);

            Room?.PushAfter(5000, DieEvent);
        }
    }
    public struct AreaPos
    {
        public Vector3 areaMin;
        public Vector3 areaMax;
    }
    public abstract class Area
    {
        public abstract int CheckObjectInArea(Positions pos);
        public List<AreaPos> areas = new List<AreaPos>();
    }
    public class ClawArea : Area
    {
        public AreaPos areaPos = new AreaPos();
        public ClawArea()
        {
            areaPos.areaMin = new Vector3(85, 0, 44);
            areaPos.areaMax = new Vector3(93, 0, 60);
            areas.Add(areaPos);
        }
        public override int CheckObjectInArea(Positions pos)
        {
            Vector3 objectPos = Utils.PositionsToVector3(pos);
            if (Utils.InRangeObject(areaPos.areaMin, areaPos.areaMax, objectPos))
                return 2;
            return -1;
        }
    }
    public class MagicArea : Area
    {
        public AreaPos areaPos1 = new AreaPos();
        public AreaPos areaPos2 = new AreaPos();
        public MagicArea()
        {
            areaPos1.areaMin = new Vector3(77, 0, 44);
            areaPos1.areaMax = new Vector3(85, 0, 52);
            areaPos2.areaMin = new Vector3(93, 0, 44);
            areaPos2.areaMax = new Vector3(101, 0, 52);
            areas.Add(areaPos1);
            areas.Add(areaPos2);
        }
        public override int CheckObjectInArea(Positions pos)
        {
            Vector3 objectPos = Utils.PositionsToVector3(pos);
            if (Utils.InRangeObject(areaPos1.areaMin, areaPos1.areaMax, objectPos) || Utils.InRangeObject(areaPos2.areaMin, areaPos2.areaMax, objectPos))
                return 1;
            return -1;
        }
    }
    public class FlameArea : Area
    {
        public AreaPos areaPos1 = new AreaPos();
        public AreaPos areaPos2 = new AreaPos();
        public AreaPos areaPos3 = new AreaPos();

        public FlameArea()
        {
            areaPos1.areaMin = new Vector3(77, 0, 36);
            areaPos1.areaMax = new Vector3(85, 0, 44);
            areaPos2.areaMin = new Vector3(85, 0, 36);
            areaPos2.areaMax = new Vector3(93, 0, 44);
            areaPos3.areaMin = new Vector3(93, 0, 36);
            areaPos3.areaMax = new Vector3(101, 0, 44);
            areas.Add(areaPos1);
            areas.Add(areaPos2);
            areas.Add(areaPos3);
        }
        public override int CheckObjectInArea(Positions pos)
        {
            Vector3 objectPos = Utils.PositionsToVector3(pos);
            if (Utils.InRangeObject(areaPos1.areaMin, areaPos1.areaMax, objectPos)
                || Utils.InRangeObject(areaPos2.areaMin, areaPos2.areaMax, objectPos)
                || Utils.InRangeObject(areaPos3.areaMin, areaPos3.areaMax, objectPos))
                return 3;
            return -1;
        }
    }
}
