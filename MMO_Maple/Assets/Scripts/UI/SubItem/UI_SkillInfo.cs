using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillInfo : UI_Base
{
    Skill skillData;
    GameObject description;
    enum Images
    {
        IconImage,
        LockSkillImage
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

        GetButton((int)Buttons.SkillLevelUpBtn).gameObject.BindEvent(
            e => 
            {
                if (Managers.Object.MyPlayer.Stat.SkillPoint <= 0) return;
                C_SkillLevelUp skillLevelUp = new C_SkillLevelUp() { Skill = new SkillInfo() };
                skillLevelUp.Skill.SkillId = templateId;
                skillLevelUp.Skill.Level = 1;
                Managers.Network.Send(skillLevelUp);
            });

        GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
        {
            if (skillData == null) return;
            description = Managers.Resource.Instantiate("UI/SubItem/UI_SkillInfoCanvas");
            description.GetComponent<UI_SkillInfoCanvas>().Setting(skillData, skillLevel);
        }, Define.UIEvent.PointerEnter);

        GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
        {
            if (skillData == null) return;
            Managers.Resource.Destroy(description);
        }, Define.UIEvent.PointerExit);

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
            GetImage((int)Images.LockSkillImage).color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            GetImage((int)Images.LockSkillImage).color = new Color(1, 1, 1, 0);
            GetText((int)Texts.SkillLevelText).text = skillLevel.ToString();
        }
        if(Managers.Object.MyPlayer.Stat.SkillPoint <= 0)
        {
            GetText((int)Texts.SkillLevelUpText).color = new Color(1, 1, 1, 0.5f);
            GetButton((int)Buttons.SkillLevelUpBtn).gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
    }
    public void InfoRemoveSkill()
    {
        if (description != null)
            Managers.Resource.Destroy(description);
    }
}
