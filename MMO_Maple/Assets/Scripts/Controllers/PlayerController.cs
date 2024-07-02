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
    public GameObject RightHand;
    public GameObject LeftHand;
    public GameObject Head;
    protected GameObject curRightWeapon;
    GameObject curLeftWeapon;
    GameObject curHeadItem;
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
        if(skill.skillType == SkillType.SkillBuff) 
        {
            StartMotionOrEffect(SkillDescription.GetSkillNameKorToEng(skill.name));
        }
        else
        {
            State = CreatureState.Skill;
            attackNum = info.SkillId;
            _anim.SetInteger("AttackNum", attackNum);
            StartCoroutine(CoAttackMotion(skill.cooldown));
        }
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
            if (damage <= 0)
            {
                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo", transform);
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage, transform);
            }
            else
            {
                _anim.SetTrigger("Damage");
                EffectInst("HitEffect", 1.0f, transform.position + new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 1f));
                // 체력바 등 작업
                Hp = hp;
                State = CreatureState.Wait;
                if (hp > 0)
                    StartCoroutine(CoWaitForSecondsToState(0.8f, CreatureState.Idle));
                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo", transform);
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage, transform);
            }
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
    public void EquipItem(int id)
    {
        ItemData data = null;
        if (Managers.Data.ItemDict.TryGetValue(id, out data) == false) return;

        switch (data.itemType)
        {
            case ItemType.Weapon:
                WeaponData weapon = (WeaponData)data;
                if(weapon.weaponType == WeaponType.Assistance)
                {
                    if (curLeftWeapon != null) Managers.Resource.Destroy(curLeftWeapon);
                    curLeftWeapon = Managers.Resource.Instantiate($"Item/{data.name}", LeftHand.transform);
                }
                else
                {
                    if (curRightWeapon != null) Managers.Resource.Destroy(curRightWeapon);
                    curRightWeapon = Managers.Resource.Instantiate($"Item/{data.name}", RightHand.transform);
                    if (data.name == "낡은 검")
                    {
                        curRightWeapon.transform.SetLocalPositionAndRotation(new Vector3(0,0,0), Quaternion.identity);
                    }
                }
                break;
            case ItemType.Armor:
                ArmorData armor = (ArmorData)data;
                if(armor.armorType == ArmorType.Helmet)
                {
                    if (curHeadItem != null) Managers.Resource.Destroy(curHeadItem);
                    curHeadItem = Managers.Resource.Instantiate($"Item/{data.name}", Head.transform);
                }
                else
                {

                }
                break;
        }
        
    }
    public void Disarm(int index)
    {
        if (index < 0) return;
        switch (index)
        {
            case 1:
                if (curHeadItem != null) Managers.Resource.Destroy(curHeadItem);
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                if (curRightWeapon != null) Managers.Resource.Destroy(curRightWeapon);
                break;
            case 7:
                break;
            case 8:
                if (curLeftWeapon != null) Managers.Resource.Destroy(curLeftWeapon);
                break;
        }
    }
    public void StartMotionOrEffect(string actionName)
    {
        switch (actionName) 
        {
            case "Drop":
                PickUpItemMotion();
                break;
            case "LevelUp":
                LevelUpEffect();
                break;
            case "Anger":
                State = CreatureState.Wait;
                _anim.SetTrigger("LevelUp");
                EffectInst("AngerEffect", 1.5f, transform.position, new Vector3(1f, 1f, 1f));
                StartCoroutine(CoWaitForSecondsToState(1.5f, CreatureState.Idle));
                break;
            default:
                Debug.Log("Not Exist Action : " + actionName);
                break;
        }
    }
    public void PickUpItemMotion()
    {
        State = CreatureState.Wait;
        _anim.SetTrigger("PickUp");
        StartCoroutine(CoWaitForSecondsToState(1.5f, CreatureState.Idle));
    }
    public void LevelUpEffect()
    {
        State = CreatureState.Wait;
        _anim.SetTrigger("LevelUp");
        EffectInst("LevelUpEffect", 1.2f, transform.position, new Vector3(1f, 1f, 1f));
        StartCoroutine(CoWaitForSecondsToState(1.2f, CreatureState.Idle));
    }

}   
