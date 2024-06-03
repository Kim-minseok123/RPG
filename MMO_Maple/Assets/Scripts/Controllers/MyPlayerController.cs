using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class MyPlayerController : PlayerController
{
    Camera cm;
    public int ClassType;
    private float _moveTime = 0.5f;
    public LayerMask layerMask;
    public override int MaxHp { get { return Stat.MaxHp; } protected set { Stat.MaxHp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public override int MaxMp { get { return Stat.MaxMp; } protected set { Stat.MaxMp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public override int Hp { get { return Stat.Hp; } protected set { Stat.Hp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public override int Mp { get { return Stat.Mp; } protected set { Stat.Mp = value; Managers.UI.SceneUI.GetComponent<UI_GameScene>().ChangeHpOrMp(); } }
    public int MaxAttack { 
        get {
            int damage = (Stat.Str * 4 + Stat.Dex) * WeaponDamage / 100;
            if (damage < 3)
                return 3;
            else
                return damage; 
        } 
    }
    public int MinAttack { 
        get {
            int damage = (int)((Stat.Str * 4 * 0.9 * 0.1 + Stat.Dex) * WeaponDamage / 100);
            if (damage < 1)
                return 1;
            else
                return damage;
        } 
    }

    protected override void Init()
    {
        base.Init();
        cm = Camera.main;
        cm.GetComponent<CameraController>().SettingPlayer(this);
        PrevPos = transform.position;
#if UNITY_SERVER

#else
        StopCoroutine(CheckPosInfo());
        StartCoroutine(CheckPosInfo());
#endif
    }
    protected override void Update()
    {
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
            if (State != CreatureState.Idle || State == CreatureState.Moving || State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            Skill skill = null;
            if (Managers.Data.SkillDict.TryGetValue(3, out skill) == false) return;
            C_SkillMotion skillMotion = new C_SkillMotion() { Info = new SkillInfo() };
            skillMotion.Info.SkillId = skill.id;
            Managers.Network.Send(skillMotion);
            StartCoroutine(CoAttackTimeWait(skill, true));
        }
        // UI
        else if(Input.GetKeyDown(KeyCode.I))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

            if (invenUI.gameObject.activeSelf)
            {
                gameSceneUI.CloseUI("Inven");
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
                gameSceneUI.CloseUI("Stat");
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
                gameSceneUI.CloseUI("Equip");
            }
            else
            {
                gameSceneUI.OpenUI("Equip");
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
        colliders = Physics.OverlapSphere(transform.position, 5f, layerMask);
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
        Debug.Log(enemy);
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
        if (IsPointerOverUIObject()) return;
        if (Input.GetMouseButton(1) && _moveTime >= 0.3f)
        {
            if (State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            Ray ray = cm.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
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
                StartCoroutine(CoAttackTimeWait(skill));
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
        for (int i = 0; i < skill.skillDatas.Count; i++)
        {
            yield return new WaitForSeconds(skill.skillDatas[i].attackTime);
            if(State == CreatureState.Skill)
            {
                C_MeleeAttack meleeAttack = new C_MeleeAttack() { Info = new SkillInfo(), Forward = new Positions() };
                meleeAttack.Info.SkillId = skill.id;
                meleeAttack.Forward = Util.Vector3ToPositions(transform.forward);
                meleeAttack.IsMonster = false;
                meleeAttack.ObjectId = Id;
                if (isContinual) { meleeAttack.Time = i; }
                else { meleeAttack.Time = 0; }
                Managers.Network.Send(meleeAttack);
            }
        }
    }
}
