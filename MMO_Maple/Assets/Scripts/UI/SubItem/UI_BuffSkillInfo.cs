using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuffSkillInfo : UI_Base
{
    public Image icon;
    public Image skillFill;
    public BuffSkill skill;
    public BuffSkillAbility ability;
    public int duration;
    public float curTime;
    Coroutine coroutine;
    public override void Init()
    {

    }

    public void Setting(BuffSkill buff, BuffSkillAbility skillAbility, int duration)
    {
        skill = buff;
        ability = skillAbility;
        this.duration = duration;
        curTime = 0;
        coroutine = StartCoroutine(CoStartSkillCool());
    }
    public IEnumerator CoStartSkillCool()
    {
        while (skill != null && curTime < duration) 
        { 
            curTime += Time.deltaTime; 
            skillFill.fillAmount = curTime / duration;
            yield return new WaitForFixedUpdate();
        }
        ability.ReSetAbility(Managers.Object.MyPlayer, skill);
        (Managers.UI.SceneUI as UI_GameScene).StatUI.RefreshUI();
        (Managers.UI.SceneUI as UI_GameScene).RemoveBuffUI(skill.id);
    }
    public void Stop()
    {
        if (coroutine != null) { StopCoroutine(coroutine); }
    }
}
