using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
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

			S_Move resMovePacket = new S_Move();
			resMovePacket.ObjectId = player.Info.ObjectId;
			resMovePacket.DestPosInfo = movePacket.PosInfo;
			resMovePacket.TargetId = -1;
			Broadcast(resMovePacket);
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
            S_StopMove resStopMovePacket = new S_StopMove();
			resStopMovePacket.ObjectId = player.Info.ObjectId;
			resStopMovePacket.PosOk = true;
			resStopMovePacket.Rotate = stopMovePacket.PosInfo.Rotate;
			resStopMovePacket.Pos = player.Pos;
			Broadcast(resStopMovePacket);
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
            S_StopMove resStopMovePacket = new S_StopMove();
            resStopMovePacket.ObjectId = monster.Info.ObjectId;
            resStopMovePacket.PosOk = true;
            resStopMovePacket.Rotate = stopMovePacket.PosInfo.Rotate;
            resStopMovePacket.Pos = monster.Pos;
            Broadcast(resStopMovePacket);
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
                PositionInfo playerPos = player.PosInfo;
                if (playerPos.Pos.PosX != posPacket.CurPosInfo.Pos.PosX || playerPos.Pos.PosY != posPacket.CurPosInfo.Pos.PosY || playerPos.Pos.PosZ != posPacket.CurPosInfo.Pos.PosZ)
                {
                    player.Pos = posPacket.CurPosInfo.Pos;
                    player.Ratate = posPacket.CurPosInfo.Rotate;
                }
            }
            else
            {
                Monster monster = null;
                _monsters.TryGetValue(posPacket.ObjectId, out monster);
                if (monster == null) return;
                monster.Pos = posPacket.CurPosInfo.Pos;
                monster.Ratate = posPacket.CurPosInfo.Rotate;
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
                Broadcast(skillMotionServer);
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
                Broadcast(skillMotionServer);
            }
        }
        public void HandleMeleeAttack(Player player, C_MeleeAttack meleeAttack)
		{
            if (player == null)
                return;
			if (meleeAttack.IsMonster == false)
			{
                ObjectInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle)
                    return;
                info.PosInfo.State = CreatureState.Skill;

                Skill skill = null;
                if (DataManager.SkillDict.TryGetValue(meleeAttack.Info.SkillId, out skill) == false) return;

                foreach (Monster monster in _monsters.Values)
                {
                    Vector3 a = Utils.PositionsToVector3(player.Pos);
                    Vector3 b = Utils.PositionsToVector3(monster.Pos);
                    Vector3 forward = Utils.PositionsToVector3(meleeAttack.Forward);
                    if (IsObjectInRange(a, b, forward, skill.skillDatas[meleeAttack.Time].range) == true)
                    {
                        monster.OnDamaged(player, skill.skillDatas[meleeAttack.Time].damage + player.Attack);
                    }
                }

                info.PosInfo.State = CreatureState.Idle;
            }
			else
			{
                Monster monster = null;
                _monsters.TryGetValue(meleeAttack.ObjectId, out monster);
                if (monster == null) return;
                Console.WriteLine("공격 중 브로드캐스팅 +" + meleeAttack.Info.SkillId);
                monster.forwardMonster = Utils.PositionsToVector3(meleeAttack.Forward);
				monster.isCanAttack = true;
            }
        }
        public void HandleSkill(Player player, C_Skill skillPacket)
		{
			if (player == null)
				return;

			ObjectInfo info = player.Info;
			if (info.PosInfo.State != CreatureState.Idle)
				return;

			// TODO : 스킬 사용 가능 여부 체크
			info.PosInfo.State = CreatureState.Skill;
			S_Skill skill = new S_Skill() { Info = new SkillInfo() };
			skill.ObjectId = info.ObjectId;
			skill.Info.SkillId = skillPacket.Info.SkillId;
			Broadcast(player.CellPos, skill);

			Data.Skill skillData = null;
			if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
				return;

			switch (skillData.skillType)
			{
				case SkillType.SkillAuto:
					{
						//Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
						//GameObject target = Map.Find(skillPos);
						/*if (target != null)
						{
							Console.WriteLine("Hit GameObject !");
						}*/
					}
					break;
				case SkillType.SkillProjectile:
					{
						Arrow arrow = ObjectManager.Instance.Add<Arrow>();
						if (arrow == null)
							return;

						arrow.Owner = player;
						arrow.Data = skillData;
						arrow.PosInfo.State = CreatureState.Moving;
						//arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
						//arrow.PosInfo.PosX = player.PosInfo.PosX;
						//arrow.PosInfo.PosY = player.PosInfo.PosY;
						//arrow.Speed = skillData.projectile.speed;
						Push(EnterGame, arrow, false);
					}
					break;
			}
		}

	}
}
