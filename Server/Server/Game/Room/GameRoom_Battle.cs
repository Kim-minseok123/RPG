using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{
		public void HandleMove(Player player, C_Move movePacket)
		{
			if (player == null)
				return;
            // TODO : 검증
            PositionInfo movePosInfo = movePacket.PosInfo;
			ObjectInfo info = player.Info;

            if (movePacket.PosInfo.Pos.PosX <= 0 || movePacket.PosInfo.Pos.PosX >= 1000 || movePacket.PosInfo.Pos.PosZ <= 0 || movePacket.PosInfo.Pos.PosZ >= 1000)
                return;

			info.PosInfo.State = movePosInfo.State;
			player.DestPos = movePacket.PosInfo.Pos;
            player.isMoving = true;
			S_Move resMovePacket = new S_Move();
			resMovePacket.ObjectId = player.Info.ObjectId;
			resMovePacket.DestPosInfo = movePacket.PosInfo;
			resMovePacket.TargetId = -1;
			Broadcast(player.Pos, resMovePacket);
		}
		public void HandleStopMove(Player player, C_StopMove stopMovePacket)
		{
            if (stopMovePacket.PosInfo.Pos.PosX <= 0 || stopMovePacket.PosInfo.Pos.PosX >= 1000 || stopMovePacket.PosInfo.Pos.PosZ <= 0 || stopMovePacket.PosInfo.Pos.PosZ >= 1000)
            {
				S_Banish banPacket = new S_Banish();
				player.Session.Send(banPacket);
				return;
			}
			player.Info.PosInfo.State = stopMovePacket.PosInfo.State;
			player.Pos = stopMovePacket.PosInfo.Pos;
			player.Ratate = stopMovePacket.PosInfo.Rotate;
			player.DestPos = null;
            player.isMoving = false;
            S_StopMove resStopMovePacket = new S_StopMove();
			resStopMovePacket.ObjectId = player.Info.ObjectId;
			resStopMovePacket.PosOk = true;
			resStopMovePacket.Rotate = stopMovePacket.PosInfo.Rotate;
			resStopMovePacket.Pos = player.Pos;
			Broadcast(player.Pos, resStopMovePacket);
        }
        public void HandleStopMoveMonster(C_StopMove stopMovePacket)
        {
			Monster monster = null;
			_monsters.TryGetValue(stopMovePacket.ObjectId, out monster);
			if (monster == null) return;
            monster.Info.PosInfo.State = stopMovePacket.PosInfo.State;
            monster.Pos = stopMovePacket.PosInfo.Pos;
            monster.Ratate = stopMovePacket.PosInfo.Rotate;
			monster.isMoving = false;
            monster._nextSearchTick = Environment.TickCount64 + 7000;
            monster.DestPos = null;
            S_StopMove resStopMovePacket = new S_StopMove();
            resStopMovePacket.ObjectId = monster.Info.ObjectId;
            resStopMovePacket.PosOk = true;
            resStopMovePacket.Rotate = stopMovePacket.PosInfo.Rotate;
            resStopMovePacket.Pos = monster.Pos;
            Broadcast(monster.Pos, resStopMovePacket);
        }
        public void HandleCheckPos(Player player, C_CheckPos posPacket)
		{
            if (posPacket.CurPosInfo.Pos.PosX <= 0 || posPacket.CurPosInfo.Pos.PosX >= 1000 || posPacket.CurPosInfo.Pos.PosZ <= 0 || posPacket.CurPosInfo.Pos.PosZ >= 1000)
            {
                S_Banish banPacket = new S_Banish();
                player.Session.Send(banPacket);
                return;
            }
			if(posPacket.IsMonster == false)
			{
                Positions playerPos = player.Pos;
                if (playerPos.PosX != posPacket.CurPosInfo.Pos.PosX || playerPos.PosY != posPacket.CurPosInfo.Pos.PosY || playerPos.PosZ != posPacket.CurPosInfo.Pos.PosZ)
                {
                    player.Pos = posPacket.CurPosInfo.Pos;
                    player.Ratate = posPacket.CurPosInfo.Rotate;
                    Zone now = player.curZone;
                    Zone after = GetZone(playerPos);
                    if (after == null)
                        return;
                    if (now == after) return;
                    now.Players.Remove(player);
                    after.Players.Add(player);
                    player.curZone = after;
                }
            }
            else
            {
                Monster monster = null;
                _monsters.TryGetValue(posPacket.ObjectId, out monster);
                if (monster == null) return;
                Positions monsterPos = monster.Pos;
                if (monsterPos.PosX != posPacket.CurPosInfo.Pos.PosX || monsterPos.PosY != posPacket.CurPosInfo.Pos.PosY || monsterPos.PosZ != posPacket.CurPosInfo.Pos.PosZ)
                {
                    monster.Pos = posPacket.CurPosInfo.Pos;
                    monster.Ratate = posPacket.CurPosInfo.Rotate;
                    Zone now = monster.curZone;
                    Zone after = GetZone(monsterPos);
                    if (after == null)
                        return;
                    if (now == after) return;
                    now.Monsters.Remove(monster);
                    after.Monsters.Add(monster);
                    monster.curZone = after;
                }
            }

        }
        public bool IsObjectInRange(Vector3 attacker, Vector3 target, Vector3 forward, SKillRange skill)
        {
            Vector3 up = new Vector3(0, 1, 0); // Up vector assuming Y is up
            Vector3 right = forward.Cross(up).normalized; // Right vector

            Vector3 relativePos = target - attacker;
            float forwardDistance = Vector3.Dot(relativePos, forward);
            float rightDistance = Vector3.Dot(relativePos, right);
            float upwardDistance = Vector3.Dot(relativePos, up);

            if (forwardDistance >= skill.nonDepth && forwardDistance <= skill.depth &&
                Math.Abs(rightDistance) <= skill.width / 2 &&
                Math.Abs(upwardDistance) <= skill.height / 2)
            {
                return true;
            }

            return false;
        }
		public void HandleSkillMotion(Player player, C_SkillMotion skillMotion)
		{
            if (player == null)
                return;
			if(skillMotion.IsMonster == false)
			{
                S_SkillMotion skillMotionServer = new S_SkillMotion() { Info = new SkillInfo() };
                skillMotionServer.ObjectId = player.Id;
                skillMotionServer.Info.SkillId = skillMotion.Info.SkillId;
                Broadcast(player.Pos, skillMotionServer);
            }
            else
			{
                Monster monster = null;
                _monsters.TryGetValue(skillMotion.ObjectId, out monster);
                if (monster == null) return;
                if (monster.isMotion) return;
				monster.isMotion = true;
                Console.WriteLine("공격 모션");
                S_SkillMotion skillMotionServer = new S_SkillMotion() { Info = new SkillInfo() };
                skillMotionServer.ObjectId = monster.Id;
                skillMotionServer.Info.SkillId = skillMotion.Info.SkillId;
                Broadcast(monster.Pos, skillMotionServer);
            }
        }
        public void HandleMeleeAttack(Player player, C_MeleeAttack meleeAttack)
		{
            if (player == null)
                return;
			if (meleeAttack.IsMonster == false)
			{
                if (player.Inven.EquipItems[5] == null) return;
                ObjectInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle)
                    return;
                player.Info.PosInfo.State = CreatureState.Skill;

                Skill skill = null;
                if (DataManager.SkillDict.TryGetValue(meleeAttack.Info.SkillId, out skill) == false) return;
                if(SkillType.SkillMeleeAttack == skill.skillType)
                {
                    if(meleeAttack.Time == 0)
                    {
                        if (player.Stat.Mp - skill.mpConsume < 0) { Console.WriteLine("마나 없음"); return; }
                        player.Stat.Mp = Math.Max(0, player.Stat.Mp - skill.mpConsume);
                        S_ChangeStat changeStat = new S_ChangeStat() { StatInfo = new StatInfo() };
                        changeStat.StatInfo.MergeFrom(player.Stat);
                        player.Session.Send(changeStat);
                    }
                    foreach (Monster monster in _monsters.Values)
                    {
                        Vector3 a = Utils.PositionsToVector3(player.Pos);
                        Vector3 b = Utils.PositionsToVector3(monster.Pos);
                        Vector3 forward = Utils.PositionsToVector3(meleeAttack.Forward);
                        if (IsObjectInRange(a, b, forward, ((AttackSkill)skill).skillDatas[meleeAttack.Time].range) == true)
                        {
                            int level = 0;
                            if (skill.id == 1 || skill.id == 2 || player.HaveSkillData.TryGetValue(skill.id, out level))
                            {
                                int damage =
                                    (((AttackSkill)skill).skillDatas[meleeAttack.Time].damage
                                    + (((AttackSkill)skill).skillDatas[meleeAttack.Time].skillLevelInc * (level - 1)))
                                    / 100
                                    * player.Attack;
                                monster.OnDamaged(player, damage);
                                if (skill.id == 1 || skill.id == 2)
                                    break;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
                
                player.Info.PosInfo.State = CreatureState.Idle;
            }
			else
			{
                Monster monster = null;
                _monsters.TryGetValue(meleeAttack.ObjectId, out monster);
                if (monster == null) return;
                if (monster.State == CreatureState.Dead) return;
                Console.WriteLine("공격 중 브로드캐스팅 +" + meleeAttack.Info.SkillId);
                monster.forwardMonster = Utils.PositionsToVector3(meleeAttack.Forward);
				monster.isCanAttack = true;
            }
        }
        public void StatChange(Player player, C_ChangeStat changeStatPacket)
        {
            if (player == null) return;
            string statName = changeStatPacket.ChangeStat;
            if (player.Stat.StatPoint <= 0) return;

            switch (statName)
            {
                case "Str" :
                    player.Stat.StatPoint--;
                    player.Stat.Str++;
                    break;
                case "Dex":
                    player.Stat.StatPoint--;
                    player.Stat.Dex++;
                    break;
                case "Luk":
                    player.Stat.StatPoint--;
                    player.Stat.Luk++;
                    break;
                case "Int":
                    player.Stat.StatPoint--;
                    player.Stat.Int++;
                    break;
                default:
                    return;
            }
            player.CalAttackValue();

            StatInfo playerStat = player.Stat;

            S_ChangeStat statPacket = new S_ChangeStat();
            statPacket.StatInfo = playerStat;
            player.Session.Send(statPacket);
        }
        public void HandleSkillLevelUp(Player player, C_SkillLevelUp skillLevelUp)
        {
            if (player == null || skillLevelUp.Skill.Level <= 0)
                return;

            player.HandleSkillLevelUp(skillLevelUp);
        }
        public void HandleSaveQuickSlot(Player player, C_SaveQuickSlot saveQuickSlot)
        {
            if (player == null)
                return;
            DbTransaction.SaveQuickSlotNoti(player, saveQuickSlot);
        }
        public void HandleSkillBuff(Player player, C_SkillBuff skillBuffPacket)
        {
            if (player == null)
                return;
            if (DataManager.SkillDict.TryGetValue(skillBuffPacket.SkillId, out Skill skill) == false)
                return;
            if (skill.skillType != SkillType.SkillBuff) return;
            BuffSkill buffSkill = (BuffSkill)skill;
            BuffSkillAbility ability = SkillAbilityFactory.CreateAbility(skill.id);

            player.HandleSkillBuff(buffSkill, ability);
        }
    }
}
