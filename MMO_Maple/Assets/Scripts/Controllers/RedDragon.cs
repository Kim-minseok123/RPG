using DG.Tweening;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDragon : MonsterController
{
    UI_BossHp_Popup hpbarUI;
    protected override void Init()
    {
      
    }
    public void Start()
    {
        hpbarUI = Managers.Resource.Instantiate("UI/Popup/UI_BossHp_Popup").GetComponent<UI_BossHp_Popup>();
        hpbarUI.Setting(gameObject);
        hpbarUI.ChangeHp(MaxHp);
        _anim = GetComponent<Animator>();
    }
    protected override void Update()
    {

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
                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo");
                damageInfo.transform.position = transform.position;
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage);
            }
            else
            {
               
                // 체력바 등 작업
                Hp = hp;
                var value = Mathf.Max(0f, (float)Hp / MaxHp);
                if (value == 0f)
                {
                    // 죽음 처리
                }
                else
                {
                    hpbarUI.ChangeHp(value);
                }

                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo");
                damageInfo.transform.position = transform.position + new Vector3(0, 3f, 0);
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage);
            }
        }
    }
    public override void OnAttack(SkillInfo info)
    {
        int actionNum = info.SkillId;
        switch (info.SkillId)
        {
            case 1:
                StartCoroutine(EffectInst("Effect/HitUIPoint", new Vector3(81, 0.1f, 48), 1f));
                StartCoroutine(EffectInst("Effect/HitUIPoint", new Vector3(97, 0.1f, 48), 1f));
                StartCoroutine(EffectInst("Effect/MagicEffect", new Vector3(81, 0f, 48), 2f, 1.2f));
                StartCoroutine(EffectInst("Effect/MagicEffect", new Vector3(97, 0f, 48), 2f, 1.2f));
                break;                                                                 
            case 2:
                StartCoroutine(EffectInst("Effect/HitUIPoint", new Vector3(89, 0.1f, 48), 1f));
                StartCoroutine(EffectInst("Effect/HitUIPoint", new Vector3(89, 0.1f, 56), 1f));
                break;                                                                 
            case 3:
            case 4:
                StartCoroutine(EffectInst("Effect/HitUIPoint", new Vector3(81, 0.1f, 40), 1f));
                StartCoroutine(EffectInst("Effect/HitUIPoint", new Vector3(89, 0.1f, 40), 1f));
                StartCoroutine(EffectInst("Effect/HitUIPoint", new Vector3(97, 0.1f, 40), 1f));
                break;
        }
        StartCoroutine(CoChagneAnimNum(actionNum.ToString()));
    }
    public IEnumerator CoChagneAnimNum(string actionName)
    {
        if(actionName != "0")
            yield return new WaitForSeconds(1f);
        _anim.SetTrigger(actionName);
    }
    public IEnumerator EffectInst(string path, Vector3 pos, float time, float time2 = 0)
    {
        yield return new WaitForSeconds(time2);

        GameObject go = Managers.Resource.Instantiate(path);
        go.transform.position = pos;
        yield return new WaitForSeconds(time);
        Managers.Resource.Destroy(go);
    }
    public void DamageSkill(AnimationEvent myEvent)
    {
        if (Managers.Object.MyPlayer.isMaster == false) return;
        C_SkillAction skillActionPacket = new C_SkillAction();
        skillActionPacket.Time = myEvent.intParameter;
        if(myEvent.floatParameter == 1)
            skillActionPacket.IsEnd = true;
        else 
            skillActionPacket.IsEnd = false;
        skillActionPacket.ObjectId = Id;
        Managers.Network.Send(skillActionPacket);
    }
}
