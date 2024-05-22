using Data;
using DG.Tweening;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : CreatureController
{
    protected NavMeshAgent _agent;
    protected Animator _anim;
    protected int attackNum = -1;
    protected override void Init()
    {
        base.Init();
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        _anim.SetBool("Death", false);
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
            case CreatureState.Skill:
                break;
            case CreatureState.Wait:
                break;
            case CreatureState.Damaged:
                break;
            case CreatureState.Dead:
                break;
        }
    }
    protected override void UpdateIdle()
    {
        if (_anim == null) return;
        _anim.SetFloat("speed", 0f);
    }
    protected override void UpdateMoving()
    {
        if (_anim == null) return;
        _anim.SetFloat("speed", _agent.speed);

    }
    public override void MoveTarget(Vector3 target, GameObject targetObj = null)
    {
        if (_agent == null) return;
        StopCoroutine(OnMove(target));
        StartCoroutine(OnMove(target));
    }
    public override void StopMove(Vector3 receivedEuler, Vector3 receivePos)
    {
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        State = CreatureState.Idle;
        if (transform.position != receivePos)
            transform.DOMove(receivePos, 0.2f);
        Quaternion targetRotation = Quaternion.Euler(receivedEuler);
        if (transform.rotation != targetRotation)
            transform.DORotate(receivedEuler, 0.1f);
    }
    IEnumerator OnMove(Vector3 target)
    {
        _agent.ResetPath();
        _agent.SetDestination(target);
        State = CreatureState.Moving;
        while (true)
        {
            if (Vector3.Distance(_agent.destination, transform.position) < 0.3f)
            {
                if(Managers.Object.MyPlayer.gameObject == gameObject)
                {
                    C_StopMove moveStopPacket = new C_StopMove() { PosInfo = new PositionInfo() };
                    moveStopPacket.PosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                    Vector3 rotationEuler = transform.rotation.eulerAngles;
                    moveStopPacket.PosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                    moveStopPacket.IsMonster = false;
                    moveStopPacket.ObjectId = Id;
                    Managers.Network.Send(moveStopPacket);
                }
                break;
            }
            yield return null;
        }
    }
    public override void OnAttack(SkillInfo info)
    {
        Skill skill = null;
        Managers.Data.SkillDict.TryGetValue(info.SkillId, out skill);
        if (skill == null) return;
        
        State = CreatureState.Skill;
        attackNum = info.SkillId;
        _anim.SetInteger("AttackNum", attackNum);
        StartCoroutine(CoAttackMotion(skill.cooldown));
        
    }
    public IEnumerator CoAttackMotion(float coolDown)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        _anim.SetInteger("AttackNum", -1);
        yield return new WaitForSecondsRealtime(coolDown - 0.2f);
        State = CreatureState.Idle;
        attackNum = -1;
    }
    public override void ChangeHp(int hp, bool isHeal, int damage)
    {
        if (isHeal)
        {
        }
        else
        {
            _anim.SetTrigger("Damage");
            // 체력바 등 작업
            Hp = hp;
            State = CreatureState.Wait;
            StartCoroutine(CoWaitForSecondsToState(0.8f, CreatureState.Idle));
            GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo");
            damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage, transform);
        }
    }
    IEnumerator CoWaitForSecondsToState(float time, CreatureState state)
    {
        yield return new WaitForSecondsRealtime(time);
        State = state;
    }
    public override void OnDead(GameObject attacker)
    {
        State = CreatureState.Dead;
        _anim.SetBool("Death", true);
        FinalAttacker = attacker;
    }
}
