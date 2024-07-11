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
        hpbarUI = Managers.Resource.Instantiate("UI/Popup/UI_BossHp_Popup").GetComponent<UI_BossHp_Popup>();
        hpbarUI.Setting(gameObject);
        hpbarUI.ChangeHp(MaxHp);
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
}
