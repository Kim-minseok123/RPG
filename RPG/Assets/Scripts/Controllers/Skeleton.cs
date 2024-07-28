using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonsterController
{
    protected override void Init()
    {
        base.Init();
        isAttackMotion = false;
    }

    public override void OnAttack(SkillInfo info)
    {
        if (State == CreatureState.Dead) return;

        Skill skill = null;
        if (Managers.Data.SkillDict.TryGetValue(info.SkillId, out skill) == false) return;
        transform.LookAt(TargetObj.transform);
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        State = CreatureState.Skill;
        _anim.SetTrigger("Attack");
        StartCoroutine(CoAttackPacket(skill));
    }

    public IEnumerator CoAttackPacket(Skill skill)
    {
        AttackSkill attackSkill = skill as AttackSkill;
        yield return new WaitForSeconds(attackSkill.skillDatas[0].attackTime);
        if (State == CreatureState.Dead) yield break;
#if UNITY_SERVER
        if(State == CreatureState.Skill)
        {
            C_MeleeAttack meleeAttack = new C_MeleeAttack() { Info = new SkillInfo(), Forward = new Positions() };
            meleeAttack.Info.SkillId = attackSkill.id;
            meleeAttack.Forward = Util.Vector3ToPositions(transform.forward);
            meleeAttack.IsMonster = true;
            meleeAttack.Time = 0;
            meleeAttack.ObjectId = Id;
            Managers.Network.Send(meleeAttack);
        }
#endif
        yield return new WaitForSeconds(skill.cooldown - (int)attackSkill.skillDatas[0].attackTime);
        if (State == CreatureState.Dead) yield break;
        State = CreatureState.Idle;
        isAttackMotion = false;
    }

    public override IEnumerator OnMove(Vector3 target)
    {
        State = CreatureState.Moving;

        if (State == CreatureState.Dead) 
        {
#if UNITY_SERVER
            Debug.Log("3");
#endif
            yield break; 
        }

        if (isAttackMotion) 
        {
#if UNITY_SERVER
            Debug.Log("4");
#endif
            yield break;
        }
        _agent.ResetPath();
        if (TargetObj == null || (TargetObj != null && Vector3.Distance(target, transform.position) >= 1.2f))
            _agent.SetDestination(target);
        State = CreatureState.Moving;
        int cnt = 0;
        while (true)
        {
            cnt++;
            if (TargetObj == null)
            {
                // || _agent.isStopped
                if (Vector3.Distance(_agent.destination, transform.position) < 0.3f || (cnt >= 100 && _agent.velocity.sqrMagnitude <= 0.1 * 0.1))
                {
#if UNITY_SERVER
                    C_StopMove moveStopPacket = new C_StopMove() { PosInfo = new PositionInfo() };
                    moveStopPacket.PosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                    Vector3 rotationEuler = transform.rotation.eulerAngles;
                    moveStopPacket.PosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                    moveStopPacket.IsMonster = true;
                    moveStopPacket.ObjectId = Id;
                    Managers.Network.Send(moveStopPacket);
                    break;
#else
                    break;
#endif
                }
            }
            else
            {
                if (Vector3.Distance(_agent.destination, transform.position) < 1.2f)
                {
#if UNITY_SERVER
                    transform.LookAt(TargetObj.transform);
                    C_SkillMotion skillMotion = new C_SkillMotion() { Info = new SkillInfo() };
                    skillMotion.ObjectId = Id;
                    skillMotion.Info.SkillId = 4;
                    skillMotion.IsMonster = true;
                    Managers.Network.Send(skillMotion);
                    isAttackMotion = true;
                    break;
#else
                    isAttackMotion = true;
                    break;
#endif
                }
            }
            yield return null;
        }

        yield return null;
    }
}
