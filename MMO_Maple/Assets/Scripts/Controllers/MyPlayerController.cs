using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class MyPlayerController : PlayerController
{
    Camera cm;
    public int ClassType;
    private float _moveTime = 0.5f;
    public float lookRotationSpeed = 0.5f;
    public LayerMask mosterLayerMask;
    public LayerMask groundLayerMask;
    public override int MaxHp { get { return Stat.MaxHp; } set { Stat.MaxHp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public override int MaxMp { get { return Stat.MaxMp; } set { Stat.MaxMp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public override int Hp { get { return Stat.Hp; } set { Stat.Hp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public override int Mp { get { return Stat.Mp; } set { Stat.Mp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public int Exp { get { return Stat.Exp; } protected set { Stat.Exp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeExp(); } }
    public Dictionary<int, int> HaveSkillData = new Dictionary<int, int>();
    public int BuffDamage;

    public int MaxAttack { 
        get {
            int attack = (int)((Stat.Str * 4 + Stat.Dex) * (WeaponDamage + BuffDamage) / 100);
            if(attack < 3) attack = 3;
            if (WeaponDamage == 0) attack = 0;
            return attack; 
        } 
    }
    public int MinAttack
    {
        get
        {
            int attack = (int)((Stat.Str * 4 * 0.9 * 0.1 + Stat.Dex) * (WeaponDamage + BuffDamage) / 100);
            if (attack < 1) attack = 1;
            if (WeaponDamage == 0) attack = 0;
            return attack;
        }
    }
    public bool NpcTrigger = false;
    public GameObject Body;
    protected override void Init()
    {
        base.Init();
        cm = Camera.main;
        GameObject vcm = GameObject.FindWithTag("CM");
        if (vcm == null)
            cm.GetComponent<CameraController>().SettingPlayer(this);
        else
            vcm.GetComponent<CameraController>().SettingPlayer(this);
        PrevPos = transform.position;
#if UNITY_SERVER

#else
        StopCoroutine(CheckPosInfo());
        StartCoroutine(CheckPosInfo());
#endif
    }
    protected override void Update()
    {
        if (NpcTrigger) return;
        OnClickMouseInputEvent();
        KeyInputEvent();
        base.Update();
    }
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    private void KeyInputEvent()
    {
        // 스킬
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (curRightWeapon == null) return;
            if (State != CreatureState.Idle || State == CreatureState.Moving || State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeSkillQuickSlot("Q");
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (curRightWeapon == null) return;
            if (State != CreatureState.Idle || State == CreatureState.Moving || State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeSkillQuickSlot("W");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (curRightWeapon == null) return;
            if (State != CreatureState.Idle || State == CreatureState.Moving || State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeSkillQuickSlot("E");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (curRightWeapon == null) return;
            if (State != CreatureState.Idle || State == CreatureState.Moving || State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeSkillQuickSlot("R");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeItemQuickSlot("1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeItemQuickSlot("2");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeItemQuickSlot("3");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (State == CreatureState.Dead || State == CreatureState.Wait) return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvokeItemQuickSlot("4");
        }
        // UI
        else if(Input.GetKeyDown(KeyCode.I))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

            if (invenUI.gameObject.activeSelf)
            {
                gameSceneUI.CloseUI("UI_Inventory");
            }
            else
            {
                gameSceneUI.OpenUI("Inven");
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Stat statUI = gameSceneUI.StatUI;

            if (statUI.gameObject.activeSelf)
            {
                gameSceneUI.CloseUI("UI_Stat");
            }
            else
            {
                gameSceneUI.OpenUI("Stat");
            }
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Equip equipUI = gameSceneUI.EquipUI;

            if (equipUI.gameObject.activeSelf)
            {
                gameSceneUI.CloseUI("UI_Equip");
            }
            else
            {
                gameSceneUI.OpenUI("Equip");
            }
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Skill skillUI = gameSceneUI.SkillUI;

            if (skillUI.gameObject.activeSelf)
            {
                gameSceneUI.CloseUI("UI_Skill");
            }
            else
            {
                gameSceneUI.OpenUI("Skill");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.CloseUI();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            FindCloseMob();
        }
        
    }
    private void FindCloseMob()
    {
        Collider[] colliders;
        GameObject enemy = null;
        colliders = Physics.OverlapSphere(transform.position + new Vector3(0f, 0.8f, 0f), 2f, mosterLayerMask, QueryTriggerInteraction.Collide);
        if (colliders.Length > 0)
        {
            float short_distance = 1000f;
            foreach (Collider col in colliders)
            {
                float short_distance2 = Vector3.Distance(transform.position, col.transform.position);
                if (short_distance > short_distance2)
                {
                    short_distance = short_distance2;
                    enemy = col.gameObject;
                }
            }
        }
        if (enemy != null)
        {
            transform.LookAt(enemy.transform);
            MakePosPacket();
        }
    }
    public void OnClickMouseInputEvent()
    {
        if (State == CreatureState.Dead) return;

        _moveTime += Time.deltaTime;
        if (Input.GetMouseButton(1) && _moveTime >= 0.3f)
        {
            if (IsPointerOverUIObject()) return;

            if (State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            Ray ray = cm.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayerMask))
            {
                transform.DOLookAt(hit.point, lookRotationSpeed, AxisConstraint.Y);
                // TODO : 이동 패킷
                //MoveTarget(hit.point);
                C_Move movePacket = new C_Move() { PosInfo = new PositionInfo() { Pos = new Positions() } };
                movePacket.PosInfo.State = CreatureState.Moving;
                Positions pos = new Positions() { PosX = hit.point.x, PosY = hit.point.y, PosZ = hit.point.z };
                movePacket.PosInfo.Pos = pos;
                Managers.Network.Send(movePacket);
                _moveTime = 0;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (curRightWeapon == null) return;
            if (IsPointerOverUIObject()) return;

            if (State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            // idle 상태인지 검증 -> 아니라면 멈춤 패킷 보냈다가 공격 패킷 보냄
            if (State != CreatureState.Idle && State == CreatureState.Moving)
            {
                C_StopMove moveStopPacket = new C_StopMove() { PosInfo = new PositionInfo() };
                moveStopPacket.PosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                Vector3 rotationEuler = transform.rotation.eulerAngles;
                moveStopPacket.PosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                Managers.Network.Send(moveStopPacket);
                return;
            }
            int attackRand = Random.Range(1, 3);
            if (attackRand < 0 || attackRand > 2) 
            {
                attackNum = -1; 
                return;
            }
            else
            {
                // 스킬 모션 발생
                Skill skill = null;
                if (Managers.Data.SkillDict.TryGetValue(attackRand, out skill) == false) return;
                C_SkillMotion skillMotion = new C_SkillMotion() { Info = new SkillInfo() };
                skillMotion.ObjectId = Id;
                skillMotion.Info.SkillId = attackRand;
                skillMotion.IsMonster = false;
                Managers.Network.Send(skillMotion);
                State = CreatureState.Wait;
                StartCoroutine(CoAttackTimeWait(skill, skill.isContinual));
            }
        }
    }
    public override IEnumerator CheckPosInfo() {
        while (true)
        {
            var offset = transform.position - PrevPos;
            if (offset.sqrMagnitude > 0.01f)
            {
                MakePosPacket();
                PrevPos = transform.position;
            }
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
    public void MakePosPacket()
    {
        C_CheckPos checkPosPacket = new C_CheckPos() { CurPosInfo = new PositionInfo() };
        checkPosPacket.CurPosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
        Vector3 rotationEuler = transform.rotation.eulerAngles;
        checkPosPacket.CurPosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
        checkPosPacket.ObjectId = Id;
        checkPosPacket.IsMonster = false;
        Managers.Network.Send(checkPosPacket);
    }
    public IEnumerator CoAttackTimeWait(Skill skill, bool isContinual = false)
    {
        if(skill.skillType == SkillType.SkillMeleeAttack)
        {
            AttackSkill attackSkill = (AttackSkill)skill;
            for (int i = 0; i < attackSkill.skillDatas.Count; i++)
            {
                yield return new WaitForSeconds(attackSkill.skillDatas[i].attackTime);
                if (State == CreatureState.Skill)
                {
                    C_MeleeAttack meleeAttack = new C_MeleeAttack() { Info = new SkillInfo(), Forward = new Positions() };
                    meleeAttack.Info.SkillId = attackSkill.id;
                    meleeAttack.Forward = Util.Vector3ToPositions(transform.forward);
                    meleeAttack.IsMonster = false;
                    meleeAttack.ObjectId = Id;
                    if (isContinual) { meleeAttack.Time = i; }
                    else { meleeAttack.Time = 0; }
                    Managers.Network.Send(meleeAttack);
                }
            }
        }
        else if(skill.skillType == SkillType.SkillBuff)
        {
            if (CreatureState.Idle != State) yield break;
            if (HaveSkillData.TryGetValue(skill.id, out int skillLevel) == false) yield break;
            C_SkillBuff skillBuffPacket = new C_SkillBuff();
            skillBuffPacket.SkillId = skill.id;
            Managers.Network.Send(skillBuffPacket);
            BuffSkill buff = (BuffSkill)skill;
            // 버프 작업
            BuffSkillAbility ability = SkillAbilityFactory.CreateAbility(skill.id);
            ability.ApplyAbility(this, buff, skillLevel);

            Managers.UI.SceneUI.GetComponent<UI_GameScene>().RegisterBuff(buff, ability, buff.duration * skillLevel);
        }
    }
    public void RefreshAdditionalStat()
    {
        WeaponDamage = 0;
        ArmorDefence = 0;
        Item[] items = Managers.Inven.EquipItems;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;
            if (items[i].Equipped == false) continue;
            switch (items[i].ItemType)
            {
                case ItemType.Weapon:
                    WeaponDamage += ((Weapon)items[i]).Damage;
                    break;
                case ItemType.Armor:
                    ArmorDefence += ((Armor)items[i]).Defence;
                    break;
            }
        }
    }
    public void QuickAction(Skill skill)
    {
        if (Stat.Mp - skill.mpConsume < 0) { Debug.Log("마나 없음"); return; }
        C_SkillMotion skillMotion = new C_SkillMotion() { Info = new SkillInfo() };
        skillMotion.Info.SkillId = skill.id;
        Managers.Network.Send(skillMotion);
        StartCoroutine(CoAttackTimeWait(skill, skill.isContinual));
    }
    public float interactionCooldown = 1.5f; 
    private float lastInteractionTime = -1f; 
    public void OnTriggerStay(Collider other)
    {
        if (other == null) return;
        if (!other.gameObject.CompareTag("NPC")) return;
        float currentTime = Time.time;
        if (Input.GetKeyDown(KeyCode.Space) && Managers.Object.MyPlayer.State == CreatureState.Idle)
        {
            if (currentTime - lastInteractionTime < interactionCooldown)
                return;
            NPCController npcController = other.gameObject.GetComponent<NPCController>();
            lastInteractionTime = currentTime;
            if(npcController != null && NpcTrigger == false)
            {
                npcController.OpenNpc();
            }
            else if(npcController != null && NpcTrigger == true)
            {
                npcController.CloseNpc();
            }
        }
    }
}
