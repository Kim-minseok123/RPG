using Data;
using DG.Tweening;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MonsterController : CreatureController
{
    protected NavMeshAgent _agent;
    protected Animator _anim;
    public GameObject TargetObj;
    public Slider hpBar;
    protected bool isAttackMotion = false;

    protected override void Init()
    {
        base.Init();
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        hpBar.gameObject.SetActive(true);
        StartCoroutine(HpSet());
        _anim.SetBool("Death", false);
        State = CreatureState.Idle;
#if UNITY_SERVER
        PrevPos = transform.position;
        StopCoroutine(CheckPosInfo());
        StartCoroutine(CheckPosInfo());
#endif
    }
    IEnumerator HpSet()
    {
        while (Hp <= 0)
        {
            hpBar.value = Mathf.Max(0f, (float)Hp / MaxHp);
            yield return null;
        }
        hpBar.value = Mathf.Max(0f, (float)Hp / MaxHp);

    }
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
        if (isAttackMotion) { _anim.SetFloat("speed", 0); return; }
        _anim.SetFloat("speed", _agent.speed);
    }
    public override void MoveTarget(Vector3 target, GameObject targetObj = null)
    {
        if (isAttackMotion) 
        {
#if UNITY_SERVER
#endif
            return; 
        }
        TargetObj = targetObj;
        if (_agent == null)
        {
#if UNITY_SERVER
            Debug.Log("2");
#endif
            return;
        }
        StopCoroutine(OnMove(target));
        StartCoroutine(OnMove(target));
    }
    public override void StopMove(Vector3 receivedEuler, Vector3 receivePos)
    {
        if (_agent == null) return;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        State = CreatureState.Idle;
        if (transform.position != receivePos)
            transform.DOMove(receivePos, 0.2f);
        Quaternion targetRotation = Quaternion.Euler(receivedEuler);
        if (transform.rotation != targetRotation)
            transform.DORotate(receivedEuler, 0.1f);
    }
    public virtual IEnumerator OnMove(Vector3 target)
    {
        _agent.ResetPath();
        _agent.SetDestination(target);
        State = CreatureState.Moving;
#if UNITY_SERVER
        while (true)
        {
            if(TargetObj == null)
            {
                if (Vector3.Distance(_agent.destination, transform.position) < 0.3f)
                {
                    C_StopMove moveStopPacket = new C_StopMove() { PosInfo = new PositionInfo() };
                    moveStopPacket.PosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                    Vector3 rotationEuler = transform.rotation.eulerAngles;
                    moveStopPacket.PosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                    moveStopPacket.IsMonster = true;
                    moveStopPacket.ObjectId = Id;
                    Managers.Network.Send(moveStopPacket);
                    break;
                }
            }
            
            yield return null;
        }
#endif
        yield return null;
    }
#if UNITY_SERVER
    public override IEnumerator CheckPosInfo()
    {
        while (true)
        {
            var offset = transform.position - PrevPos;
            if (offset.sqrMagnitude > 0.01f)
            {
                C_CheckPos checkPosPacket = new C_CheckPos() { CurPosInfo = new PositionInfo() };
                checkPosPacket.CurPosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                Vector3 rotationEuler = transform.rotation.eulerAngles;
                checkPosPacket.CurPosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                checkPosPacket.ObjectId = Id;
                checkPosPacket.IsMonster = true;
                Managers.Network.Send(checkPosPacket);
                PrevPos = transform.position;
            }
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
#endif
    public override void ChangeHp(int hp, bool isHeal, int damage)
    {
        if (isHeal)
        {
        }
        else
        {
            if(damage <= 0)
            {
                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo");
                damageInfo.transform.position = transform.position;
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage);
            }
            else
            {
                State = CreatureState.Damaged;
                _anim.SetTrigger("Damage");
                EffectInst("HitEffect", 1.0f, transform.position + new Vector3(0f, 0.1f, 0f), new Vector3(0.1f, 0.1f, 0.1f));
                // ü�¹� �� �۾�
                Hp = hp;
                var value = Mathf.Max(0f, (float)Hp / MaxHp);
                if (value == 0f)
                    hpBar.gameObject.SetActive(false);
                else
                    hpBar.DOValue(value, 0.5f).SetEase(Ease.OutExpo);

                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo");
                damageInfo.transform.position = transform.position;
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage);
            }
        }
    }
    public override void OnDead(GameObject attacker)
    {
        State = CreatureState.Dead;
        _anim.SetBool("Death", true);
        FinalAttacker = attacker;
    }

}
