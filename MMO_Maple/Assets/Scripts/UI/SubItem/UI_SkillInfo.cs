using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillInfo : UI_Base
{
    Skill skillData;
    enum Images
    {
        IconImage,
    }
    enum Texts
    {
        SkillNameText,
        SkillLevelText,
        SkillLevelUpText,
    }
    enum Buttons
    {
        SkillLevelUpBtn
    }
    bool _init = false;
    int templateId = -1;
    int skillLevel = 0;
    public override void Init()
    {
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        _init = true;

        RefreshUI();
    }
    public void Setting(int templateId)
    {
        this.templateId = templateId;
    }
    public void RefreshUI()
    {
        if (_init == false) return;
        if (Managers.Data.SkillDict.TryGetValue(templateId, out skillData) == false)
            return;

        GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>($"Textures/Skill/{skillData.name}");
        GetText((int)Texts.SkillNameText).text = skillData.name;

        if(Managers.Object.MyPlayer.HaveSkillData.TryGetValue(templateId, out skillLevel) == false)
        {
            GetText((int)Texts.SkillLevelText).text = "0";
        }
        else
        {
            GetText((int)Texts.SkillLevelText).text = skillLevel.ToString();
        }
        if(Managers.Object.MyPlayer.Stat.SkillPoint <= 0)
        {
            GetText((int)Texts.SkillLevelUpText).color = new Color(1, 1, 1, 0.5f);
            GetButton((int)Buttons.SkillLevelUpBtn).gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
    }
}
